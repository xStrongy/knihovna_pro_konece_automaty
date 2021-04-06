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
        /// <summary>
        /// Saves automaton into XML file
        /// </summary>
        public void Save2Xml()
        {
            if (!IsValidAutomaton())
            {
                Console.WriteLine("Automaton was not defined corectly");
                return;
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            XmlWriter Writer = XmlWriter.Create("test.xml", settings);

            Writer.WriteStartDocument();

            Writer.WriteStartElement("Automaton");

            Writer.WriteAttributeString("Type", "Nondeterministic");

            base.Save2Xml(Writer);

            Writer.WriteStartElement("DeltaFunction");

            foreach (KeyValuePair<int, SortedList<char, List<int>>> pair1 in DeltaFunction)
            {
                foreach (KeyValuePair<char, List<int>> pair2 in pair1.Value)
                {
                    foreach (int pair3 in pair2.Value)
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

            foreach (KeyValuePair<int, List<int>> pair1 in EpsilonDeltaFunction)
            {
                foreach (int id in pair1.Value)
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

        /// <summary>
        /// Loads an instance of automaton from XML file
        /// </summary>
        /// <param name="xmlPath">a path of XML file</param>
        /// <returns>returns new instance of nondeterministic finite automaton</returns>
        static NondeterministicFiniteAutomaton LoadFromXml(string xmlPath)
        {
            StreamReader xmlStreamReader = new StreamReader(xmlPath);
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(xmlStreamReader);
            XmlNodeList nodes = xmlDoc.DocumentElement.ChildNodes;

            string type = xmlDoc.DocumentElement.GetAttribute("Type");

            if (!type.Equals("Nondeterministic"))
            {
                Console.WriteLine("Automaton cannot be loaded, because it is not nondeterministic");
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

            SortedList<int, List<int>> EpsilonTransition = new SortedList<int, List<int>>();
            foreach (XmlNode edfp in nodes.Item(3))
            {
                int From = int.Parse(edfp.Attributes["From"].Value);
                int To = int.Parse(edfp.Attributes["To"].Value);

                if (EpsilonTransition.TryGetValue(From, out List<int> value))
                {
                    value.Add(To);
                }
                else
                {
                    value = new List<int>();
                    value.Add(To);
                    EpsilonTransition.Add(From, value);
                }

            }
            return new NondeterministicFiniteAutomaton(states, Alphabet, dft, EpsilonTransition);
        }


        /// <summary>
        /// Determinates if the automaton accepts current word from alphabet
        /// </summary>
        /// <param name="input">a word from alphabet</param>
        /// <returns>returns true if the automaton accepted the word</returns>
        public bool Accepts(string input)
        {
            if (!IsValidAutomaton())
            {
                Console.WriteLine("Automaton was not defined corectly");
                return false;
            }

            List<int> currentStateIds = new List<int>(InitialStateIds);

            ExpandCurrentStatesByEpsilonTransitions(currentStateIds);

            foreach (char c in input)
            {
                while (HasEpsilonTransition(currentStateIds))
                {
                    GoThroughEpsilon(currentStateIds);
                }
                currentStateIds = DoDeltaFunction(currentStateIds, c);
                if (currentStateIds.Count == 0)
                {
                    return false;
                }
            }

            ExpandCurrentStatesByEpsilonTransitions(currentStateIds);

            return AcceptStates.Any(x => currentStateIds.Contains(x.Id));
        }

        /// <summary>
        /// returns source code for dot language
        /// </summary>
        /// <returns>returns source code as string</returns>
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

            foreach (int id in InitialStateIds)
            {
                shadowCounter++;
                sb.Append("shadow" + shadowCounter + " -> " + States[id].Label + " [ label = \"\" ];\n");
            }

            foreach (KeyValuePair<int, SortedList<char, List<int>>> pair1 in DeltaFunction)
            {
                foreach (KeyValuePair<char, List<int>> pair2 in pair1.Value)
                {
                    foreach (int EndStateId in pair2.Value)
                    {
                        sb.Append(States[pair1.Key].Label + " -> " + States[EndStateId].Label + "[ label = \"" + pair2.Key + "\" ];\n");
                    }
                }
            }

            foreach (KeyValuePair<int, List<int>> pair1 in EpsilonDeltaFunction)
            {
                foreach (int EndStateId in pair1.Value)
                {
                    sb.Append(States[pair1.Key].Label + " -> " + States[EndStateId].Label + "[ label = \"ε\" ];\n");
                }
            }

            for (int index = 1; index <= shadowCounter; index++)
            {
                sb.Append("shadow" + index + " [ style = invis ];\n");
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// A function for reducing automaton
        /// </summary>
        public void ReduceAutomaton()
        {
            if (!IsValidAutomaton())
            {
                Console.WriteLine("Automaton was not defined corectly");
                return;
            }

            DeleteUnattainableStates();

            DeleteUnnecessaryStates();

        }

        public void DeleteEpsilonTransitions()
        {
            Dictionary<int, List<int>> EpsilonSeals = new Dictionary<int, List<int>>();

            List<int> currentStatesIds = new List<int>();

            Dictionary<int, SortedList<char, List<int>>> NewDeltaFunction = new Dictionary<int, SortedList<char, List<int>>>();

            foreach (KeyValuePair<int, State> state in States)
            {
                currentStatesIds.Add(state.Key);

                ExpandCurrentStatesByEpsilonTransitions(currentStatesIds);

                if (currentStatesIds.Count == 1)
                {
                    currentStatesIds.Clear();
                }
                else
                {
                    currentStatesIds.Remove(state.Key);

                    EpsilonSeals.Add(state.Key, new List<int>(currentStatesIds));

                    currentStatesIds.Clear();
                }
            }

            foreach (KeyValuePair<int, List<int>> EpsilonSeal in EpsilonSeals)
            {
                if (States[EpsilonSeal.Key].IsInitial)
                {
                    foreach (int id in EpsilonSeal.Value)
                    {
                        States[id].IsInitial = true;
                        if (!InitialStateIds.Contains(id))
                        {
                            InitialStateIds.Add(id);
                        }
                    }
                }
            }

            SortedList<int, List<char>> LinkedStates = new SortedList<int, List<char>>();

            foreach (KeyValuePair<int, List<int>> EpsilonSeal in EpsilonSeals)
            {
                LinkedStates = FindLinkedStates(EpsilonSeal.Key);

                foreach (KeyValuePair<int, List<char>> LinkedState in LinkedStates)
                {
                    foreach (char c in LinkedState.Value)
                    {
                        foreach (int id in EpsilonSeal.Value)
                        {
                            if (NewDeltaFunction.TryGetValue(LinkedState.Key, out SortedList<char, List<int>> value))
                            {
                                if (NewDeltaFunction[LinkedState.Key].TryGetValue(c, out List<int> value1))
                                {
                                    if (!value1.Contains(id))
                                    {
                                        value1.Add(id);
                                    }
                                }
                                else
                                {
                                    value1 = new List<int>();
                                    value1.Add(id);
                                    NewDeltaFunction[LinkedState.Key].Add(c, value1);
                                }
                            }
                            else
                            {
                                List<int> ids = new List<int>();
                                ids.Add(id);
                                value = new SortedList<char, List<int>>();
                                value.Add(c, ids);
                                NewDeltaFunction.Add(LinkedState.Key, value);
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<int, SortedList<char, List<int>>> pair1 in NewDeltaFunction)
            {
                foreach (KeyValuePair<char, List<int>> pair2 in pair1.Value)
                {
                    SortedList<char, List<int>> EndStates = DeltaFunction[pair1.Key];
                    List<int> EndStateIds = EndStates[pair2.Key];
                    foreach (int id in pair2.Value)
                    {
                        if (!EndStateIds.Contains(id))
                        {
                            EndStateIds.Add(id);
                        }
                    }
                    EndStates[pair2.Key] = EndStateIds;
                    DeltaFunction[pair1.Key] = EndStates;
                }
            }

            EpsilonDeltaFunction.Clear();
        }

        private SortedList<int, List<char>> FindLinkedStates(int key)
        {
            SortedList<int, List<char>> LinkedStates = new SortedList<int, List<char>>();

            foreach (KeyValuePair<int, SortedList<char, List<int>>> pair1 in DeltaFunction)
            {
                foreach (KeyValuePair<char, List<int>> pair2 in pair1.Value)
                {
                    foreach (int id in pair2.Value)
                    {
                        if (id == key)
                        {
                            if (LinkedStates.TryGetValue(pair1.Key, out List<char> value))
                            {
                                value.Add(pair2.Key);
                            }
                            else
                            {
                                value = new List<char>();
                                value.Add(pair2.Key);
                                LinkedStates.Add(pair1.Key, value);
                            }
                        }
                    }
                }
            }

            return LinkedStates;
        }

        /// <summary>
        /// Converts nondeterministic automaton into deterministic automaton
        /// </summary>
        /// <returns>A new instance of deterministic automaton</returns>
        public DeterministicFiniteAutomaton ConvertToDeterministicFiniteAutomaton()
        {

            if (!IsValidAutomaton())
            {
                Console.WriteLine("Automaton was not defined corectly");
                return null;
            }

            List<DeltaFunctionTriplet> NewDFT = new List<DeltaFunctionTriplet>();

            Dictionary<string, int> StateRecorder = new Dictionary<string, int>();

            List<State> NewStates = new List<State>();

            List<int> currentStateIds = new List<int>(InitialStateIds);

            int stateCounter = 0;

            for (int i = 0; i <= stateCounter; i++)
            {
                if (i == stateCounter && i != 0)
                {
                    break;
                }
                if (i != 0)
                {
                    foreach (KeyValuePair<string, int> kvp in StateRecorder)
                    {
                        if (kvp.Value == i + 1)
                        {
                            string key = kvp.Key;

                            currentStateIds.Clear();

                            string[] arrayOfIds = key.Split("+");

                            foreach (string s in arrayOfIds)
                            {
                                currentStateIds.Add(int.Parse(s));
                            }

                            break;
                        }
                    }
                }

                ExpandCurrentStatesByEpsilonTransitions(currentStateIds);

                string keyStateId = BuildStateId(currentStateIds);

                if (i == 0)
                {
                    stateCounter++;
                    int value = stateCounter;
                    StateRecorder.Add(keyStateId, value);
                    NewStates.Add(new State(
                        StateRecorder[keyStateId],
                        "q" + StateRecorder[keyStateId],
                        true,
                        AcceptStates.Any(x => currentStateIds.Contains(x.Id))));
                }

                HashSet<int> AllEndStatesIds = new HashSet<int>();

                foreach (char c in Alphabet)
                {
                    foreach (int id in currentStateIds)
                    {
                        if (!DeltaFunction.ContainsKey(id))
                        {
                            continue;
                        }
                        SortedList<char, List<int>> EndStates = DeltaFunction[id];

                        if (!EndStates.ContainsKey(c))
                        {
                            continue;
                        }

                        ExpandCurrentStatesByEpsilonTransitions(EndStates[c]);


                        AllEndStatesIds.UnionWith(EndStates[c]);
                    }

                    string valueStateId = BuildStateId(AllEndStatesIds.ToList());

                    if (valueStateId.Equals(""))
                    {
                        valueStateId = "0";
                    }

                    if (!StateRecorder.ContainsKey(valueStateId))
                    {
                        if (!valueStateId.Equals("0"))
                        {

                            string[] arrayOfIds = valueStateId.Split("+");

                            bool isAccept = AcceptStates.Any(x => arrayOfIds.ToList().Contains(x.Id.ToString()));

                            stateCounter++;

                            StateRecorder.Add(valueStateId, stateCounter);

                            NewStates.Add(new State(
                           StateRecorder[valueStateId],
                           "q" + StateRecorder[valueStateId],
                           false,
                           isAccept));
                        }
                        else
                        {
                            StateRecorder.Add(valueStateId, 0);

                            NewStates.Add(new State(0, char.ConvertFromUtf32(216).ToString(), false, false));
                        }
                    }

                    AllEndStatesIds.Clear();

                    NewDFT.Add(new DeltaFunctionTriplet(StateRecorder[keyStateId], c, StateRecorder[valueStateId]));
                }
            }

            if (StateRecorder.ContainsKey("0"))
            {
                foreach (char c in Alphabet)
                {
                    NewDFT.Add(new DeltaFunctionTriplet(0, c, 0));
                }
            }
            return new DeterministicFiniteAutomaton(NewStates, Alphabet, NewDFT);
        }

        /// <summary>
        /// Deletes unnattainable states from automaton
        /// </summary>
        public void DeleteUnattainableStates()
        {
            List<int> attainableStatesIds = new List<int>(InitialStateIds);
            List<int> currentStatesIds = new List<int>(InitialStateIds);

            while (currentStatesIds.Count != 0)
            {
                currentStatesIds = ExpandAttainableStates(currentStatesIds, attainableStatesIds);
            }

            Dictionary<int, SortedList<char, List<int>>> NewDeltaFunction = new Dictionary<int, SortedList<char, List<int>>>();

            Dictionary<int, List<int>> NewEpsilonDeltaFunction = new Dictionary<int, List<int>>();

            foreach (int id in attainableStatesIds)
            {
                if (DeltaFunction.ContainsKey(id))
                {
                    NewDeltaFunction[id] = DeltaFunction[id];
                }

                if (EpsilonDeltaFunction.ContainsKey(id))
                {
                    NewEpsilonDeltaFunction[id] = EpsilonDeltaFunction[id];
                }
            }

            DeltaFunction = NewDeltaFunction;

            EpsilonDeltaFunction = NewEpsilonDeltaFunction;

            foreach (KeyValuePair<int, State> pair in States)
            {
                if (!attainableStatesIds.Contains(pair.Key))
                {
                    States.Remove(pair.Key);
                }
            }

        }

        /// <summary>
        /// Deletes unnecessary states from automaton
        /// </summary>
        public void DeleteUnnecessaryStates()
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

        /// <summary>
        /// Expands list of necessary states
        /// </summary>
        /// <param name="currentStatesIds">List of current states, which are worked with </param>
        /// <param name="necessaryStatesIds">List of all existing necessary states</param>
        /// <param name="NewDeltaFunction">A new delta function which will replace the old one</param>
        /// <param name="NewEpsilonDeltaFunction">A new delta function for epsilon transitions which will replace the old one</param>
        /// <returns>returns a list of new current states for one iteration</returns>
        private List<int> ExpandNecessaryStates(List<int> currentStatesIds, List<int> necessaryStatesIds,
            Dictionary<int, SortedList<char, List<int>>> NewDeltaFunction, Dictionary<int, List<int>> NewEpsilonDeltaFunction)
        {
            List<int> NewCurrentStates = new List<int>();

            foreach (KeyValuePair<int, SortedList<char, List<int>>> pair1 in DeltaFunction)
            {
                foreach (KeyValuePair<char, List<int>> pair2 in pair1.Value)
                {
                    foreach (int id in pair2.Value)
                    {
                        if (currentStatesIds.Contains(id))
                        {
                            if (!NewCurrentStates.Contains(pair1.Key) && !necessaryStatesIds.Contains(pair1.Key))
                            {
                                NewCurrentStates.Add(pair1.Key);
                                necessaryStatesIds.Add(pair1.Key);
                            }
                            if (NewDeltaFunction.TryGetValue(pair1.Key, out SortedList<char, List<int>> list1))
                            {
                                if (list1.TryGetValue(pair2.Key, out List<int> list2))
                                {
                                    if (list2.Contains(id))
                                    {
                                        continue;
                                    }
                                    list2.Add(id);
                                }
                                else
                                {
                                    list2 = new List<int>();
                                    list2.Add(id);
                                    list1.Add(pair2.Key, list2);
                                    NewDeltaFunction.Add(pair1.Key, list1);
                                }
                            }
                            else
                            {
                                list1 = new SortedList<char, List<int>>();
                                List<int> list2 = new List<int>();
                                list2.Add(id);
                                list1.Add(pair2.Key, list2);
                                NewDeltaFunction.Add(pair1.Key, list1);
                            }

                        }
                    }
                }
            }


            foreach (KeyValuePair<int, List<int>> pair1 in EpsilonDeltaFunction)
            {
                foreach (int id in pair1.Value)
                {
                    if (currentStatesIds.Contains(id))
                    {
                        if (!NewCurrentStates.Contains(pair1.Key) && !necessaryStatesIds.Contains(pair1.Key))
                        {
                            NewCurrentStates.Add(pair1.Key);
                            necessaryStatesIds.Add(pair1.Key);
                        }
                        if (NewEpsilonDeltaFunction.TryGetValue(pair1.Key, out List<int> list))
                        {
                            if (list.Contains(id))
                            {
                                continue;
                            }
                            list.Add(id);
                        }
                        else
                        {
                            list = new List<int>();
                            list.Add(id);
                            NewEpsilonDeltaFunction.Add(pair1.Key, list);
                        }

                    }
                }
            }

            return NewCurrentStates;
        }

        /// <summary>
        /// Performs a delta function
        /// </summary>
        /// <param name="currentStates">A list of current states which are worked with</param>
        /// <param name="by">A character which indetifies a transition</param>
        /// <returns></returns>
        private List<int> DoDeltaFunction(List<int> currentStates, char by)
        {
            List<int> NewCurrentStates = new List<int>();

            foreach (int id in currentStates)
            {
                if (!DeltaFunction.ContainsKey(id))
                {
                    continue;
                }
                SortedList<char, List<int>> EndStates = DeltaFunction[id];
                if (!EndStates.ContainsKey(by))
                {
                    continue;
                }
                List<int> EndStatesIds = EndStates[by];

                foreach (int item in EndStatesIds)
                {
                    if (!NewCurrentStates.Contains(item))
                    {
                        NewCurrentStates.Add(item);
                    }
                }
            }

            return NewCurrentStates;
        }

        /// <summary>
        /// Determinates if any item of current states contains an epsilon transition
        /// </summary>
        /// <param name="currentStateIds">A list of current states which are worked with</param>
        /// <returns>return true if at least one item in list of current states has an epsilon transition</returns>
        private bool HasEpsilonTransition(List<int> currentStateIds)
        {
            foreach (int id in currentStateIds)
            {
                if (EpsilonDeltaFunction.ContainsKey(id))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Performs an epsilon transition
        /// </summary>
        /// <param name="currentStateIds">A list of current states which are worked with</param>
        private void GoThroughEpsilon(List<int> currentStateIds)
        {
            List<int> currentStateIds_copy = new List<int>(currentStateIds);

            foreach (int id in currentStateIds_copy)
            {
                List<int> NewEndStates;
                if (EpsilonDeltaFunction.ContainsKey(id))
                {
                    NewEndStates = EpsilonDeltaFunction[id];
                    currentStateIds.Remove(id);
                    foreach (int item in NewEndStates)
                    {
                        if (!currentStateIds.Contains(item))
                        {
                            currentStateIds.Add(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds new states as current states if any of existing current states contains an epsilon transitions
        /// </summary>
        /// <param name="LastAddedStates">A list of last added states to current states for knowing which states should not be to added again</param>
        /// <param name="currentStateIds">A list of current states which are worked with</param>
        /// <returns></returns>
        private List<int> AddStatesToCurrent(List<int> LastAddedStates, List<int> currentStateIds)
        {
            List<int> NewCurrentState = new List<int>();

            foreach (int id in LastAddedStates)
            {
                List<int> EndStates;
                if (EpsilonDeltaFunction.ContainsKey(id))
                {
                    EndStates = EpsilonDeltaFunction[id];
                    foreach (int item in EndStates)
                    {
                        if (!currentStateIds.Contains(item))
                        {
                            NewCurrentState.Add(item);
                            currentStateIds.Add(item);
                        }
                    }
                }
            }

            return NewCurrentState;
        }

        /// <summary>
        /// Expands existing list of current states by another states which has epsilon transition with old ones, but does not delete old ones
        /// </summary>
        /// <param name="currentStateIds">A list of current states which are worked with</param>
        private void ExpandCurrentStatesByEpsilonTransitions(List<int> currentStateIds)
        {
            List<int> NewCurrentStates = new List<int>(currentStateIds);

            while (HasEpsilonTransition(NewCurrentStates))
            {
                NewCurrentStates = AddStatesToCurrent(NewCurrentStates, currentStateIds);
            }
        }

        /// <summary>
        /// Expands a list of attainable states
        /// </summary>
        /// <param name="currentStatesIds">A list of current states which are worked with</param>
        /// <param name="attainableStatesIds">All existing attainable states</param>
        /// <returns></returns>
        private List<int> ExpandAttainableStates(List<int> currentStatesIds, List<int> attainableStatesIds)
        {
            List<int> NewCurrentStates = new List<int>();
            foreach (int id in currentStatesIds)
            {
                if (DeltaFunction.ContainsKey(id))
                {
                    SortedList<char, List<int>> EndStates = DeltaFunction[id];

                    foreach (KeyValuePair<char, List<int>> pair1 in EndStates)
                    {
                        foreach (int item in pair1.Value)
                        {
                            if (!attainableStatesIds.Contains(item))
                            {
                                attainableStatesIds.Add(item);
                                NewCurrentStates.Add(item);
                            }
                        }
                    }
                }

                if (EpsilonDeltaFunction.ContainsKey(id))
                {
                    List<int> EndStates = EpsilonDeltaFunction[id];

                    foreach (int item in EndStates)
                    {
                        if (!attainableStatesIds.Contains(item))
                        {
                            attainableStatesIds.Add(item);
                            NewCurrentStates.Add(item);
                        }
                    }
                }
            }

            return NewCurrentStates;
        }

        /// <summary>
        /// Creates an one Id for whole list of Ids
        /// </summary>
        /// <param name="listIds"></param>
        /// <returns>returns a string which is compiled by all items in list with a connecting character "+"</returns>
        private string BuildStateId(List<int> listIds)
        {
            listIds.Sort();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < listIds.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append(listIds[i]);
                }
                else
                {
                    sb.Append("+" + listIds[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determinates if an instance of automaton was defined corectly
        /// </summary>
        /// <returns>returns false if an instance of automaton was not defined corectly</returns>
        private bool IsValidAutomaton()
        {
            if (States.Count == 0 || InitialStateIds.Count == 0 || AcceptStates.Count == 0)
            {
                return false;
            }

            if (Alphabet.Trim().Equals(""))
            {
                return false;
            }

            if (DeltaFunction.Count() == 0 && EpsilonDeltaFunction.Count() == 0)
            {
                return false;
            }

            return true;
        }
        public List<State> GetStates()
        {
            List<State> returnStates = new List<State>();

            foreach(KeyValuePair<int, State> state in States)
            {
                returnStates.Add(state.Value);
            }

            return returnStates;
        }

        public List<DeltaFunctionTriplet> GetTriplets()
        {
            List<DeltaFunctionTriplet> returnTriplets = new List<DeltaFunctionTriplet>();

            foreach(KeyValuePair<int, SortedList<char, List<int>>> pair1 in DeltaFunction)
            {
                foreach(KeyValuePair<char, List<int>> pair2 in pair1.Value)
                {
                    foreach(int id in pair2.Value)
                    {
                        returnTriplets.Add(new DeltaFunctionTriplet(pair1.Key, pair2.Key, id));
                    }
                }
            }

            return returnTriplets;
        }

        public SortedList<int, List<int>> GetEpsilonTransitions()
        {
            SortedList<int, List<int>> returnEpsilonTransition = new SortedList<int, List<int>>();

            foreach(KeyValuePair<int, List<int>> pair1 in EpsilonDeltaFunction)
            {
                foreach(int id in pair1.Value)
                {
                    if(returnEpsilonTransition.TryGetValue(pair1.Key, out List<int> value))
                    {
                        value.Add(id);
                    }
                    else
                    {
                        value = new List<int>();
                        value.Add(id);
                        returnEpsilonTransition.Add(pair1.Key, value);
                    }
                }
            }

            return returnEpsilonTransition;
        }
    }
}
