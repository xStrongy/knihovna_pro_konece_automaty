using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class DFA : FA
    {
       
        public DFA(string nazev, List<char> tokens)
        {
            this.Name = nazev;
            this.tokens = tokens;
            tokens.Add('e');
            this.transitions = new List<Transition>();
            this.states = new List<State>();
            this.AktualniStav = 1;
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

        //funkce která vrací bool hodnotu podle toho, zda daný automat přijímá input nebo ne
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

    }

}
