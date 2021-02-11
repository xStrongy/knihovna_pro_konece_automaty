using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class DeltaFunctionTriplet
    {
		public int From { get; }
		public char By { get; }
		public int To { get; }

		public DeltaFunctionTriplet(int From, char By, int To)
		{
			this.From = From;
			this.By = By;
			this.To = To;
		}
	}
}
