using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class DFA
    {
        public string Name { get; set; }
        private List<State> states;                       // list stavů
        private List<Transition> transitions;             // list prechodu
        private List<char> tokens;                        // list znaku, ktere bude automat prijimat (napr.: a,b ... atd)
        public RegEx ex { get; private set; }
        public int AktualniStav { get; private set; }
        public DFA(string nazev, List<char> tokens)
        {
            this.Name = nazev;
            this.tokens = tokens;
            this.transitions = new List<Transition>();
            this.states = new List<State>();
            this.AktualniStav = 1;
        }

        public void useRegEx(RegEx ex)
        {
            this.ex = ex;
        }

        public void createState(int id, TypeOfState state)
        {
            State a = new State(id, state);
            if (states.Count == 0)
                a.Type = TypeOfState.Start;
            states.Add(a);
        }

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

        public void setTypeOfState(int id, TypeOfState type)
        {
            foreach (State s in states)
            {
                if (s.Id == id)
                    s.Type = type;
            }
        }

        public bool accepts(string input)
        {
            if (input == "" && states[0].Type == TypeOfState.End)
              return true;
            if (input == "" && states[0].Type != TypeOfState.End)
              return false;

            for (int i = 0; i < input.Length; i++)
            {
                foreach( Transition t in transitions)
                {
                    if (t.StartState == AktualniStav && input[i] == t.Token)
                    {
                        this.AktualniStav = t.EndState;
                        break;
                    }
                }
            }

            foreach (State s in states)
            {
                if (s.Id == AktualniStav && s.Type == TypeOfState.End)
                    return true;
            }
          return false;
        }

        public void countOfStates()
        {
            Console.WriteLine("Je vytvoreno " + states.Count + " stavů!");
        }

        public bool isValidInput(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!(tokens.Contains(input[i])))
                    return false;
            }
            return true;
        }
    }

}
