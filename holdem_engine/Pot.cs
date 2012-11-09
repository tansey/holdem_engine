using System;
using System.Collections.Generic;
using System.Text;

namespace holdem_engine
{
    

    /// <summary>
    /// A pot is defined as an amount of money which is currently able
    /// to be won by only those who are eligible.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    public class Pot
    {
        #region Member Variables
        private Seat[] players;
        private bool[] eligible;
        private double size;
        private string name;
        private int eligibleCount;
        #endregion

        #region Properties
        /// <summary>
        /// The size of this pot, i.e. how much money is in the pot.
        /// </summary>
        public double Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// The name of this pot.
        /// 
        /// Examples: "Main Pot", "Side Pot #1"
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new, empty Pot with the given name.
        /// </summary>
        public Pot(string potName, Seat[] players)
        {
            this.players = players;

            //.NET defaults to false on booleans, no need to initialize
            eligible = new bool[players.Length]; 

            size = 0;
            eligibleCount = 0;
            name = potName;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a player to the pot.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="amount"></param>
        public void AddPlayer(int playerIdx, double amount)
        {
            eligible[playerIdx] = true;
            eligibleCount++;
            size += amount;
        }

        /// <summary>
        /// Removes a player from this pot, but keeps his money in there.
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(int playerIdx)
        {
            eligible[playerIdx] = false;
            eligibleCount--;
        }

        /// <summary>
        /// Adds money to the pot.
        /// </summary>
        /// <param name="amount"></param>
        public void Add(double amount)
        {
            size += amount;
        }

        /// <summary>
        /// Gets the winners of this pot according to the given dictionary
        /// of players' hand strengths.  Winners are added to the winner list.
        /// </summary>
        /// <param name="handStrengths">An array of player hand strengths at showdown</param>
        /// <param name="winners">A list of winners to which to add the winners of this pot.</param>
        public void GetWinners(uint[] handStrengths,  List<Winner> winners)
        {            
            //if there's only 1 player who can win this pot, just give him the money
            if( eligibleCount == 1 )
            {
                for(int i = 0; i < eligible.Length; i++)
                    if (eligible[i])
                    {
                        winners.Add(new Winner(players[i].Name, name, size));
                        players[i].Chips += size;//pay the player
                        return;
                    }

                throw new Exception("eligibleCount == 1 but no eligible players found!");
            }

            List<Seat> toPay = new List<Seat>();
            uint highest = uint.MinValue;

            //Go through each player who showed down his hand
            for (int player = 0; player < players.Length; player++)
            {
                //we only want players who are eligible to win this pot
                if (eligible[player])
                {
                    uint strength = handStrengths[player];

                    //if this player has the strongest hand so far, start
                    //a new list of people to pay with him being the first.
                    if (strength > highest)
                    {
                        highest = strength;
                        toPay.Clear();
                        toPay.Add(players[player]);
                    }
                    //if this player ties the strongest hand so far, add him
                    //to the current list of people to pay.
                    else if (strength == highest)
                    {
                        toPay.Add(players[player]);
                    }
                }
            }

            //if only 1 player won this hand, give him the whole thing
            if (toPay.Count == 1)
            {
                winners.Add(new Winner(toPay[0].Name,name,size));
                toPay[0].Chips += size;
                return;
            }

            //if we have multiple winners, then this pot needs to be split
            double splitAmt = Math.Round(size / (double)toPay.Count,2,MidpointRounding.AwayFromZero);

            //pay each player their portion of the pot.
            foreach (Seat player in toPay)
            {
                winners.Add(new Winner(player.Name,name,splitAmt));
                player.Chips += splitAmt;
            }
        }
        #endregion
    }
}
