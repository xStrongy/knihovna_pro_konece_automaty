using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class Builder
    {

        private DKA Automata;
        public Builder(DKA automata)
        { this.Automata = automata; }
        
        public void Create()
        {
            Console.WriteLine(Automata.ex.Expression);
        }
    }
}
