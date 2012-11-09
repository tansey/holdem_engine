using System;
using System.Collections.Generic;
using System.Text;

namespace test_holdem_engine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BetManagerTest bmt = new BetManagerTest();
            bmt.SetUp();
            bmt.TestBlinds();

            //PotManagerTest pmt = new PotManagerTest();
            //pmt.setup();
            //pmt.TestTotal();
            //pmt.setup();
            //pmt.TestAllInSidePots();

            FastPotManagerTest fpmt = new FastPotManagerTest();
            fpmt.setup();
            fpmt.TestTotal();
            fpmt.setup();
            fpmt.TestAllInSidePots();

            HandEngineTest test = new HandEngineTest();
            test.SetUp();
            test.TestPlayHand();
        }
    }
}
