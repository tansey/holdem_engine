using System;
using System.Collections.Generic;
using System.Text;

namespace holdem_engine
{
    /// <summary>
    /// A class to represent a player's action, like a player raising or folding.
    ///
    /// Author: Wesley Tansey
    /// </summary>
    public class Action : IEquatable<Action>
    {
        #region ActionTypes Enumeration
        public enum ActionTypes
        {
            None,
            PostAnte,
            PostSmallBlind,
            PostBigBlind,
            Fold,
            Check,
            Call,
            Bet,
            Raise
        }
        #endregion

        #region Member Variables
        private string name;
        private ActionTypes type;
        private double amount;
        private bool allIn = false;
        #endregion

        #region Properties

        /// <summary>
        /// The name of the player performing this action
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        /// <summary>
        /// The type of action that the player is performing
        /// </summary>
        public ActionTypes ActionType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
        /// <summary>
        /// The amount of money that was committed in this action, or 0 if the
        /// player performed an action that did not require money.
        /// </summary>
        public double Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
            }
        }


        /// <summary>
        /// True if this action has put the player all-in.
        /// </summary>
        public bool AllIn
        {
            get { return allIn; }
            set { allIn = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor, sets name to "", type to Predeal, and amount to 0.
        /// </summary>
        public Action()
        {
            name = "";
            type = ActionTypes.None;
            amount = 0;
        }

        /// <summary>
        /// Constructor for actions which take no money to perform.  Amount is set to 0.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public Action(string name, ActionTypes type)
        {
            this.name = name;
            this.type = type;
            amount = 0;
        }

        /// <summary>
        /// Normal constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        public Action( string name, ActionTypes type, double amount )
        {
            this.name = name;
            this.type = type;
            this.amount = amount;
        }

        public Action(string name, ActionTypes type, double amount, bool allIn)
        {
            this.name = name;
            this.type = type;
            this.amount = amount;
            this.allIn = allIn;
        }
        #endregion

        #region Operator Overloads
        public bool Equals(Action a)
        {
            return ActionType == a.ActionType && Name.CompareTo(a.Name) == 0;
        }
        #endregion

        #region String Conversion Methods
        /// <summary>
        /// Converts the action object to its string representation.  The strings conform
        /// to PokerStars hand _history conventions as of August 02, 2008.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(name + ": ");
            switch( type )
	        {
            case ActionTypes.PostAnte: result.Append("posts the ante $" + amount);
                break;
            case ActionTypes.PostSmallBlind: result.Append("posts small blind [$" + amount + "]");
		        break;
            case ActionTypes.PostBigBlind: result.Append("posts big blind [$" + amount + "]");
		        break;
	        case ActionTypes.Fold: result.Append("folds");
		        break;
	        case ActionTypes.Check: result.Append("checks");
		        break;
            case ActionTypes.Call: result.Append("calls [$" + amount + "]");
		        break;
            case ActionTypes.Bet: result.Append("bets [$" + amount + "]");
		        break;
            case ActionTypes.Raise: result.Append("raises [$" + amount + "]");
		        break;
	        }

            if(allIn)
                result.Append(" and is all-In.");

            return result.ToString();
        }

        /// <summary>
        /// Converts the action object to its string representation.  The strings conform
        /// to PokerStars hand _history conventions as of August 02, 2008.
        /// </summary>
        /// <returns></returns>
        public string ToNoDollarSignString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(name + ": ");
            switch (type)
            {
                case ActionTypes.PostAnte: result.Append("posts the ante " + amount);
                    break;
                case ActionTypes.PostSmallBlind: result.Append("posts small blind [" + amount + "]");
                    break;
                case ActionTypes.PostBigBlind: result.Append("posts big blind [" + amount + "]");
                    break;
                case ActionTypes.Fold: result.Append("folds");
                    break;
                case ActionTypes.Check: result.Append("checks");
                    break;
                case ActionTypes.Call: result.Append("calls [" + amount + "]");
                    break;
                case ActionTypes.Bet: result.Append("bets [" + amount + "]");
                    break;
                case ActionTypes.Raise: result.Append("raises [" + amount + "]");
                    break;
            }

            if (allIn)
                result.Append(" and is all-In.");

            return result.ToString();
        }
        #endregion
    }
}
