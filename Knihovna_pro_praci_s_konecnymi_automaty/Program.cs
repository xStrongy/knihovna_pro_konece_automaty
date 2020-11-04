using System;
using System.Collections.Generic;
using TridniKnihovna;

namespace Knihovna_pro_praci_s_konecnymi_automaty
{
    class Program
    {
        static void Main(string[] args)
        {
            String input = "abbbbbbbbbbbbbbbbbbbbbbbbbbbb";
            List<char> tokens = new List<char>();
            tokens.Add('a');
            tokens.Add('b');
            //RegEx ex = new RegEx();
            //ex.Expression = "((a+b).b)*";
            DFA Automata = new DFA("Q1", tokens);
            //Automata.useRegEx(ex);
            if (Automata.isValidInput(input) == true)
            {
                Automata.createState(1, TypeOfState.Start);
                Automata.addTransition(1, 'a', 2);
                Automata.addTransition(1, 'b', 1);
                Automata.addTransition(2, 'a', 4);
                Automata.addTransition(2, 'b', 5);
                Automata.addTransition(3, 'a', 1);
                Automata.addTransition(3, 'b', 4);
                Automata.addTransition(4, 'a', 1);
                Automata.addTransition(4, 'b', 3);
                Automata.addTransition(5, 'a', 4);
                Automata.addTransition(5, 'b', 5);
                Automata.setTypeOfState(1, TypeOfState.End);
                Automata.setTypeOfState(4, TypeOfState.End);
                Automata.setTypeOfState(5, TypeOfState.End);
                Automata.countOfStates();

                if (Automata.accepts(input) == true)
                    Console.WriteLine("Zadany parametr automat prijima!");
                else
                    Console.WriteLine("Zadany parametr automat neprijima");
            }
            else
            {
                Console.WriteLine("Zadany parametr je neplatny");
            }
        }
    }
}
