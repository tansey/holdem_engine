using System;
using System.Collections.Generic;
using System.Text;

namespace holdem_engine
{
    /// <summary>
    /// BettingStructure enum represents the type of betting that the game
    /// allows.  Options are Limit, Pot-Limit, or No-Limit.
    /// </summary>
    public enum BettingStructure
    {
        None,
        Limit,
        PotLimit,
        NoLimit
    }
}
