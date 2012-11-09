using System;
using System.Collections.Generic;
using System.Text;
using holdem_engine;

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
