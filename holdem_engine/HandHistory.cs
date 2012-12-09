using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PokerHandHistory;

namespace holdem_engine
{
    /// <summary>
    /// An accessor class which contains the _history of a hand.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    public class HandHistory : IComparable<HandHistory>
    {

        #region Member Variables
        private ulong handNumber;
        private uint button;
        private double stakes;
        private double smallBlind;
        private double bigBlind;
        private double[] allBlinds;
        private double ante;
        private double pot;
        private string tableName;
        private string site;
        private ulong[] hc;
        private ulong flop;
        private ulong turn;
        private ulong river;
        private BettingStructure bs;
        private List<Action> predealActions;
        private List<Action> preflopActions;
        private List<Action> flopActions;
        private List<Action> turnActions;
        private List<Action> riverActions;
        private List<Winner> winners;
        private Seat[] players;
        private double[] startingChips;
        private int maxPlayersPerTable;
        private ulong dealtCards;
        #endregion

        #region Properties
        public int CurrentBetLevel { get; set; }
        public Round CurrentRound { get; set; }

        public ulong Board
        {
            get { return flop | turn | river; }
        }

        public double[] AllBlinds
        {
            get { return allBlinds; }
            set { allBlinds = value; }
        }
        

        public ulong DealtCards
        {
            get { return dealtCards; }
            set { dealtCards = value; }
        }


        public ulong Flop
        {
            get { return flop; }
            set { flop = value; }
        }

        public ulong Turn
        {
            get { return turn; }
            set { turn = value; }
        }

        public ulong River
        {
            get { return river; }
            set { river = value; }
        }

        public List<Winner> Winners
        {
            get { return winners; }
            set { winners = value; }
        }
        
        public List<Action> PredealActions
        {
            get { return predealActions; }
            set { predealActions = value; }
        }
        
        public List<Action> PreflopActions
        {
            get { return preflopActions; }
            set { preflopActions = value; }
        }
        public List<Action> FlopActions
        {
            get { return flopActions; }
            set { flopActions = value; }
        }

        public List<Action> TurnActions
        {
            get { return turnActions; }
            set { turnActions = value; }
        }

        public List<Action> RiverActions
        {
            get { return riverActions; }
            set { riverActions = value; }
        }

        public double[] StartingChips
        {
            get { return startingChips; }
            set { startingChips = value; }
        }

        public Seat[] Players
        {
            get { return players; }
            set { players = value; }
        }

        public int MaxPlayersPerTable
        {
            get { return maxPlayersPerTable; }
            set { maxPlayersPerTable = value; }
        }

        public string Site
        {
            get { return site; }
            set { site = value; }
        }

        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        public double Stakes
        {
            get { return stakes; }
            set { stakes = value; }
        }

        public double Pot
        {
            get { return pot; }
            set { pot = value; }
        }

        public BettingStructure BettingStructure
        {
            get { return bs; }
            set { bs = value; }
        }
	
        public ulong[] HoleCards
        {
            get { return hc; }
            set { hc = value; }
        }

        public double Ante
        {
            get { return ante; }
            set { ante = value; }
        }

        public double BigBlind
        {
            get { return bigBlind; }
            set { bigBlind = value; }
        }
	

        public double SmallBlind
        {
            get { return smallBlind; }
            set { smallBlind = value; }
        }

        public uint Button
        {
            get { return button; }
            set { button = value; }
        }
                   

        public ulong HandNumber
        {
            get { return handNumber; }
            set { handNumber = value; }
        }
        public bool[] Folded { get; set; }
        public bool[] AllIn { get; set; }
        public bool ShowDown { get; set; }

        /// <summary>
        /// The index of the player who has to act currently
        /// </summary>
        public int Hero { get; set; }
        #endregion

        #region Constructors
        public HandHistory(Seat[] players, ulong handNumber, uint button, double[] blinds, double ante, BettingStructure bs)
        {
            this.button = button;
            predealActions = new List<Action>();
            preflopActions = new List<Action>();
            flopActions = new List<Action>();
            turnActions = new List<Action>();
            riverActions = new List<Action>();
            Folded = new bool[players.Length];
            AllIn = new bool[players.Length];
            this.players = players;
            hc = new ulong[players.Length];
            startingChips = new double[players.Length];
            for (int i = 0; i < startingChips.Length; i++)
                startingChips[i] = players[i].Chips;

            this.handNumber = handNumber;
            switch (blinds.Length)
            {
                case 1: this.bigBlind = blinds[0];
                    break;

                case 2: 
                    this.smallBlind = blinds[0];
                    this.bigBlind = blinds[1];                    
                    break;
                default:
                    break;
            }

            this.ante = ante;
            BettingStructure = bs;
            allBlinds = blinds;
            site = "SimulatedPokerSite";
            CurrentRound = Round.Predeal;
            CurrentBetLevel = 1;
        }
        #endregion

        #region Comparison methods
        /// <summary>
        /// All comparisons are done simply on a HandNumber basis.  If this hand number
        /// is greater than hand's hand number, this method returns 1.  If it's less
        /// than hand's hand number, it will return -1.  If they are equal, it returns 0.
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public int CompareTo(HandHistory hand)
        {
            if (handNumber > hand.handNumber)
            {
                return 1;
            }
            if (handNumber < hand.handNumber)
            {
                return -1;
            }
            return 0;
        }

        #endregion

		public PokerHand ToXmlHand()
		{
			PokerHand hand = new PokerHand()
			{
				Hero = players[Hero].Name
			};

			#region Convert blinds
			var blindPostings = predealActions.Where(s => 
			                          s.ActionType == Action.ActionTypes.PostSmallBlind
			                          || s.ActionType == Action.ActionTypes.PostBigBlind
			                          || s.ActionType == Action.ActionTypes.PostAnte);


			hand.Blinds = new Blind[blindPostings.Count()];
			for(int i =0 ; i < hand.Blinds.Length; i++)
			{
				var blindPosting = blindPostings.ElementAt(i);
				hand.Blinds[i] = new Blind() {
					AllIn = blindPosting.AllIn,
					Amount = (decimal)blindPosting.Amount,
					Player = blindPosting.Name,
					Type = blindPosting.ActionType == Action.ActionTypes.PostBigBlind ? BlindType.BigBlind
						: blindPosting.ActionType == Action.ActionTypes.PostSmallBlind ? BlindType.SmallBlind
						: blindPosting.ActionType == Action.ActionTypes.PostAnte ? BlindType.Ante
						: BlindType.None
				};
			}
			#endregion

			#region Context
			hand.Context = new Context();
			hand.Context.Ante = (decimal)Ante;

			#region Betting Structure
			if(BettingStructure == holdem_engine.BettingStructure.None)
				throw new Exception("Unknown betting structure");
			hand.Context.BettingType = this.BettingStructure == holdem_engine.BettingStructure.Limit ? BettingType.FixedLimit
									: BettingStructure == holdem_engine.BettingStructure.NoLimit ? BettingType.NoLimit
									: BettingType.PotLimit;
			#endregion

			hand.Context.BigBlind = (decimal)BigBlind;
			hand.Context.Button = (int)Button;
			hand.Context.CapAmount = 0; // no caps supported
			hand.Context.CapAmountSpecified = false;
			hand.Context.Capped = false;
			hand.Context.Currency = "$";
			hand.Context.Format = GameFormat.CashGame;
			hand.Context.ID = HandNumber.ToString();
			hand.Context.Online = false;
			hand.Context.PokerVariant = PokerVariant.TexasHoldEm;
			hand.Context.Site = "SimulatedPokerSite";
			hand.Context.SmallBlind = (decimal)SmallBlind;
			hand.Context.Table = TableName;
			hand.Context.TimeStamp = DateTime.Now;
			#endregion

			#region HoleCards
			hand.HoleCards = HoldemHand.Hand.Cards(HoleCards[Hero])
											.Select(c => new PokerHandHistory.Card(c))
											.ToArray();
			#endregion

			hand.Rake = 0m;
			hand.Players = this.Players.Select((s,i) => new Player() { Name = s.Name, Seat = s.SeatNumber, Stack = (decimal)StartingChips[i] }).ToArray();

			#region Rounds (actions and community cards)
			hand.Rounds = new PokerHandHistory.Round[Math.Min(4, (int)this.CurrentRound)];
			if(CurrentRound >= Round.Preflop)
			{
				hand.Rounds[0] = new PokerHandHistory.Round();
				hand.Rounds[0].Actions = PreflopActions.Select(a => convertActionToXml(a)).ToArray();
				if(CurrentRound >= Round.Flop)
				{
					hand.Rounds[1] = new PokerHandHistory.Round();
					hand.Rounds[1].Actions = FlopActions.Select(a => convertActionToXml(a)).ToArray();
					hand.Rounds[1].CommunityCards = HoldemHand.Hand.Cards (Flop)
													.Select(c => new PokerHandHistory.Card(c))
													.ToArray();

					if(CurrentRound >= Round.Turn)
					{
						hand.Rounds[2] = new PokerHandHistory.Round();
						hand.Rounds[2].Actions = TurnActions.Select(a => convertActionToXml(a)).ToArray();
						hand.Rounds[2].CommunityCards = HoldemHand.Hand.Cards (Turn)
							.Select(c => new PokerHandHistory.Card(c))
							.ToArray();

						if(CurrentRound >= Round.River)
						{
							hand.Rounds[3] = new PokerHandHistory.Round();
							hand.Rounds[3].Actions = RiverActions.Select(a => convertActionToXml(a)).ToArray();
							hand.Rounds[3].CommunityCards = HoldemHand.Hand.Cards (River)
								.Select(c => new PokerHandHistory.Card(c))
								.ToArray();
						}
					}
				}
			}
			#endregion

			#region Results
			if(Winners != null && Winners.Count() > 0)
			{
				hand.Results = new HandResult[players.Length];
				//int potNum = 0;
				for(int i = 0; i < hand.Results.Length; i++)
				{
					HandResult hr = new HandResult(players[i].Name);
					hr.HoleCards = HoldemHand.Hand.Cards(HoleCards[i])
									.Select(c => new PokerHandHistory.Card(c))
									.ToArray();
					var wins = winners.Where(w => w.Player == hr.Player);
					if(wins != null)
						hr.WonPots = wins.Select(w => new PokerHandHistory.Pot() {
							Amount = (decimal)w.Amount,
							//Number = potNum++ // not able to handle this properly currently.
						}).ToArray();

					hand.Results[i] = hr;
				}
			}
			#endregion

			return hand;
		}

		private PokerHandHistory.Action convertActionToXml(Action a)
		{
			PokerHandHistory.Action result = new PokerHandHistory.Action();
			result.Player = a.Name;
			result.Amount = (decimal)a.Amount;
			result.AllIn = a.AllIn;

			switch (a.ActionType) {
			case Action.ActionTypes.Bet: result.Type = PokerHandHistory.ActionType.Bet;
				break;
			case Action.ActionTypes.Raise: result.Type = PokerHandHistory.ActionType.Raise;
				break;
			case Action.ActionTypes.Check: result.Type = PokerHandHistory.ActionType.Check;
				break;
			case Action.ActionTypes.Call: result.Type = PokerHandHistory.ActionType.Call;
				break;
			case Action.ActionTypes.Fold: result.Type = PokerHandHistory.ActionType.Fold;
				break;
			default: throw new Exception("unknown post-blinds action type: " + a.ToString());
			}
			return result;
		}

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            
                result.Append(string.Format("{0} Game #{1}:  Hold'em ", site, handNumber));
                #region Betting Structure
                switch (bs)
                {
                    case BettingStructure.Limit: result.Append("Limit ");
                        break;
                    case BettingStructure.PotLimit: result.Append("Pot Limit ");
                        break;
                    case BettingStructure.NoLimit: result.Append("No Limit ");
                        break;
                    default: throw new Exception("Unknown betting structure ");
                }
                #endregion

                result.AppendLine("Seat #" + Button + " is the button");

                for (int i = 0; i < Players.Length; i++)
                {
                    result.AppendLine(string.Format("Seat {0}: {1} ({2} in chips)", Players[i].SeatNumber,
                                                                          Players[i].Name, StartingChips[i]));

                }

                foreach (Action action in PredealActions)
                    result.AppendLine(action.ToString());

                result.AppendLine("*** HOLE CARDS ***");

                for (int i = 0; i < Players.Length; i++)
                    if (HoleCards[i] != 0UL)
                        result.AppendLine(string.Format("Dealt to {0} [{1}]", Players[i].Name,
                                                        HoldemHand.Hand.MaskToString(HoleCards[i])));

                foreach (Action action in PreflopActions)
                    result.AppendLine(action.ToString());

                #region Print post-flop actions and board cards.
                if (Flop != 0UL)
                {
                    result.AppendLine("*** Flop *** [" + HoldemHand.Hand.MaskToString(Flop) + "]");

                    foreach (Action action in FlopActions)
                        result.AppendLine(action.ToString());
                }
                if (Turn != 0UL)
                {
                    result.AppendLine("*** Turn *** [" + HoldemHand.Hand.MaskToString(Flop) + "] ["
                                                       + HoldemHand.Hand.MaskToString(Turn) + "]");

                    foreach (Action action in TurnActions)
                        result.AppendLine(action.ToString());
                }
                if (River != 0UL)
                {
                    result.AppendLine("*** River *** ["
                                          + HoldemHand.Hand.MaskToString(Flop)
                                          + " " + HoldemHand.Hand.MaskToString(Turn)
                                          + "] ["
                                          + HoldemHand.Hand.MaskToString(River) + "]");

                    foreach (Action action in RiverActions)
                        result.AppendLine(action.ToString());
                }
                #endregion

                if (ShowDown)
                {
                    result.AppendLine("*** Show Down ***");
                    for (int i = 0; i < players.Length; i++)
                        if (!Folded[i])
                            result.AppendLine(string.Format("{0} shows {1}", Players[i].Name,
                                HoldemHand.Hand.DescriptionFromMask(Flop | Turn | River | hc[i])));
                }

                if (Winners != null)
                {
                    result.AppendLine("*** Summary ***");
                    foreach (Winner winner in Winners)
                    {
                        result.AppendFormat("{0} collected {1} from {2}", winner.Player, winner.Amount, winner.Pot);
                        result.AppendLine();
                    }
                    //TODO: output summaries
                }
                return result.ToString();
        }
    }
}
