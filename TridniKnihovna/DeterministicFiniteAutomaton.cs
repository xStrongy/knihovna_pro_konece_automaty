using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace TridniKnihovna
{
    public class DeterministicFiniteAutomaton : AbstractFiniteAutomaton
    {
        protected Dictionary<int, SortedList<char, int>> DeltaFunction = new Dictionary<int, SortedList<char, int>>();

        protected int InitialStateId;

        public DeterministicFiniteAutomaton(IEnumerable<State> States, string Alphabet, IEnumerable<DeltaFunctionTriplet> Triplets)
            : base(States, Alphabet)
        {
            foreach (DeltaFunctionTriplet dft in Triplets)
            {
                if (DeltaFunction.TryGetValue(dft.From, out SortedList<char, int> l))
                {
                    l.Add(dft.By, dft.To);
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

        public void Save2Xml(string xmlPath)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            XmlWriter Writer = XmlWriter.Create(xmlPath, settings);

            Writer.WriteStartDocument();

            Writer.WriteStartElement("Automaton");

            Writer.WriteAttributeString("Type", "Deterministic");

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

        static DeterministicFiniteAutomaton LoadFromXML(string xmlPath)
        {
            StreamReader xmlStreamReader = new StreamReader(xmlPath);
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(xmlStreamReader);
            XmlNodeList nodes = xmlDoc.DocumentElement.ChildNodes;

            string type = xmlDoc.DocumentElement.GetAttribute("Type");

            if (!type.Equals("Deterministic"))
            {
                Console.WriteLine("Automaton cannot be loaded, because it is not deterministic");
                return null;
            }

            List<State> states = new List<State>();

            foreach (XmlNode stateNode in nodes.Item(0))
            {
                State s = new State(stateNode);
                states.Add(s);
            }

            XmlNode alphabetNode = nodes.Item(1);
            string Alphabet = alphabetNode.InnerText;

            List<DeltaFunctionTriplet> dft = new List<DeltaFunctionTriplet>();
            foreach (XmlNode dftNode in nodes.Item(2))
            {
                int From = int.Parse(dftNode.Attributes["From"].Value);
                char By = char.Parse(dftNode.Attributes["By"].Value);
                int To = int.Parse(dftNode.Attributes["To"].Value);

                dft.Add(new DeltaFunctionTriplet(From, By, To));
            }


            return new DeterministicFiniteAutomaton(states, Alphabet, dft);
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

        public void DeleteEquivalentStates()
        {
            Dictionary<int, List<int>> GroupRecorder = new Dictionary<int, List<int>>();

            Dictionary<int, SortedList<char, int>> Table = new Dictionary<int, SortedList<char, int>>();

            foreach (KeyValuePair<int, State> s in States)
            {
                if (s.Value.IsAccept)
                {
                    if (GroupRecorder.TryGetValue(2, out List<int> value))
                    {
                        value.Add(s.Key);
                    }
                    else
                    {
                        value = new List<int>();
                        value.Add(s.Key);
                        GroupRecorder.Add(2, value);
                    }
                }
                else
                {
                    if (GroupRecorder.TryGetValue(1, out List<int> value))
                    {
                        value.Add(s.Key);
                    }
                    else
                    {
                        value = new List<int>();
                        value.Add(s.Key);
                        GroupRecorder.Add(1, value);
                    }
                }
            }

            bool groupCreated;

            do
            {
                groupCreated = false;

                EditTable(GroupRecorder, Table);

                List<int> differentStates = new List<int>();

                FindDifferentStates(differentStates, GroupRecorder, Table);

                DeleteDifferentStatesFromGroupRecorder(differentStates, GroupRecorder, Table);

                ClassifyDifferentStatesIntoRightGroups(differentStates, GroupRecorder, Table);

                while (differentStates.Count != 0)
                {
                    groupCreated = true;
                    CreateNewGroup(differentStates, GroupRecorder, Table);
                }

            } while (groupCreated);

            Dictionary<int, SortedList<char, int>> NewDeltaFunction = new Dictionary<int, SortedList<char, int>>();

            Dictionary<int, State> NewStates = new Dictionary<int, State>();

            foreach (KeyValuePair<int, List<int>> group in GroupRecorder)
            {
                bool hasInitialState = false;

                if (group.Value.Contains(InitialStateId))
                {
                    hasInitialState = true;
                }

                NewStates.Add(GroupRecorder[group.Key].ElementAt(0), States[GroupRecorder[group.Key].ElementAt(0)]);

                SortedList<char, int> NewEndStates = new SortedList<char, int>();

                foreach (char c in Alphabet)
                {
                    int j = GroupRecorder[group.Key].ElementAt(0);
                    int i = Table[j].GetValueOrDefault(c);
                    NewEndStates.Add(c, GroupRecorder[i].ElementAt(0));
                }

                NewDeltaFunction[GroupRecorder[group.Key].ElementAt(0)] = NewEndStates;

                if (hasInitialState == true)
                {
                    NewStates[GroupRecorder[group.Key].ElementAt(0)].IsInitial = true;
                    InitialStateId = NewStates[GroupRecorder[group.Key].ElementAt(0)].Id;
                }
            }

            DeltaFunction = NewDeltaFunction;

            States = NewStates;

        }

        private void ClassifyDifferentStatesIntoRightGroups(List<int> differentStates,
            Dictionary<int, List<int>> GroupRecorder,
            Dictionary<int, SortedList<char, int>> Table)
        {
            for (int i = 0; i < differentStates.Count; i++)
            {
                SortedList<char, int> StatePattern = Table[differentStates[i]];

                for (int j = 1; j <= GroupRecorder.Count; j++)
                {
                    SortedList<char, int> groupPattern = Table[GroupRecorder[j].ElementAt(0)];

                    List<int> groupMembers = GroupRecorder[j];

                    if (StatePattern.SequenceEqual(groupPattern))
                    {
                        groupMembers.Add(differentStates[i]);
                        differentStates.RemoveAt(i);
                        i--;
                    }

                    GroupRecorder[j] = groupMembers;
                }
            }
        }

        private void DeleteDifferentStatesFromGroupRecorder(List<int> differentStates,
            Dictionary<int, List<int>> GroupRecorder,
            Dictionary<int, SortedList<char, int>> Table)
        {
            for (int i = 1; i <= GroupRecorder.Count; i++)
            {
                List<int> groupMembers = GroupRecorder[i];

                for (int j = 0; j < groupMembers.Count; j++)
                {
                    if (differentStates.Contains(groupMembers[j]))
                    {
                        groupMembers.Remove(groupMembers[j]);
                        j--;
                    }
                }

                GroupRecorder[i] = groupMembers;
            }
        }

        private void FindDifferentStates(List<int> differentStates,
            Dictionary<int, List<int>> GroupRecorder,
            Dictionary<int, SortedList<char, int>> Table)
        {
            foreach (KeyValuePair<int, List<int>> groups in GroupRecorder)
            {
                SortedList<char, int> groupPattern = new SortedList<char, int>();

                foreach (int groupMember in groups.Value)
                {
                    if (groupPattern.Count == 0)
                    {
                        groupPattern = Table[groupMember];
                    }

                    if (!groupPattern.SequenceEqual(Table[groupMember]))
                    {
                        differentStates.Add(groupMember);
                    }
                }
            }
        }

        private void EditTable(Dictionary<int, List<int>> GroupRecorder, Dictionary<int, SortedList<char, int>> Table)
        {
            foreach (KeyValuePair<int, State> s in States)
            {
                SortedList<char, int> EndState = new SortedList<char, int>();

                foreach (char c in Alphabet)
                {
                    EndState[c] = GroupRecorder.FirstOrDefault(x => x.Value.Contains(DoDeltaFunction(s.Key, c)) == true).Key;
                }

                Table[s.Key] = EndState;
            }
        }

        private int DoDeltaFunction(int currentStateId, char by)
        {
            SortedList<char, int> EndState = DeltaFunction[currentStateId];
            currentStateId = EndState[by];
            return currentStateId;
        }

        private void CreateNewGroup(List<int> differentStates,
            Dictionary<int, List<int>> GroupRecorder,
            Dictionary<int, SortedList<char, int>> Table)
        {
            List<int> NewStates = new List<int>();

            NewStates.Add(differentStates.ElementAt(0));

            GroupRecorder[GroupRecorder.Count + 1] = NewStates;

            differentStates.RemoveAt(0);

            for (int i = 0; i < differentStates.Count; i++)
            {
                SortedList<char, int> StatePattern = Table[differentStates[i]];

                if (StatePattern.SequenceEqual(Table[GroupRecorder[GroupRecorder.Count].ElementAt(0)]))
                {
                    NewStates.Add(differentStates[i]);
                    differentStates.RemoveAt(i);
                    i--;
                }

                GroupRecorder[GroupRecorder.Count] = NewStates;
            }
        }

        public string GetDotSourceCode()
        {
            StringBuilder sb = new StringBuilder();

            int shadowCounter = 0;

            sb.Append("digraph finite_state_machine {\n");
            sb.Append("rankdir=LR\n");
            sb.Append("size=\"8,5\"\n");
            sb.Append("node [shape = doublecircle]; ");

            int i = 0;
            foreach (State s in AcceptStates)
            {
                if (i == AcceptStates.Count - 1)
                {
                    sb.Append(s.Label + ";\n");
                }
                else
                {
                    sb.Append(s.Label + " ");
                }
                i++;
            }

            sb.Append("node [shape = circle];\n");


            shadowCounter++;
            sb.Append("shadow" + shadowCounter + " -> " + States[InitialStateId].Label + " [ label = \"\" ];\n");


            foreach (KeyValuePair<int, SortedList<char, int>> pair1 in DeltaFunction)
            {
                foreach (KeyValuePair<char, int> pair2 in pair1.Value)
                {
                    sb.Append(States[pair1.Key].Label + " -> " + States[pair2.Value].Label + "[ label = \"" + pair2.Key + "\" ];\n");
                }
            }

            sb.Append("shadow" + 1 + " [ style = invis ];\n");

            sb.Append("}");
            return sb.ToString();
        }
    }
}
