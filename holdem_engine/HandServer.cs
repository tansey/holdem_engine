using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerHandHistory;

namespace holdem_engine
{
    /// <summary>
    /// Plays a hand of poker between a list of players. Designed to be restorable, non-blocking, and for use by web servers.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    public class HandServer
    {
        BetManager _betManager;
        PotManager _potManager;
        HandHistory _history;
        CircularList<int> _playerIndices;
        Seat[] _seats;
        int _buttonIdx;
        int _utgIdx;
        int _bbIdx;
        CachedHand _cache;

		public IEnumerable<Action> ValidNextActions { get; set;	}

        public HandServer()
        {
        }

        public HandHistory Resume(PokerHand savedHand, CachedHand cache)
        {
            _cache = cache;

            #region Restore hand context
            _seats = new Seat[savedHand.Players.Length];
			var orderedPlayers = savedHand.Players.OrderBy(p => p.Seat);
            for (int i = 0; i < _seats.Length; i++)
			{
				var player = orderedPlayers.ElementAt(i);
                _seats[i] = new Seat(player.Seat, player.Name, (double)player.Stack);
			}

            ulong handNum = ulong.Parse(savedHand.Context.ID);
            uint button = (uint)savedHand.Context.Button;

            List<double> blinds = new List<double>();
            if (savedHand.Context.SmallBlind > 0m)
                blinds.Add((double)savedHand.Context.SmallBlind);
            if (savedHand.Context.BigBlind > 0m)
                blinds.Add((double)savedHand.Context.BigBlind);
            double ante = (double)savedHand.Context.Ante;
            BettingStructure bs = BettingStructure.None;
            switch (savedHand.Context.BettingType)
	        {
		        case BettingType.FixedLimit: bs = BettingStructure.Limit; break;
                case BettingType.NoLimit: bs = BettingStructure.NoLimit; break;
                case BettingType.PotLimit: bs = BettingStructure.PotLimit; break;
                default: throw new Exception("Unspecified betting structure.");
	        }
            _history = new HandHistory(_seats, handNum, button, blinds.ToArray(), ante, bs);
            #endregion

            //Create a new map from player names to player chips for the BetManager
            Dictionary<string, double> namesToChips = new Dictionary<string, double>();

            //Create a new list of players for the PlayerManager
            _playerIndices = new CircularList<int>();
            _playerIndices.Loop = true;

            // Initialize the names to chips map and find the index of the button
            for (int i = 0; i < _seats.Length; i++)
            {
                namesToChips[_seats[i].Name] = _seats[i].Chips;
                if (_seats[i].SeatNumber == _history.Button)
                {
                    _buttonIdx = i;
                    _utgIdx = (i + 1) % _seats.Length;
                }
            }

            // Create a circular list of players, in order of first to act
            for (int i = (_buttonIdx + 1) % _seats.Length; _playerIndices.Count < _seats.Length; )
            {
                _playerIndices.Add(i);
                i = (i + 1) % _seats.Length;
            }

            _betManager = new BetManager(namesToChips, _history.BettingStructure, _history.AllBlinds, _history.Ante);
            _potManager = new PotManager(_seats);

            _history.CurrentRound = Round.Predeal;
			if(savedHand.Blinds == null)
				savedHand.Blinds = new Blind[0];
            if (!restoreBlinds(savedHand))
                return _history;
            
            DealHoleCards();

            if (_betManager.In <= 1)
            {
                _history.CurrentRound = Round.Over;
                return _history;
            }

            _history.CurrentRound = Round.Preflop;

            if (_betManager.CanStillBet > 1)
            {
                if(savedHand.Rounds == null || savedHand.Rounds.Length == 0)
					savedHand.Rounds = new PokerHandHistory.Round[] { new PokerHandHistory.Round(){Actions = new PokerHandHistory.Action[0]} };
				else if(savedHand.Rounds[0].Actions == null)
					savedHand.Rounds[0].Actions = new PokerHandHistory.Action[0];
    			if (!restoreBets(savedHand.Rounds[0].Actions, _history.PreflopActions))
                    return _history;
            }
            if (_betManager.In <= 1)
            {
                payWinners();
                _history.CurrentRound = Round.Over;
                return _history;
            }

            DealFlop();
            _history.CurrentRound = Round.Flop;

            if (_betManager.CanStillBet > 1)
            {
                if (savedHand.Rounds.Length < 2)
					savedHand.Rounds = new PokerHandHistory.Round[]{savedHand.Rounds[0], new PokerHandHistory.Round(){Actions = new PokerHandHistory.Action[0]}};
				else if(savedHand.Rounds[1].Actions == null)
					savedHand.Rounds[1].Actions = new PokerHandHistory.Action[0];

                if (!restoreBets(savedHand.Rounds[1].Actions, _history.FlopActions))
                    return _history;
            }
            if (_betManager.In <= 1)
            {
                payWinners();
                _history.CurrentRound = Round.Over;
                return _history;
            }

            DealTurn();
            _history.CurrentRound = Round.Turn;

            if (_betManager.CanStillBet > 1)
            {
                if (savedHand.Rounds.Length < 3)
					savedHand.Rounds = new PokerHandHistory.Round[]{savedHand.Rounds[0], savedHand.Rounds[1], new PokerHandHistory.Round(){Actions = new PokerHandHistory.Action[0]}};
				else if(savedHand.Rounds[2].Actions == null)
					savedHand.Rounds[2].Actions = new PokerHandHistory.Action[0];

                if (!restoreBets(savedHand.Rounds[2].Actions, _history.TurnActions))
                    return _history;
            }
            if (_betManager.In <= 1)
            {
                payWinners();
                _history.CurrentRound = Round.Over;
                return _history;
            }

            DealRiver();
            _history.CurrentRound = Round.River;

            if (_betManager.CanStillBet > 1)
            {
                if (savedHand.Rounds.Length < 4)
					savedHand.Rounds = new PokerHandHistory.Round[]{savedHand.Rounds[0], savedHand.Rounds[1], savedHand.Rounds[2], new PokerHandHistory.Round(){Actions = new PokerHandHistory.Action[0]}};
				else if(savedHand.Rounds[3].Actions == null)
					savedHand.Rounds[3].Actions = new PokerHandHistory.Action[0];

                if (!restoreBets(savedHand.Rounds[3].Actions, _history.RiverActions))
                    return _history;
            }
            if (_betManager.In <= 1)
            {
                payWinners();
                _history.CurrentRound = Round.Over;
                return _history;
            }

            payWinners();
            _history.ShowDown = true;
            _history.CurrentRound = Round.Over;
            return _history;
        }

        private void payWinners()
        {
            uint[] strengths = new uint[_seats.Length];
            for (int i = 0; i < strengths.Length; i++)
                if (!_history.Folded[i])
                    strengths[i] = HoldemHand.Hand.Evaluate(_history.HoleCards[i] | _history.Board, 7);

            List<Winner> winners = _potManager.GetWinners(strengths);
            _history.Winners = winners;
        }

        bool restoreBets(PokerHandHistory.Action[] savedActions, List<Action> curRoundActions)
        {
            bool roundOver = false;

            int pIdx = GetFirstToAct(_history.CurrentRound == Round.Preflop);
            var aIdx = 0;
            _history.Hero = pIdx;

            //keep getting bets until the round is over
            while (!roundOver && aIdx < savedActions.Length)
            {
                _history.CurrentBetLevel = _betManager.BetLevel;
                _history.Pot = _potManager.Total;
                _history.Hero = pIdx;

                //get the next player's action
                var a = savedActions[aIdx];

                if (a.Player != _seats[pIdx].Name)
                    throw new Exception("Action list not aligned with restored state.");

                var atype = Action.ActionTypes.None;
                switch (a.Type)
	            {
		            case ActionType.Bet: atype = Action.ActionTypes.Bet; break;
                    case ActionType.Call: atype = Action.ActionTypes.Call; break;
                    case ActionType.Check: atype = Action.ActionTypes.Check; break;
                    case ActionType.Fold: atype = Action.ActionTypes.Fold; break;
                    case ActionType.Raise: atype = Action.ActionTypes.Raise; break;
                    case ActionType.Returned: atype = Action.ActionTypes.None; break;
	            }

                AddAction(pIdx, new Action(a.Player, atype, (double)a.Amount, a.AllIn), curRoundActions);

                roundOver = _betManager.RoundOver;

                if (!roundOver)
                    pIdx = _playerIndices.Next;

                aIdx++;
            }

            if (savedActions.Length != aIdx)
                throw new Exception("Actions left after round over.");

            if (!roundOver)
			{
                List<Action> validActions = new List<Action>();
				var name = _seats[pIdx].Name;
				Action fold = new Action(name, Action.ActionTypes.Fold);
				fold = _betManager.GetValidatedAction(fold);
				validActions.Add(fold);//may be check or fold
				if(fold.ActionType == Action.ActionTypes.Fold)
				{
					Action call = new Action(name, Action.ActionTypes.Call);
					call = _betManager.GetValidatedAction(call);
					validActions.Add(call);
				}
				Action minRaise = new Action(name, Action.ActionTypes.Raise, 0);
				minRaise = _betManager.GetValidatedAction(minRaise);
				if(minRaise.ActionType == Action.ActionTypes.Bet || minRaise.ActionType == Action.ActionTypes.Raise)
				{
					validActions.Add(minRaise);
					// In no-limit and pot-limit, we return the valid raises as a pair of
					// (min, max) bets.
					if(!minRaise.AllIn && _history.BettingStructure != BettingStructure.Limit)
					{
						Action maxRaise = new Action(name, Action.ActionTypes.Raise, _seats[pIdx].Chips);
						maxRaise = _betManager.GetValidatedAction(maxRaise);
						if(maxRaise.Amount > minRaise.Amount)
							validActions.Add(maxRaise);
					}
				}
				ValidNextActions = validActions;
				return false;
			}

            return true;
        }

        private int GetFirstToAct(bool preflop)
        {
            int desired = ((preflop ? _bbIdx : _buttonIdx) + 1) % _seats.Length;
            while (!_playerIndices.Contains(desired)) { desired = (desired + 1) % _seats.Length; }
            while (_playerIndices.Next != desired) { }

            return desired;
        }

        bool restoreBlinds(PokerHand savedHand)
        {
            if (_history.Ante > 0)
                for(int i = _utgIdx, count = 0; count < _seats.Length; i = (i+1)%_seats.Length, count++)
                {
                    var ante = savedHand.Blinds.FirstOrDefault(b => b.Type == BlindType.Ante && b.Player == _seats[i].Name);
                    if (ante != null)
                        AddAction(i, new Action(_seats[i].Name, Action.ActionTypes.PostAnte, _history.Ante), _history.PredealActions);
                    else
					{
					 	var nextAction = new Action(_seats[i].Name, Action.ActionTypes.PostAnte, _history.Ante);
						nextAction = _betManager.GetValidatedAction(nextAction);
						ValidNextActions = new Action[]{nextAction};
						return false;
					}
                }
            _bbIdx = _playerIndices.Next;

            // If there is no small blind, the big blind is the utg player, otherwise they're utg+1
            if (_history.SmallBlind > 0)
            {
                // If there was an ante and the small blind was put all-in, they can't post the small blind
                if (_playerIndices.Contains(_utgIdx))
                {
                    var sb = savedHand.Blinds.FirstOrDefault(b => b.Type == BlindType.SmallBlind && b.Player == _seats[_bbIdx].Name);
                    if (sb != null)
                        AddAction(_bbIdx,
                              new Action(_seats[_bbIdx].Name, Action.ActionTypes.PostSmallBlind, _history.SmallBlind),
                              _history.PredealActions);
                    else
					{
						var nextAction = new Action(_seats[_bbIdx].Name, Action.ActionTypes.PostSmallBlind, _history.SmallBlind);
						nextAction = _betManager.GetValidatedAction(nextAction);
						ValidNextActions = new Action[]{nextAction};
						return false;
					}
                }
                _bbIdx = _playerIndices.Next;
            }

            if (_history.BigBlind > 0 && _playerIndices.Contains(_bbIdx))
            {
                var bb = savedHand.Blinds.FirstOrDefault(b => b.Type == BlindType.BigBlind && b.Player == _seats[_bbIdx].Name);
                if (bb != null)
                    AddAction(_bbIdx,
                              new Action(_seats[_bbIdx].Name, Action.ActionTypes.PostBigBlind, _history.BigBlind),
                              _history.PredealActions);
                else
				{
					var nextAction = new Action(_seats[_bbIdx].Name, Action.ActionTypes.PostBigBlind, _history.BigBlind);
					nextAction = _betManager.GetValidatedAction(nextAction);
					ValidNextActions = new Action[]{nextAction};
					return false;
				}
            }

            return true;
        }

        private void AddAction(int pIdx, Action action, List<Action> curRoundActions)
        {
            //Action unvalidatedAction = new Action(action.Name, action.ActionType, action.Amount, action.AllIn);
            action = _betManager.GetValidatedAction(action);
            
            _betManager.Commit(action);
            curRoundActions.Add(action);

            if (action.Amount > 0)
                _seats[pIdx].Chips -= action.Amount;

            //update the pots
            _potManager.AddAction(pIdx, action);

            if (action.ActionType == Action.ActionTypes.None)
                throw new Exception("Must have an action");

            //if the player either folded or went all-in, they can no longer
            //bet so remove them from the player pool
            if (action.ActionType == Action.ActionTypes.Fold)
            {
                _playerIndices.Remove(pIdx);
                _history.Folded[pIdx] = true;
            }
            else if (action.AllIn)
            {
                _playerIndices.Remove(pIdx);
                _history.AllIn[pIdx] = true;
            }

        }

        /// <summary>
        /// Deals out all of the players' hole cards.
        /// </summary>
        public void DealHoleCards()
        {
            for (int i = 0; i < _seats.Length; i++)
            {
                _history.HoleCards[i] = _cache.HoleCards[i];
                _history.DealtCards = _history.DealtCards | _history.HoleCards[i];
            }
        }

        public void DealFlop()
        {
            _history.Flop = _cache.Flop;
            _history.DealtCards = _history.DealtCards | _history.Flop;
        }

        public void DealTurn()
        {
            _history.Turn = _cache.Turn;
            _history.DealtCards = _history.DealtCards | _history.Turn;
        }

        public void DealRiver()
        {
            _history.River = _cache.River;
            _history.DealtCards = _history.DealtCards | _history.River;
        }
    }

    public class HandState
    {
        public PokerHand SavedHand { get; set; }
        
    }
}
