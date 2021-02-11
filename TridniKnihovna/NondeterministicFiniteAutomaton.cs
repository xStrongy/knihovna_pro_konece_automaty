using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;

namespace TridniKnihovna
{
    public class NondeterministicFiniteAutomaton : AbstractFiniteAutomaton
    {
        protected List<int> InitialStateIds;

		protected Dictionary<int, SortedList<char, List<int>>> DeltaFunction = new Dictionary<int, SortedList<char, List<int>>>();

		protected Dictionary<int, List<int>> EpsilonDeltaFunction;
		public NondeterministicFiniteAutomaton(IEnumerable<State> States, string Alphabet, IEnumerable<DeltaFunctionTriplet> Triplets, SortedList<int, List<int>> pairs)
			: base(States, Alphabet)
		{
			InitialStateIds = new List<int>();
			foreach (DeltaFunctionTriplet dft in Triplets)
			{
				List<int> To = new List<int>();
				if (DeltaFunction.TryGetValue(dft.From, out SortedList<char, List<int>> l))
				{
					if (l.ContainsKey(dft.By))
					{
						To = l[dft.By];
						l.Remove(dft.By);
					}
					To.Add(dft.To);
					l.Add(dft.By, To);  // co s duplicitami??? - snad vyreseno
				}
				else
				{
					l = new SortedList<char, List<int>>();
					To.Add(dft.To);
					l.Add(dft.By, To);
					DeltaFunction.Add(dft.From, l);
				}
			}
			foreach (State s in States)
			{
				if (s.IsInitial)
				{
					InitialStateIds.Add(s.Id);
				}
			}

			EpsilonDeltaFunction = pairs.ToDictionary(p => p.Key, p => p.Value);
		}

		public void Save2Xml(XmlWriter Writer)
		{
			base.Save2Xml(Writer);
			Writer.WriteStartElement("DeltaFunction");
			foreach (KeyValuePair<int, SortedList<char, List<int>>> pair1 in DeltaFunction)
			{
				foreach (KeyValuePair<char, List<int>> pair2 in pair1.Value)
				{
					foreach(int pair3 in pair2.Value)
					{
						int From = pair1.Key;
						char By = pair2.Key;
						int To = pair3;
						Writer.WriteStartElement("DeltaFunctionTriplet");
						Writer.WriteAttributeString("From", From.ToString());
						Writer.WriteAttributeString("By", By.ToString());
						Writer.WriteAttributeString("To", To.ToString());
						Writer.WriteEndElement();
					}
				}
			}
			Writer.WriteEndElement();
		}

		private void IsValidAutomaton()
		{
			if (InitialStateIds.Count == 0 || FinalStates.Count == 0)
			{
				throw new NoValidAutomatonException();
			}

			if(Alphabet.Trim().Equals(""))
			{
				throw new NoValidAutomatonException();
			}
		}

		public bool Accepts(string input)
		{
			try
			{
				IsValidAutomaton();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}

			List<int> currenentStateIds = new List<int>();
			foreach(int id in InitialStateIds)
			{
				currenentStateIds.Add(id);
			}

			foreach(char c in input)
			{
				while(HasEpsilonTransition(currenentStateIds))
				{
					 GoThroughEpsilon(currenentStateIds);
				}
				currenentStateIds = DoDeltaFunction(currenentStateIds, c);
			}

			return FinalStates.Any(x => currenentStateIds.Contains(x.Id));
		}

		private List<int> DoDeltaFunction(List<int> currentStates, char by)
		{
			List<int> NewCurrentStates = new List<int>();
			foreach(int id in currentStates)
			{
				SortedList<char, List<int>> EndStates = DeltaFunction[id];
				if(!EndStates.ContainsKey(by))
				{
					continue;
				}
				List<int> EndStatesIds = EndStates[by];

				foreach (int item in EndStatesIds)
				{
					if(!NewCurrentStates.Contains(item))
					{
						NewCurrentStates.Add(item);
					}
				}
			}

			return NewCurrentStates;
		}

		private bool HasEpsilonTransition(List<int> currentStateIds)
		{
			foreach(int id in currentStateIds)
			{
				if (EpsilonDeltaFunction.ContainsKey(id))
				{
					return true;
				}
			}
			return false;
		}

		private void GoThroughEpsilon(List<int> currentStateIds)
		{
			List<int> currentStateIds_copy = new List<int>();

			foreach(int id in currentStateIds)
			{
				currentStateIds_copy.Add(id);
			}

			foreach(int id in currentStateIds_copy)
			{
				List<int> NewEndStates;
				if(EpsilonDeltaFunction.ContainsKey(id))
				{
					NewEndStates = new List<int>();
					NewEndStates = EpsilonDeltaFunction[id];
					currentStateIds.Remove(id);
					foreach(int item in NewEndStates)
					{
						if(!currentStateIds.Contains(item))
						{
							currentStateIds.Add(item);
						}
					}
				}
			}
		}
	}
}
