using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace holdem_engine
{
    /// <summary>
    /// Caches the cards that will be dealt for a single hand.
    /// </summary>
    [Serializable]
    public class CachedHand
    {
        public ulong[] HoleCards { get; set; }
        public ulong Flop { get; set; }
        public ulong Turn { get; set; }
        public ulong River { get; set; }

        public CachedHand()
        {
        }

        public CachedHand(int numPlayers, Random random)
        {
            cacheCards(numPlayers, random);
        }

        private void cacheCards(int numPlayers, Random random)
        {
            ulong dead = 0UL;
            HoleCards = new ulong[numPlayers];
            for (int i = 0; i < numPlayers; i++)
            {
                HoleCards[i] = HoldemHand.Hand.RandomHand(random, dead, 2);
                dead = dead | HoleCards[i];
            }

            Flop = HoldemHand.Hand.RandomHand(random, dead, 3);
            dead = dead | Flop;

            Turn = HoldemHand.Hand.RandomHand(random, dead, 1);
            dead = dead | Turn;

            River = HoldemHand.Hand.RandomHand(random, dead, 1);
        }
    }

    [Serializable]
    public class CachedHands
    {
        public List<CachedHand> Hands { get; set; }
    }
}
