using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TridniKnihovna
{
   public class AbstractFiniteAutomaton
    {
		protected Dictionary<int, State> States = new Dictionary<int, State>();

		protected string Alphabet;

		public AbstractFiniteAutomaton(IEnumerable<State> States, string Alphabet)
		{
			foreach (State s in States)
			{
				this.States.Add(s.Id, s);
			}
			this.Alphabet = Alphabet;
		}

		public IReadOnlyList<State> AcceptStates
		{
			get
			{
				List<State> f = new List<State>();
				foreach (State s in States.Values)
				{
					if (s.IsAccept)
					{
						f.Add(s);
					}
				}
				return f;
			}
		}

		public void Save2Xml(XmlWriter Writer)
		{
			Writer.WriteStartElement("States");
			foreach (State s in States.Values)
			{
				s.Save2Xml(Writer);
			}
			Writer.WriteEndElement();
			Writer.WriteStartElement("Alphabet");
			Writer.WriteString(Alphabet);
			Writer.WriteEndElement();
		}
	}
}

