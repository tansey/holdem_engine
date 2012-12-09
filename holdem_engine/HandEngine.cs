using System;
using System.Collections.Generic;
using System.Text;
using HoldemHand;

namespace holdem_engine
{
    /// <summary>
    /// Plays of a hand of poker between a list of players. Designed for fast local use by AI agents.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    public class HandEngine 
    {
        #region Member Variables
        Seat[] _seats;
        BetManager _betManager;
        PotManager _potManager;
        HandHistory _history;
        CircularList<int> _playerIndices;
        int _buttonIdx;
        int _utgIdx;
        int _bbIdx;
        CachedHand _cache;
        #endregion

        #region Constructors
        public HandEngine()
        {
        }
        #endregion

        /// <summary>
        /// Plays a hand from the start. Note that this method will <b>not</b> resume a game from a saved hand _history.
        /// </summary>
        /// <param name="handHistory">An new hand _history with the list of players and the game parameters.</param>
        /// <param name="cachedHand">The cached deck to use.</param>
        public void PlayHand(HandHistory handHistory, CachedHand cachedHand)
        {
            _cache = cachedHand;
            PlayHand(handHistory);
        }

        /// <summary>
        /// Plays a hand from the start. Note that this method will <b>not</b> resume a game from a saved hand _history.
        /// </summary>
        /// <param name="handHistory">An new hand _history with the list of players and the game parameters.</param>
        public void PlayHand(HandHistory handHistory)
        {
			_catchUp = catchUp;

            #region Hand Setup
            _seats = handHistory.Players;
            handHistory.HoleCards = new ulong[_seats.Length];
            handHistory.DealtCards = 0UL;
            handHistory.Flop = 0UL;
            handHistory.Turn = 0UL;
            handHistory.River = 0UL;

            //Setup the hand _history
            this._history = handHistory;

            //Create a new map from player names to player chips for the BetManager
            Dictionary<string, double> namesToChips = new Dictionary<string, double>();

            //Create a new list of players for the PlayerManager
            _playerIndices = new CircularList<int>();
            _playerIndices.Loop = true;

            for (int i = 0; i < _seats.Length; i++)
            {
                namesToChips[_seats[i].Name] = _seats[i].Chips;
                if (_seats[i].SeatNumber == _history.Button)
                {
                    _buttonIdx = i;
                    _utgIdx = (i + 1) % _seats.Length;
                }
            }
            for (int i = (_buttonIdx + 1) % _seats.Length; _playerIndices.Count < _seats.Length;)
            {
                _playerIndices.Add(i);
                i = (i + 1) % _seats.Length;
            }


            _betManager = new BetManager(namesToChips, _history.BettingStructure, _history.AllBlinds, _history.Ante);
            _potManager = new PotManager(_seats);
            #endregion

            

            if (_betManager.In > 1)
            {
                GetBlinds();
                DealHoleCards();
            }
            
            _history.CurrentRound = Round.Preflop;

            if (_betManager.CanStillBet > 1)
            {
                GetBets(_history.PreflopActions);
            }
            if (_betManager.In <= 1)
            {
                payWinners();
                return;
            }

            DealFlop();
            _history.CurrentRound = Round.Flop;

            if (_betManager.CanStillBet > 1)
            {
                GetBets(_history.FlopActions);
            }
            if (_betManager.In <= 1)
            {
                payWinners();
                return;
            }
            
            DealTurn();
            _history.CurrentRound = Round.Turn;

            if (_betManager.CanStillBet > 1)
            {
                GetBets(_history.TurnActions);
            }
            if (_betManager.In <= 1)
            {
                payWinners();
                return;
            }

            DealRiver();
            _history.CurrentRound = Round.River;

            if (_betManager.CanStillBet > 1)
            {
                GetBets(_history.RiverActions);
            }
            if (_betManager.In <= 1)
            {
                payWinners();
                return;
            }

            payWinners();
            _history.ShowDown = true;
            _history.CurrentRound = Round.Over;
        }

        private void payWinners()
        {
            uint[] strengths = new uint[_seats.Length];
            for (int i = 0; i < strengths.Length; i++)
                if(!_history.Folded[i])
                    strengths[i] = HoldemHand.Hand.Evaluate(_history.HoleCards[i] | _history.Board, 7);
            
            List<Winner> winners = _potManager.GetWinners(strengths);
            _history.Winners = winners;
        }

        /// <summary>
        /// Gets the bets from all the players still in the hand.
        /// </summary>
        public void GetBets(List<Action> curRoundActions)
        {
            bool roundOver = false;
            
            int pIdx = GetFirstToAct(_history.CurrentRound == Round.Preflop);
            
            //keep getting bets until the round is over
            while (!roundOver)
            {
                _history.CurrentBetLevel = _betManager.BetLevel;
                _history.Pot = _potManager.Total;
                _history.Hero = pIdx;
                
                //get the next player's action
                Action.ActionTypes actionType; double amount;
                _seats[pIdx].Brain.GetAction(_history, out actionType, out amount);

                AddAction(pIdx, new Action(_seats[pIdx].Name, actionType, amount), curRoundActions);

                roundOver = _betManager.RoundOver;

				if(!roundOver)
                	pIdx = _playerIndices.Next;
            }
            
        }

        private int GetFirstToAct(bool preflop)
        {
            int desired = ((preflop ? _bbIdx : _buttonIdx) + 1) % _seats.Length;
            while (!_playerIndices.Contains(desired)) { desired = (desired + 1) % _seats.Length; }
            while(_playerIndices.Next != desired){}

            return desired;
        }

        private void AddAction(int pIdx, Action action, List<Action> curRoundActions)
        {
            action = betManager.GetValidatedAction(action);
            
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
        /// Forces players to post blinds before the hand can start.
        /// </summary>
        public void GetBlinds()
        {
            if (_history.Ante > 0)
                for (int i = _utgIdx, count = 0; count < _seats.Length; i = (i + 1) % _seats.Length, count++)
                    AddAction(i, new Action(_seats[i].Name, Action.ActionTypes.PostAnte, _history.Ante), _history.PredealActions);

            // If there is no small blind, the big blind is the utg player, otherwise they're utg+1
            _bbIdx = _playerIndices.Next;
            if (_history.SmallBlind > 0)
            {
                
                // If there was an ante and the small blind was put all-in, they can't post the small blind
                if (_playerIndices.Contains(_utgIdx))
                {
                    AddAction(_bbIdx, 
                              new Action(_seats[_bbIdx].Name, Action.ActionTypes.PostSmallBlind, _history.SmallBlind),
                              _history.PredealActions);
                }
                _bbIdx = _playerIndices.Next;
            }
            if (_history.BigBlind > 0)
                if (_playerIndices.Contains(_bbIdx))
                    AddAction(_bbIdx, 
                              new Action(_seats[_bbIdx].Name, Action.ActionTypes.PostBigBlind, _history.BigBlind), 
                              _history.PredealActions);
        }

        /// <summary>
        /// Deals out all of the players' hole cards.
        /// </summary>
        public void DealHoleCards()
        {
            for (int i = 0; i < _seats.Length; i++)
            {
                _history.HoleCards[i] = _cache != null ? _cache.HoleCards[i] : Hand.RandomHand(_history.DealtCards, 2);
                _history.DealtCards = _history.DealtCards | _history.HoleCards[i];
            }
        }

        public void DealFlop()
        {
            _history.Flop = _cache != null ? _cache.Flop : Hand.RandomHand(_history.DealtCards, 3);
            _history.DealtCards = _history.DealtCards | _history.Flop;
        }

        public void DealTurn()
        {
            _history.Turn = _cache != null ? _cache.Turn : Hand.RandomHand(_history.DealtCards, 1);
            _history.DealtCards = _history.DealtCards | _history.Turn;
        }

        public void DealRiver()
        {
            _history.River = _cache != null ? _cache.River : Hand.RandomHand(_history.DealtCards, 1);
            _history.DealtCards = _history.DealtCards | _history.River;
        }



    }
 
}
