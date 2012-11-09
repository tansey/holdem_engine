using System;
using System.Linq;
using HoldemHand;
using System.Text;
using System.Collections.Generic;

namespace holdem_engine
{
	public static class CardExtensions
	{
		public static string ToCardString(this ulong card)
		{
			return Hand.MaskToString(card); 
		}

		public static string ToCardString(this IEnumerable<ulong> cards)
		{
			StringBuilder sb = new StringBuilder();
			int count = cards.Count();
			sb.Append("[ ");
			for(int i = 0; i < count; i++)
			{
				sb.Append(cards.ElementAt(i).ToCardString());
				sb.Append(" ");
			}
			sb.Append("]");
			return sb.ToString();
		}
	}
}

