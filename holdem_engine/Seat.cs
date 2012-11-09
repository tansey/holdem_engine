using System;
using System.Collections.Generic;
using System.Text;

namespace holdem_engine
{
    public class Seat
    {
        #region Member Variables
        private string name;
        private double chips;
        private int seat;
        private IPlayer brain;
        #endregion

        #region Properties
        /// <summary>
        /// The seat number that this player is at
        /// </summary>
        public int SeatNumber
        {
            get { return seat; }
            set { seat = value; }
        }

        /// <summary>
        /// The amount of chips this player has.
        /// </summary>
        public double Chips
        {
            get { return chips; }
            set { chips = value; }
        }

        /// <summary>
        /// The name of this player.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// The brain of this player (i.e., the object that makes betting decisions).
        /// </summary>
        public IPlayer Brain
        {
            get { return brain; }
            set { brain = value; }
        }
        #endregion

        #region Constructors
        public Seat()
        {
        }

        public Seat(int seatNumber, string playerName, double chips)
        {
            this.name = playerName;
            this.seat = seatNumber;
            this.chips = chips;
        }

        public Seat(int seatNumber, string playerName, double chips, IPlayer brain)
        {
            this.name = playerName;
            this.seat = seatNumber;
            this.chips = chips;
            this.brain = brain;
        }
        #endregion
    }
}
