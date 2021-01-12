using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace TridniKnihovna
{
    public class FA
    {
        public string Name { get; set; }
        [JsonProperty]
        protected List<State> states;                       // list stavů
        [JsonProperty]
        protected List<Transition> transitions;             // list prechodu
        [JsonProperty]
        protected List<char> tokens;                        // list znaku, ktere bude automat prijimat (napr.: a,b ... atd)
        public RegEx ex { get; set; }
        protected const string dataPath = @"C:\Users\Strongy\source\repos\Knihovna_pro_praci_s_konecnymi_automaty\saved\";


        //funkce pro vytvoření stavů
        public void createState(int id, TypeOfState state)
        {
            State a = new State(id, state);
            if (states.Count == 0)
                a.Type = TypeOfState.Start;
            states.Add(a);
        }

        //funkce pro nastavení typu stavu
        public void setTypeOfState(int id, TypeOfState type)
        {
            foreach (State s in states)
            {
                if (s.Id == id)
                    s.Type = type;
            }
        }


        //funkce pro výpis počtu stavů
        public void countOfStates()
        {
            Console.WriteLine("Je vytvoreno " + states.Count + " stavů!");
        }

        //funkce která určuje, zda vstupní input je validní
        public bool isValidInput(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!(tokens.Contains(input[i])))
                    return false;
            }
            return true;
        }

        public void useRegEx(RegEx ex)
        {
            this.ex = ex;
        }

        //funkce pro vytvoření přechodů a stavů
        public void addTransition(int start, char token, int end)
        {
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
            transitions.Add(transition);

        }

    }
}
