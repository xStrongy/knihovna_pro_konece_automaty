using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class DKA
    {
        public string Name { get; set; }
        private List<State> states;                       // list stavů
        private List<Transition> transitions;                 // list prechodu
        private List<char> tokens;                      // list znaku, ktere bude automat prijimat (napr.: a,b ... atd)
        public RegEx ex { get; private set; }
        public int AktualniStav { get; private set; }
        public DKA(string nazev, List<char> tokens)
        {
            this.Name = nazev;
            this.tokens = tokens;
            this.transitions = new List<Transition>();
            this.states = new List<State>();
        }

        public void useRegEx(RegEx ex)
        {
            this.ex = ex;
        }
    }

}
