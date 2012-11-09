using System;
using System.Collections.Generic;
using System.Text;

namespace holdem_engine
{
    /// <summary>
    /// Simple data class representing the winner of a Pot.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    public class Winner
    {
        public readonly double Amount;
        public readonly string Player;
        public readonly string Pot;

        public Winner(string player, string pot, double amount)
        {
            Player = player;
            Pot = pot;
            Amount = amount;            
        }
    }
}
