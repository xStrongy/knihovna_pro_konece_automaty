using System;
using System.Collections.Generic;
using TridniKnihovna;

namespace Knihovna_pro_praci_s_konecnymi_automaty
{
    class Program
    {
        static void Main(string[] args)
        {
            List<char> tokens = new List<char>();
            tokens.Add('a');
            tokens.Add('b');
            RegEx ex = new RegEx();
            ex.Expression = "((a+b).b)*";
            DKA Automata = new DKA("Q1", tokens);
            Automata.useRegEx(ex);
            Builder builder = new Builder(Automata);
            builder.Create();
        }
    }
}
