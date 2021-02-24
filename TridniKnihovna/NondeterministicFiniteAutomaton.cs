using System;
using System.Collections.Generic;
using System.IO;
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
			EpsilonDeltaFunction = pairs.ToDictionary(p => p.Key, p => p.Value);
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

		}
		public void Save2Xml()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;
			XmlWriter Writer = XmlWriter.Create("test.xml", settings);
			Writer.WriteStartDocument();
			Writer.WriteStartElement("Automaton");
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
			Writer.WriteStartElement("EpsilonDeltaFunction");

			foreach(KeyValuePair<int, List<int>> pair1 in EpsilonDeltaFunction)
            {
				foreach(int id in pair1.Value)
                {
					int From = pair1.Key;
					int To = id;
					Writer.WriteStartElement("EpsilonDeltaFunctionPair");
					Writer.WriteAttributeString("From", From.ToString());
					Writer.WriteAttributeString("To", To.ToString());
					Writer.WriteEndElement();
                }
            }

			Writer.WriteEndElement();
			Writer.WriteEndElement();
			Writer.WriteEndDocument();
			Writer.Close();
		}

		private void IsValidAutomaton()
		{
			if (InitialStateIds.Count == 0 || AcceptStates.Count == 0)
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

			List<int> currentStateIds = new List<int>();
			foreach(int id in InitialStateIds)
			{
				currentStateIds.Add(id);
			}

			ExpandInitialOrFinalStatesByEpsilonTransitions(currentStateIds);

			foreach(char c in input)
			{
				while(HasEpsilonTransition(currentStateIds))
				{
					 GoThroughEpsilon(currentStateIds);
				}
				currentStateIds = DoDeltaFunction(currentStateIds, c);
				if(currentStateIds.Count == 0)
                {
					return false;
                }
			}

			ExpandInitialOrFinalStatesByEpsilonTransitions(currentStateIds);

			return AcceptStates.Any(x => currentStateIds.Contains(x.Id));
		}

		private List<int> DoDeltaFunction(List<int> currentStates, char by)
		{
			List<int> NewCurrentStates = new List<int>();
			foreach(int id in currentStates)
			{
				if(!DeltaFunction.ContainsKey(id))
                {
					continue;
                }
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

		private List<int> AddStatesToCurrent(List<int> LastAddedStates,List<int> currentStateIds)
        {
			List<int> NewCurrentState = new List<int>();

			foreach(int id in LastAddedStates)
            {
				List<int> EndStates;
				if(EpsilonDeltaFunction.ContainsKey(id))
                {
					EndStates = EpsilonDeltaFunction[id];
					foreach(int item in EndStates)
                    {
						if(!currentStateIds.Contains(item))
                        {
							NewCurrentState.Add(item);
							currentStateIds.Add(item);
                        }
                    }
				}
            }

			return NewCurrentState;
		}

		private void ExpandInitialOrFinalStatesByEpsilonTransitions(List<int> currentStateIds)
		{
			List<int> NewCurrentStates = new List<int>();

			foreach (int id in currentStateIds)
			{
				NewCurrentStates.Add(id);
			}

			while (HasEpsilonTransition(NewCurrentStates))
			{
				NewCurrentStates = AddStatesToCurrent(NewCurrentStates, currentStateIds);
			}
		}

		public void DeleteUnattainableStates()
        {
			List<int> attainableStatesIds = new List<int>();
			List<int> currentStatesIds = new List<int>();

			foreach(int id in InitialStateIds)
            {
				currentStatesIds.Add(id);
				attainableStatesIds.Add(id);
			}

			while(currentStatesIds.Count != 0)
            {
				currentStatesIds = ExpandAttainableStates(currentStatesIds, attainableStatesIds);
            }

			Dictionary<int, SortedList<char, List<int>>> NewDeltaFunction = new Dictionary<int, SortedList<char, List<int>>>();

			Dictionary<int, List<int>> NewEpsilonDeltaFunction = new Dictionary<int, List<int>>();

			foreach(int id in attainableStatesIds)
            {
				if(DeltaFunction.ContainsKey(id))
                {
					NewDeltaFunction[id] = DeltaFunction[id];
                }

				if(EpsilonDeltaFunction.ContainsKey(id))
                {
					NewEpsilonDeltaFunction[id] = EpsilonDeltaFunction[id];
                }
            }

			DeltaFunction = NewDeltaFunction;

			EpsilonDeltaFunction = NewEpsilonDeltaFunction;

			foreach(KeyValuePair<int, State> pair in States)
			{
				if(!attainableStatesIds.Contains(pair.Key))
                {
					States.Remove(pair.Key);
                }
            }
				
		}

		public void DeleteUnnecessaryStates() // JESTE TREBA DODELAT !
        {
			List<int> necessaryStatesIds = new List<int>();

			List<int> currentStatesIds = new List<int>();

			Dictionary<int, SortedList<char, List<int>>> NewDeltaFunction = new Dictionary<int, SortedList<char, List<int>>>();

			Dictionary<int, List<int>> NewEpsilonDeltaFunction = new Dictionary<int, List<int>>();

			foreach (State s in AcceptStates)
            {
				necessaryStatesIds.Add(s.Id);
				currentStatesIds.Add(s.Id);
            }

			while (currentStatesIds.Count != 0)
			{
				currentStatesIds = ExpandNecessaryStates(currentStatesIds, necessaryStatesIds, 
					NewDeltaFunction, NewEpsilonDeltaFunction);
			}
			

			DeltaFunction = NewDeltaFunction;

			EpsilonDeltaFunction = NewEpsilonDeltaFunction;

			foreach (KeyValuePair<int, State> pair in States)
			{
				if (!necessaryStatesIds.Contains(pair.Key))
				{
					States.Remove(pair.Key);
				}
			}
		}

        private List<int> ExpandNecessaryStates(List<int> currentStatesIds, List<int> necessaryStatesIds,
			Dictionary<int, SortedList<char, List<int>>> NewDeltaFunction, Dictionary<int, List<int>> NewEpsilonDeltaFunction)
        {
			List<int> NewCurrentStates = new List<int>();
			
			foreach(KeyValuePair<int, SortedList<char, List<int>>> pair1 in DeltaFunction)
            {
				foreach (KeyValuePair<char, List<int>> pair2 in pair1.Value)
                {
					foreach (int id in pair2.Value)
                    {
						if(currentStatesIds.Contains(id))
                        {
							if (!NewCurrentStates.Contains(pair1.Key) && !necessaryStatesIds.Contains(pair1.Key))
							{
								NewCurrentStates.Add(pair1.Key);
								necessaryStatesIds.Add(pair1.Key);
							}
						}
                    }
                }
            }

			foreach (KeyValuePair<int, List<int>> pair1 in EpsilonDeltaFunction)
			{
				foreach(int id in pair1.Value)
                {
					if(currentStatesIds.Contains(id))
                    {
						if(!NewCurrentStates.Contains(pair1.Key) && !necessaryStatesIds.Contains(pair1.Key))
                        {
							NewCurrentStates.Add(pair1.Key);
							necessaryStatesIds.Add(pair1.Key);
                        }
					}
                }
			}

			return NewCurrentStates;
        }

        private List<int> ExpandAttainableStates(List<int> currentStatesIds, List<int> attainableStatesIds)
        {
			List<int> NewCurrentStates = new List<int>();
			foreach(int id in currentStatesIds)
            {
				if(DeltaFunction.ContainsKey(id))
                {
					SortedList<char, List<int>> EndStates = DeltaFunction[id];

					foreach(KeyValuePair<char, List<int>> pair1 in EndStates)
                    {
						foreach(int item in pair1.Value)
                        {
							if(!attainableStatesIds.Contains(item))
                            {
								attainableStatesIds.Add(item);
								NewCurrentStates.Add(item);
                            }
                        }
                    }
                }

				if(EpsilonDeltaFunction.ContainsKey(id))
                {
					List<int> EndStates = EpsilonDeltaFunction[id];

					foreach(int item in EndStates)
                    {
						if(!attainableStatesIds.Contains(item))
                        {
							attainableStatesIds.Add(item);
							NewCurrentStates.Add(item);
						}
                    }
                }
            }

			return NewCurrentStates;
        }
	}
}
