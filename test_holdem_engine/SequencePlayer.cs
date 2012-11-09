using System;
using System.Collections.Generic;
using System.Text;
using holdem_engine;
using System.IO;
using System.Xml.Serialization;

namespace test_holdem_engine
{
    /// <summary>
    /// Makes a set of pre-defined actions.
    /// </summary>
    public class SequencePlayer : IPlayer
    {
		private holdem_engine.Action[] actions;
        private int curAction;

		public SequencePlayer(params holdem_engine.Action[] actions)
        {
            this.actions = actions;
            this.curAction = 0;
        }



        public void NewHand(HandHistory history)
        {
        }

        public void GetAction(HandHistory history,
		                      out holdem_engine.Action.ActionTypes type, out double amount)
        {
			PokerHandHistory.PokerHand xml = history.ToXmlHand();
			
			PokerHandHistory.PokerHandXML hands = new PokerHandHistory.PokerHandXML(){
				Hands = new PokerHandHistory.PokerHand[] { xml }
			};

			StringBuilder sb = new StringBuilder();
			using(TextWriter writer = new StringWriter(sb))
			{
				XmlSerializer ser = new XmlSerializer(typeof(PokerHandHistory.PokerHandXML));
				ser.Serialize(writer, hands);
			}
			
			Console.WriteLine(sb.ToString());
            if (actions != null && curAction < actions.Length)
            {
				holdem_engine.Action action = actions[curAction++];
                type = action.ActionType;
                amount = action.Amount;
            }
            else
            {
				type = holdem_engine.Action.ActionTypes.Fold;
                amount = 0;
            }
        }
    }
}
