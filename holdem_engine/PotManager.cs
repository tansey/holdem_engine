using System;
using System.Collections.Generic;
using System.Text;

namespace holdem_engine
{
    /// <summary>
    /// A class to manage the pots that are created in a hand.  The
    /// PotManager is responsible for creating side pots when players
    /// go all in and for determining who wins what at the end of the hand.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    public class PotManager : IEnumerable<Pot>
    {
        #region PlayerPotInfo private class
        private class PlayerPotInfo
        {
            public double committed;
            public bool stillIn;
            public bool allIn;
        }
        #endregion

        #region Member Variables
        //Dictionary<string, PlayerPotInfo> players;
        private PlayerPotInfo[] players;
        private Seat[] seats;
        private List<Pot> pots;
        private bool updateNeeded;
        private double tempPot;
        private double total;
        #endregion

        #region Properties

        /// <summary>
        /// The total number of pots.
        /// </summary>
        public int PotCount
        {
            get { if (updateNeeded) UpdatePots(); return pots.Count; }
        }

        /// <summary>
        /// The amount of money in the current side pot. If no players are
        /// all-in, then this is simply the value of the main pot.
        /// </summary>
        public double CurrentSidePot
        {
            get { if (updateNeeded) UpdatePots(); return pots[pots.Count - 1].Size; }
        }

        /// <summary>
        /// The total amount of money in the entire pot.
        /// </summary>
        public double Total
        {
            get { return total; }
            set { total = value; }
        }

        public List<Pot> Pots
        {
            get { if (updateNeeded) UpdatePots(); return pots; }
        }

        #region Pot Names
        /// <summary>
        /// All the different names of the pots.
        /// </summary>
        private static readonly string[] PotNames = 
            {
                "Main pot ",
                "Side pot-1 ",
                "Side pot-2 ",
                "Side pot-3 ",
                "Side pot-4 ",
                "Side pot-5 ",
                "Side pot-6 ",
                "Side pot-7 ",
                "Side pot-8 ",
                "Side pot-9 ",
            };
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new PotManager for the given set of players.
        /// </summary>
        /// <param name="players">The names of the players in the hand.</param>
        public PotManager(Seat[] players)
        {
            total = 0;
            tempPot = 0;
            updateNeeded = false;
            seats = players;

            this.players = new PlayerPotInfo[players.Length];
            pots = new List<Pot>();
            Pot main = new Pot(PotNames[0], players);
            for(int i = 0; i < players.Length; i++)
            {
                main.AddPlayer(i, 0);
                PlayerPotInfo ppi = new PlayerPotInfo();
                ppi.stillIn = true;
                ppi.allIn = false;
                ppi.committed = 0;
                this.players[i] = ppi;
            }
            pots.Add(main);
        }
        #endregion

        #region Public Methods

        #region Add Action Helper Methods
        /// <summary>
        /// Adds the player to the list of all-in players
        /// </summary>
        /// <param name="player">Name of the all in player</param>
        /// <param name="amt">Amount of money this player went all in for (not
        /// including what he already was in for)</param>
        private void AddAllIn(int playerIdx, double amt)
        {
            players[playerIdx].allIn = true;
            players[playerIdx].committed += amt;
            total += amt;
            updateNeeded = true;
        }

        /// <summary>
        /// Adds a bet from the specified player to the pot.
        /// </summary>
        /// <param name="player">Name of the betting player</param>
        /// <param name="amt">Amount the player is betting</param>
        private void AddBet(int playerIdx, double amt)
        {
            players[playerIdx].committed += amt;
            tempPot += amt;
            total += amt;
        }

        /// <summary>
        /// Folds the player out of the pot.
        /// </summary>
        /// <param name="player">Name of the folding player</param>
        private void AddFold(int playerIdx)
        {
            players[playerIdx].stillIn = false;
            foreach (Pot p in pots)
            {
                p.RemovePlayer(playerIdx);
            }
        }
        #endregion

        /// <summary>
        /// Adds the action to the Pot.  All actions should be fed to
        /// the PotManager using this method.
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(int playerIdx, Action action)
        {
            if (action.AllIn)
            {
                AddAllIn(playerIdx, action.Amount);
            }
            else if (action.ActionType == Action.ActionTypes.Fold)
            {
                AddFold(playerIdx);
            }
            else if (action.Amount > 0)
            {
                AddBet(playerIdx, action.Amount);
            }
            UpdatePots();
        }

        public bool CanStillBet(int playerIdx)
        {
            return players[playerIdx].stillIn && !players[playerIdx].allIn;
        }

        /// <summary>
        /// Updates the main and side pots.  If players have gone all-in
        /// since the last call to UpdatePots, new side pots will be created.
        /// </summary>
        public void UpdatePots()
        {
            //if we haven't had any all ins, we can just
            //add the recent bets to the last pot.
            if (!updateNeeded)
            {
                pots[pots.Count - 1].Add(tempPot);
                tempPot = 0;
                return;
            }
            
            //if we have had an all in since the last update,
            //we need to recalculate all the pots.
            double[] tempCom = new double[players.Length];
            bool[] tempStill = new bool[players.Length];
            bool[] tempAllIn = new bool[players.Length];
            for (int pIdx = 0; pIdx < players.Length; pIdx++) 
            {
                tempCom[pIdx] = players[pIdx].committed;
                tempStill[pIdx] = players[pIdx].stillIn;
                tempAllIn[pIdx] = players[pIdx].allIn;
            }

            List<Pot> result = new List<Pot>();

            int smallest = 1;
            int potCount = 0;
            while (smallest >= 0)
            {
                //find the smallest amount a player has committed who's all-in in.
                smallest = -1;
                for (int i = 0; i < players.Length; i++)
                {
                    if (tempStill[i] && tempAllIn[i] && (smallest < 0 || tempCom[i] < tempCom[smallest]))
                    {
                        smallest = i;
                    }
                }
                //make this value the next pot
                if (smallest < 0)
                    break;

                Pot nextPot = new Pot(PotNames[potCount++], seats);
                double toSubtract = tempCom[smallest];
                for (int i = 0; i < players.Length; i++)
                {
                    if (tempCom[i] > 0)
                    {
                        //We want to make sure we don't take more from a
                        //folded player's committed money than he actually
                        //put in.
                        double amt = Math.Min(toSubtract,tempCom[i]);
                        if (tempStill[i])
                        {
                            nextPot.AddPlayer(i, amt);
                        }
                        else
                        {
                            nextPot.Add(amt);
                        }
                        tempCom[i] -= amt;
                        tempStill[i] = tempCom[i] > 0;
                    }
                }

                //add the pot to the end of the list
                result.Add(nextPot);
            }

            Pot remainingPot = new Pot(PotNames[potCount], seats);
            for (int i = 0; i < players.Length; i++)
            {
                if (tempCom[i] > 0)
                {
                    if (tempStill[i])
                        remainingPot.AddPlayer(i, tempCom[i]);
                    else
                        remainingPot.Add(tempCom[i]);
                }
            }
            if(remainingPot.Size > 0)
                result.Add(remainingPot);


            tempPot = 0;

            updateNeeded = false;

            pots = result;
        }

        /// <summary>
        /// Determines the winners of each pot and returns a list of
        /// who won each pot and how much they won.
        /// </summary>
        /// <param name="handStrengths">The strength of each player's hand</param>
        /// <param name="chips">The chip stacks of the players</param>
        public List<Winner> GetWinners(uint[] handStrengths)
        {
            List<Winner> winners = new List<Winner>();
            foreach (Pot p in pots)
            {
                p.GetWinners(handStrengths, winners);
            }
            return winners;
        }
        
        #endregion

        #region IEnumerable<Pot> Members

        public List<Pot> GetPots()
        {
            return pots;
        }
        #endregion

        #region IEnumerable<Pot> Members

        public IEnumerator<Pot> GetEnumerator()
        {
            return pots.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return pots.GetEnumerator();
        }

        #endregion
    }
}
