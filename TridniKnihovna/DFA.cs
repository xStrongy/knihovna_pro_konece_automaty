using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Transactions;
using Newtonsoft.Json;

namespace TridniKnihovna
{
    public class DFA : FA
    {
        [JsonProperty]
        private IDictionary<uint, SortedList<char, uint>> dictionary = new Dictionary<uint, SortedList<char, uint>>();
        private uint currentStateId;
        public DFA(string nazev, List<char> tokens)
        {
            this.Name = nazev;
            this.tokens = tokens;
            this.transitions = new List<Transition>();
            this.states = new List<State>();
            this.currentStateId = 0;
        }




        //funkce která vrací bool hodnotu podle toho, zda daný automat přijímá input nebo ne
        public bool accepts(string input)
        {
            foreach(State s in states)
            {
                if(s.Type == TypeOfState.Start || s.Type == TypeOfState.StartAndEnd)
                {
                    currentStateId = s.Id;
                    break;
                }
            }

            if (input.Equals("") && (states.Find(x => x.Id == currentStateId).Type == TypeOfState.End || states.Find(x => x.Id == currentStateId).Type == TypeOfState.StartAndEnd))
            {
                return true;
            }

            if (input.Equals("") && (states.Find(x => x.Id == currentStateId).Type != TypeOfState.End && states.Find(x => x.Id == currentStateId).Type != TypeOfState.StartAndEnd))
            {
                return false; 
            }
                

           for (int i = 0 ; i < input.Length ;i++ )
            {
                SortedList<char, uint> tempList = dictionary[currentStateId];
                currentStateId = tempList[input[i]];
            }

            Console.WriteLine(currentStateId);

            if (states.Find(x => x.Id == currentStateId).Type == TypeOfState.End || states.Find(x => x.Id == currentStateId).Type == TypeOfState.StartAndEnd)
                return true;
            else
                return false;
        }

        public DFA loadFromJson(string name)
        {
            if (!(File.Exists(dataPath + "dfa\\" + name + ".json")))
            {
                Console.WriteLine("Soubor neexistuje");
                return null;
            }

            DFA dfa = JsonConvert.DeserializeObject<DFA>(File.ReadAllText(dataPath + "dfa\\" + name + ".json"));
            return dfa;
        }

        public void saveToJson()
        {
            string context = JsonConvert.SerializeObject(this);
            //Console.WriteLine(context);
            File.WriteAllText(dataPath + "dfa\\" + this.Name + ".json", context);
        }

        public void addTransition(uint start, char token, uint end)
        {
            SortedList<char, uint> tempList = new SortedList<char, uint>();
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
                tempList.Add(token, end);
            }
            else
            {
                tempList = dictionary[start];
                dictionary.Remove(start);
                tempList.Add(token, end);
            }
            dictionary.Add(start, tempList);
                
            transitions.Add(transition);

        }

    }
}
