using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class Builder
    {

        private DFA Automata;
        public Builder(DFA automata)
        { this.Automata = automata; }
        
        public void Create()
        {
            Console.WriteLine(Automata.ex.Expression);
        }
    }
}
