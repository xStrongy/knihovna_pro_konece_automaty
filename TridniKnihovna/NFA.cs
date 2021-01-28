using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Transactions;
using Newtonsoft.Json;

namespace TridniKnihovna
{
    public class NFA : FA
    {
        [JsonProperty]
        private IDictionary<uint, SortedList<char, List<uint>>> dictionary = new Dictionary<uint, SortedList<char, List<uint>>>();
        List<uint> currentStateIds = new List<uint>();
        SortedList<char, List<uint>> sortedHelpList = new SortedList<char, List<uint>>();
        List<uint> helpStateIdList = new List<uint>();
        List<uint> helpCurrentStateIdList = new List<uint>();
        public NFA(string nazev, List<char> tokens)
        {
            this.Name = nazev;
            this.tokens = tokens;
            tokens.Add('E');
            this.transitions = new List<Transition>();
            this.states = new List<State>();
        }
        public bool accepts(string input)
        {
            List<uint> helpList = new List<uint>();
            foreach (State s in states)
            {
                if (s.Type == TypeOfState.Start || s.Type == TypeOfState.StartAndEnd)
                {
                    currentStateIds.Add(s.Id);
                }
            }

            for (int i = 0; i < input.Length; i++)
            {
                foreach(uint test in currentStateIds)
                {
                    Console.Write(test + " ");
                }
                Console.WriteLine();
                do
                {
                    helpList = hasEpsylonTransition(currentStateIds);
                    if(helpList.Count != 0)
                    {
                        helpList = goThroughEpsylonTransition(helpList);
                    }
                } while (helpList.Count != 0);
                
                foreach(uint index in currentStateIds)
                {
                    helpCurrentStateIdList.Add(index);
                }
                currentStateIds.Clear();
                foreach (uint index in helpCurrentStateIdList)
                {
                    sortedHelpList = dictionary[index];
                    if(sortedHelpList.ContainsKey(input[i]) == false)
                    {
                        continue;
                    }
                    helpStateIdList = sortedHelpList[input[i]];
                    foreach (uint newState in helpStateIdList)
                    {
                        if(currentStateIds.Contains(newState) == true)
                        {
                            continue;
                        }
                        currentStateIds.Add(newState);
                    }
                }
                helpCurrentStateIdList.Clear();
            }

            foreach(uint i in currentStateIds)
            {
                if (states.Find(x => x.Id == i).Type == TypeOfState.End || states.Find(x => x.Id == i).Type == TypeOfState.StartAndEnd)
                    return true;
            }
            return false;
        }

        private List<uint> goThroughEpsylonTransition(List<uint> helpList)
        {
            List<uint> tempHelpList = new List<uint>();
            foreach(uint i in helpList)
            {
                tempHelpList.Add(i);
            }

            foreach (uint i in helpList)
            {
                tempHelpList.Remove(i);
                currentStateIds.Remove(i);
                sortedHelpList = dictionary[i];
                helpStateIdList = sortedHelpList['E'];
                foreach(uint index in helpStateIdList)
                {
                    currentStateIds.Add(index);
                }
            }
            return tempHelpList;
        }

        private List<uint> hasEpsylonTransition(List<uint> currentStateIds)
        {
            List<uint> epsylonTransitionStateId = new List<uint>();
            SortedList<char, List<uint>> helpSortedList = new SortedList<char, List<uint>>();
            foreach(uint i in currentStateIds)
            {
                helpSortedList = dictionary[i];
                if(helpSortedList.ContainsKey('E'))
                {
                    epsylonTransitionStateId.Add(i);
                }
            }

            return epsylonTransitionStateId;
        }

        public void saveToJson()
        {
            string context = JsonConvert.SerializeObject(this);
            File.WriteAllText(dataPath + "nfa\\" + this.Name + ".json", context);
        }

        public NFA loadFromJson(string name)
        {
            if (!(File.Exists(dataPath + "nfa\\" + name + ".json")))
            {
                Console.WriteLine("Soubor neexistuje");
                return null;
            }

            NFA nfa = JsonConvert.DeserializeObject<NFA>(File.ReadAllText(dataPath + "nfa\\" + name + ".json"));
            return nfa;
        }

        //funkce pro vytvoření přechodů a stavů
        public void addTransition(uint start, char token, uint end)
        {
            SortedList<char, List<uint>> tempList = new SortedList<char, List<uint>>();
            List<uint> tempIdList = new List<uint>();
            bool exists = false;
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Id == end)
                    exists = true;
            }

            if (exists == false)
            {
                State a = new State(end, TypeOfState.Normal);
                states.Add(a);
            }
            exists = false;

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Id == start)
                    exists = true;
            }

            if (exists == false)
            {
                State a = new State(start, TypeOfState.Normal);
            }
            Transition transition = new Transition(start, token, end);
            if (dictionary.ContainsKey(start) == false)
            {
                tempIdList.Add(end);
                tempList.Add(token, tempIdList);
            }
            else
            {
                tempList = dictionary[start];
                dictionary.Remove(start);
                if (tempList.ContainsKey(token) == false)
                {
                    tempIdList = new List<uint>();
                    tempIdList.Add(end);
                    tempList.Add(token, tempIdList);
                }
                else
                {
                    tempIdList = tempList[token];
                    tempList.Remove(token);
                    tempIdList.Add(end);
                    tempList.Add(token, tempIdList);
                }
            }
            dictionary.Add(start, tempList);

            transitions.Add(transition);

        }

    }
}
