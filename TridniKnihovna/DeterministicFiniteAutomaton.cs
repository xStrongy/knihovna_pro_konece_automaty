using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TridniKnihovna
{
    public class DeterministicFiniteAutomaton : AbstractFiniteAutomaton
    {
		protected Dictionary<int, SortedList<char, int>> DeltaFunction = new Dictionary<int, SortedList<char, int>>();

		protected int InitialStateId;

		/*
		internal DeterministicFiniteAutomaton(XmlReader Reader)
		{

		}
		*/

		public DeterministicFiniteAutomaton(IEnumerable<State> States, string Alphabet, IEnumerable<DeltaFunctionTriplet> Triplets)
			: base(States, Alphabet)
		{
			foreach (DeltaFunctionTriplet dft in Triplets)
			{
				if (DeltaFunction.TryGetValue(dft.From, out SortedList<char, int> l))
				{
					l.Add(dft.By, dft.To);  // co s duplicitami???
				}
				else
				{
					l = new SortedList<char, int>();
					l.Add(dft.By, dft.To);
					DeltaFunction.Add(dft.From, l);
				}
			}
			foreach (State s in States)
			{
				if (s.IsInitial)
				{
					InitialStateId = s.Id;
					break;
				}
			}
		}

		public void Save2Xml()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;

			XmlWriter Writer = XmlWriter.Create("test2.xml", settings);

			Writer.WriteStartDocument();

			Writer.WriteStartElement("Automaton");

			Writer.WriteAttributeString("Type", "deterministic");

			base.Save2Xml(Writer);

			Writer.WriteStartElement("DeltaFunction");
			foreach (KeyValuePair<int, SortedList<char, int>> pair1 in DeltaFunction)
			{
				foreach (KeyValuePair<char, int> pair2 in pair1.Value)
				{
					int From = pair1.Key;
					char By = pair2.Key;
					int To = pair2.Value;
					Writer.WriteStartElement("DeltaFunctionTriplet");
					Writer.WriteAttributeString("From", From.ToString());
					Writer.WriteAttributeString("By", By.ToString());
					Writer.WriteAttributeString("To", To.ToString());
					Writer.WriteEndElement();
				}
			}
			Writer.WriteEndElement();
			Writer.WriteEndElement();
			Writer.WriteEndDocument();
			Writer.Close();
		}

		public bool Accepts(string input)
		{

			int currenentStateId = this.InitialStateId;
			foreach (char c in input)
			{
				currenentStateId = DoDeltaFunction(currenentStateId, c);
			}

			return AcceptStates.Any(x => x.Id == currenentStateId);
		}

		private int DoDeltaFunction(int currentStateId, char by)
		{
			SortedList<char, int> EndState = DeltaFunction[currentStateId];
			currentStateId = EndState[by];
			return currentStateId;
		}
	}
}
