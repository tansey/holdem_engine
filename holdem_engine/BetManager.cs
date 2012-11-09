using System;
using System.Collections.Generic;
using System.Text;

namespace holdem_engine
{
    /// <summary>
    /// The job of the BetManager is to validate all bets returned by the
    /// PlayerManager.  The BetManager also determines if the current betting
    /// round is over.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    public class BetManager
    {
        #region Member Variables
        private double minimumRaise;
        private double mostCommitted;//the most amount of money a player has committed so far.
        private double smallBlind;
        private double bigBlind;
        private double ante;
        private Dictionary<string, double> committedTotal;
        private Dictionary<string, double> committedThisRound;
        private Dictionary<string, double> startingChips;
        private BettingStructure bs;
        private int numberBlindsPosted;
        private int numberAntesPosted;
        private int numberPlayersIn;//number of players still in the hand
        private int numberCalls;//number of players who have called the most recent raise
        private int numberAllIns;//number of players who ahve gone all-in
        private int numberPlayersCanStillBet; //number of players who can still bet (i.e. not folded nor all-in)
        private int betLevel;
        private int round;

        #endregion

        #region Properties

        /// <summary>
        /// A property that can only be true once per round.  When you access
        /// it and it's true, it will set itself to false and return true.
        /// 
        /// Example:
        /// BetManager bm = new BetManager(...);
        /// ...
        /// bool testA = bm.RoundOver;
        /// bool testB = bm.RoundOver;
        /// bool testC = testA && testB;
        /// ...
        /// 
        /// In this example, testC will ALWAYS be false.
        /// 
        /// </summary>
        public bool RoundOver
        {
            get
            {
                if (numberPlayersCanStillBet <= 1 || (numberCalls == numberPlayersCanStillBet
                && (numberCalls > 0 || numberAllIns > 0)))
                {
                    numberCalls = 0;
                    round++;
                    if (round == 1 || bs != BettingStructure.Limit)
                    {
                        minimumRaise = bigBlind;
                    }
                    else
                    {
                        minimumRaise = 2 * bigBlind;
                    }
                    betLevel = 0;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// The current bet level for this hand.  Preflop bet level starts at 1.  All others
        /// start at 0.  Bet level is incremented each time there is a bet or raise.
        /// </summary>
        public int BetLevel
        {
            get { return betLevel; }
            set { betLevel = value; }
        }
        /// <summary>
        /// The minimum amount that the next player can raise.
        /// 
        /// By convention, this is equal to the size of the last raise.
        /// 
        /// Example: 
        /// In a No-Limit game where the big blind is $1:
        ///  - PlayerA raises to $4.
        ///  - PlayerB reraises to $10.
        ///  - It is now on PlayerC to act.
        /// The value of MinimumRaise for PlayerC is $6 since the last
        /// raise was by PlayerB for a value of $6 on top of his $4 call.
        /// 
        /// 
        /// If the structure is limit, this value is capped at
        /// 4 bets (i.e. 4x the big blind on Preflop and Flop rounds,
        /// and 8x the big blind on Turn and River rounds).
        /// </summary>
        public double MinimumRaise
        {
            get { return minimumRaise; }
            set { minimumRaise = value; }
        }

        /// <summary>
        /// The number of players who are still in the hand (i.e. who have yet to fold)
        /// </summary>
        public int In
        {
            get { return numberPlayersIn; }
        }

        /// <summary>
        /// The number of players who can still bet in the hand (i.e. who have not folded
        /// nor gone all-in).
        /// </summary>
        public int CanStillBet
        {
            get { return numberPlayersCanStillBet; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new BetManager for the hand.
        /// </summary>
        /// <param name="namesToChips">A dictionary that maps player names to the amount of money they started the hand with.</param>
        /// <param name="structure">The type of betting structure this hand is.</param>
        /// <param name="ante">The ante for this hand. If there is no ante, it should be set to -1.</param>
        public BetManager(Dictionary<string,double> namesToChips, BettingStructure structure,
            double[] blinds, double ante )
        {
            round = 0;
            bs = structure;
            numberBlindsPosted = 0;
            this.smallBlind = blinds.Length > 1 ? blinds[0] : -1;
            this.bigBlind = blinds.Length > 1 ? blinds[1] : blinds[0];
            this.ante = ante;
            this.minimumRaise = bigBlind;
            numberPlayersIn = namesToChips.Keys.Count;
            numberPlayersCanStillBet = numberPlayersIn;
            numberCalls = 0;
            betLevel = 0;

            committedThisRound = new Dictionary<string, double>();
            committedTotal = new Dictionary<string, double>();
            startingChips = new Dictionary<string, double>();

            foreach (string name in namesToChips.Keys)
            {
                committedTotal[name] = 0;
                committedThisRound[name] = 0;
                startingChips[name] = namesToChips[name];
            }
        }
        #endregion



        /// <summary>
        /// Checks the given Action object to make sure it's valid.
        /// Invalid actions will be changed to the closest corresponding action.
        /// The convention used to change invalid actions is as follows:
        /// 
        /// - Actions of type None, JoinTable, SitOut, LeaveTable will automatically
        ///   become Fold Actions.
        /// - Invalid Post* actions will be treated as Checks (see below).
        /// - Invalid Checks will be turned into Folds.
        /// - Bets which are sent after another player has bet this round will be turned
        ///   into raises and have to adhear to the same convention (see below).
        /// - Raises which are less than MinimumRaise will be changed to be of Amount = MinimumRaise.
        /// - Raises which are more than the size of a player's remaining chips will be changed into AllIns
        ///   and adhear to the same convention (see below).
        /// - AllIns will have their Amount changed to the size of a player's remaining stack.
        /// </summary>
        /// <param name="action">The Action which needs to be validated.</param>
        /// <returns>An Action which is guaranteed to be valid.</returns>
        public Action GetValidatedAction(Action action)
        {
            switch (action.ActionType)
            {
                case Action.ActionTypes.PostAnte: return validatePostAnte(action);
                case Action.ActionTypes.PostSmallBlind: return validatePostSmallBlind(action);
                case Action.ActionTypes.PostBigBlind: return validatePostBigBlind(action);
                case Action.ActionTypes.Fold: return validateFold(action);
                case Action.ActionTypes.Check: return validateCheck(action);
                case Action.ActionTypes.Call: return validateCall(action);
                case Action.ActionTypes.Bet: return validateBet(action);
                case Action.ActionTypes.Raise: return validateRaise(action);
                //case Action.ActionTypes.AllIn: return validateAllIn(action);
                default: return validateFold(action);
            }
        }

        private Action validatePostAnte(Action action)
        {
            if (ante <= 0 || numberAntesPosted > startingChips.Count)
                return validateFold(action);

            action.Amount = Math.Min(startingChips[action.Name], ante);

            return action;
        }

        private Action validatePostSmallBlind(Action action)
        {
            if (numberBlindsPosted > 0)
                return validateFold(action);

            if (smallBlind <= 0)
                return validatePostBigBlind(action);

            action.Amount = Math.Min(startingChips[action.Name], smallBlind);
            action.ActionType = Action.ActionTypes.PostSmallBlind;
            return action;
        }

        private Action validatePostBigBlind(Action action)
        {
            // if sb and bb have been posted
            if ((numberBlindsPosted >= 2)
                //if there is only 1 blind this hand, and it's already been posted
                || (smallBlind <= 0 && numberBlindsPosted >= 1)
                //if there are 2 blinds this hand and sb hasn't posted yet
                || (smallBlind > 0 && numberBlindsPosted < 1))
            {
                return validateFold(action);
            }
            action.ActionType = Action.ActionTypes.PostBigBlind;

            action.Amount = Math.Min(startingChips[action.Name], bigBlind);
            
            numberBlindsPosted++;
            return action;
        }

        private Action validateFold(Action action)
        {
            //We don't test if the player can check, that's up to the player to notice.
            //If you want to fold when you could check, that's your right.
            //
            //CORRECTION: I changed my mind. I think it's easier for a neural network
            //to learn if it doesn't have to worry about knowing that it should check
            //because no one has bet
            if (committedTotal[action.Name] >= mostCommitted)
                return validateCheck(action);

            action.Amount = 0;
            action.ActionType = Action.ActionTypes.Fold;
            return action;
        }

        private Action validateCheck(Action action)
        {
            if (committedTotal[action.Name] < mostCommitted)//fold if you can't check
                return validateFold(action);

            action.ActionType = Action.ActionTypes.Check;
            action.Amount = 0;//just in case it's not set to 0 already.
            return action;
        }

        private Action validateCall(Action action)
        {
            if (committedTotal[action.Name] >= mostCommitted)//check if you don't have to call anything
                return validateCheck(action);

            action.Amount = mostCommitted - committedTotal[action.Name];

            if (action.Amount + committedTotal[action.Name] > startingChips[action.Name])
                return validateAllIn(action);

            return action;
        }

        private Action validateAllIn(Action action)
        {
            //if (action.ActionType != Action.ActionTypes.AllIn)
                //action.ActionType = Action.ActionTypes.AllIn;
            if (!action.AllIn)
                action.AllIn = true;

            action.Amount = startingChips[action.Name] - committedTotal[action.Name];
            return action;
        }

        private Action validateBet(Action action)
        {
            //if someone has already bet, we have to raise.
            if (betLevel > 0)
                return validateRaise(action);

            if(action.ActionType != Action.ActionTypes.Bet)
                action.ActionType = Action.ActionTypes.Bet;

            //make sure a player is betting the minimum.
            double amt = Math.Max(action.Amount, MinimumRaise);

            //if the bet is larger than the player's remaining chips, change him to all-in.
            if (amt + mostCommitted >= startingChips[action.Name])
            {
                //action.ActionType = Action.ActionTypes.AllIn;
                action.AllIn = true;
                action.Amount = startingChips[action.Name] - committedTotal[action.Name];
                return action;
            }

            //if we're playing limit and the hand has been capped, we can only call.
            if (bs == BettingStructure.Limit && betLevel >= 4)
            {
                action.ActionType = Action.ActionTypes.Call;
                action.Amount = mostCommitted - committedTotal[action.Name];
                return action;
            }

            //if the bet's legit, just make sure the amount is legit, and return it.
            action.Amount = amt;
            return action;
        }

        private Action validateRaise(Action action)
        {
            //if someone has already bet, we have to raise.
            if (betLevel == 0)
                return validateBet(action);

            if (action.ActionType != Action.ActionTypes.Raise)
                action.ActionType = Action.ActionTypes.Raise;

            //make sure a player is raising the minimum.
            double amt = Math.Max(action.Amount,
                                  mostCommitted - committedTotal[action.Name] + minimumRaise);

            //if the bet is larger than the player's remaining chips, change him to all-in.
            if (amt + committedTotal[action.Name] >= startingChips[action.Name])
            {
                //action.ActionType = Action.ActionTypes.AllIn;
                action.AllIn = true;
                action.Amount = startingChips[action.Name] - committedTotal[action.Name];
                return action;
            }

            //if we're playing limit and the hand has been capped, we can only call.
            if (bs == BettingStructure.Limit && betLevel >= 4)
            {
                action.ActionType = Action.ActionTypes.Call;
                action.Amount = mostCommitted - committedTotal[action.Name];
                return action;
            }

            //if the raise is legit, just make sure the amount is legit, and return it.
            action.Amount = amt;
            return action;
        }

        /// <summary>
        /// Updates the information about the current betting scenario.
        /// </summary>
        /// <param name="action">The validated action the player is taking</param>
        public void Commit(Action action)
        {
            double amt = action.Amount;
            string name = action.Name;

            //Handle all-in cases
            if (action.AllIn)
            {
                committedTotal[name] += amt;
                committedThisRound[name] += amt;
                double total = committedTotal[name];
                if (total > mostCommitted)//if the all-in is a raise
                {
                    mostCommitted = committedTotal[name];
                    betLevel++;
                    minimumRaise = Math.Max(committedTotal[name] - mostCommitted, minimumRaise);
                    numberCalls = 0;
                    numberAllIns++;
                    numberPlayersCanStillBet--;
                }
                else//else the all-in is a short stack who just calls off his stack
                {
                    numberAllIns++;
                    numberPlayersCanStillBet--;
                }
                return;
            }

            switch (action.ActionType)
            {
                case Action.ActionTypes.PostAnte:
                    {
                        committedTotal[name] += amt;
                        committedThisRound[name] += amt;
                        double total = committedTotal[name];
                        numberAntesPosted++;
                        if (mostCommitted < total)
                        {
                            mostCommitted = total;
                        }
                    }
                    break;
                case Action.ActionTypes.PostSmallBlind:
                    {
                        committedTotal[name] += amt;
                        committedThisRound[name] += amt;
                        double total = committedTotal[name];
                        numberBlindsPosted++;
                        if (mostCommitted < total)
                        {
                            mostCommitted = total;
                        }
                    }
                    break;
                case Action.ActionTypes.PostBigBlind:
                    {
                        committedTotal[name] += amt;
                        committedThisRound[name] += amt;
                        double total = committedTotal[name];
                        numberBlindsPosted++;
                        if (mostCommitted < total)
                        {
                            mostCommitted = total;
                        }
                        betLevel++;
                    }
                    break;
                case Action.ActionTypes.Fold:
                    {
                        numberPlayersIn--;
                        numberPlayersCanStillBet--;
                    }
                    break;
                case Action.ActionTypes.Check:
                    {
                        numberCalls++;
                    }
                    break;
                case Action.ActionTypes.Call:
                    {
                        committedTotal[name] += amt;
                        committedThisRound[name] += amt;
                        numberCalls++;
                        
                    }
                    break;
                case Action.ActionTypes.Bet:
                    {
                        committedTotal[name] += amt;
                        committedThisRound[name] += amt;
                        mostCommitted = committedTotal[name];
                        betLevel++;
                        minimumRaise = amt;
                        numberCalls = 1;//technically, the player has called his own bet.
                    }
                    break;
                case Action.ActionTypes.Raise:
                    {
                        committedTotal[name] += amt;
                        committedThisRound[name] += amt;
                        minimumRaise = committedTotal[name] - mostCommitted;
                        mostCommitted = committedTotal[name];
                        betLevel++;
                        numberCalls = 1;//technically, the player has called his own bet.
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
