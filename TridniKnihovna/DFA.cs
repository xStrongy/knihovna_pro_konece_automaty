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
        private int? currentState {get; set;}
        public DFA(string nazev, List<char> tokens)
        {
            this.Name = nazev;
            this.tokens = tokens;
            this.transitions = new List<Transition>();
            this.states = new List<State>();
            this.currentState = 1;
        }




        //funkce která vrací bool hodnotu podle toho, zda daný automat přijímá input nebo ne
        public bool accepts(string input)
        {
            if (input == "" && states[0].Type == TypeOfState.End)
                return true;
            if (input == "" && states[0].Type != TypeOfState.End)
                return false;

            for (int i = 0; i < input.Length; i++)
            {
                foreach (Transition t in transitions)
                {
                    if (t.StartState == currentState && input[i] == t.Token)
                    {
                        this.currentState = t.EndState;
                        break;
                    }
                }
            }

                foreach (State s in states)
                {
                    if (s.Id == currentState && s.Type == TypeOfState.End)
                        return true;
                }
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

    }
}
