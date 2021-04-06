using System;
using System.Collections.Generic;
using System.Xml;
using TridniKnihovna;

namespace Knihovna_pro_praci_s_konecnymi_automaty
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "aa";
            string Alphabet = "a";
            string RegEx = "(a|b)?";
            List<string> RegularGrammar = new List<string>();
            HashSet<string> words = new HashSet<string>();
            words.Add("abc");
            words.Add("abb");
            words.Add("abc");
            words.Add("c");

            RegularGrammar.Add("S - aA");
            RegularGrammar.Add("A - ε | bS | bA | aB");
            RegularGrammar.Add("B - b | aS");

            AutomataBuilder builder = new AutomataBuilder();
            NondeterministicFiniteAutomaton NFA1 = builder.BuildAutomatonFromRegularExpression(RegEx, Alphabet);
            NondeterministicFiniteAutomaton NFA3 = builder.BuildAutomatonFromRegularGrammar(RegularGrammar);
            NondeterministicFiniteAutomaton NFA4 = builder.BuildAutomatonFronDerivationOfRegularExpression(words);
            NFA4.DeleteEpsilonTransitions();
            DeterministicFiniteAutomaton DFA4 = NFA4.ConvertToDeterministicFiniteAutomaton();
            string dotcodeNfa4 = DFA4.GetDotSourceCode();
            string dotcode = NFA3.GetDotSourceCode();
            DeterministicFiniteAutomaton DFA1 = NFA1.ConvertToDeterministicFiniteAutomaton();
            //dotcode = DFA1.GetDotSourceCode();
            List<State> states = new List<State>();
            /*states.Add(new State(1, "q0", true, false));
            states.Add(new State(2, "q1", false, false));
            states.Add(new State(3, "q2", false, false));
            states.Add(new State(4, "q3", false, false));
            states.Add(new State(5, "q4", false, false));
            states.Add(new State(6, "q5", false, false));
            states.Add(new State(7, "q6", false, false));
            states.Add(new State(8, "q7", false, true));
            states.Add(new State(9, "q8", false, true));
            states.Add(new State(10, "q9", false, false));
            states.Add(new State(11, "q10", false, false));
            states.Add(new State(12, "q11", false, true));
            states.Add(new State(13, "q12", false, false));*/
            states.Add(new State(1, "q0", false, true));
            states.Add(new State(2, "q1", false, false));
            states.Add(new State(3, "q2", false, false));
            states.Add(new State(4, "q3", true, true));
            states.Add(new State(5, "q4", false, false));



            List<DeltaFunctionTriplet> dft = new List<DeltaFunctionTriplet>();
            /*dft.Add(new DeltaFunctionTriplet(1, 'a', 3));
            dft.Add(new DeltaFunctionTriplet(3, 'a', 2));
            dft.Add(new DeltaFunctionTriplet(2, 'a', 4));
            dft.Add(new DeltaFunctionTriplet(2, 'a', 5));
            dft.Add(new DeltaFunctionTriplet(2, 'a', 6));
            dft.Add(new DeltaFunctionTriplet(5, 'a', 7));
            dft.Add(new DeltaFunctionTriplet(6, 'a', 8));
            dft.Add(new DeltaFunctionTriplet(6, 'a', 9));
            dft.Add(new DeltaFunctionTriplet(13, 'a', 11));*/
            dft.Add(new DeltaFunctionTriplet(4, 'b', 1));
            dft.Add(new DeltaFunctionTriplet(1, 'a', 2));
            dft.Add(new DeltaFunctionTriplet(2, 'b', 5));
            dft.Add(new DeltaFunctionTriplet(5, 'a', 4));
            dft.Add(new DeltaFunctionTriplet(5, 'b', 4));



            SortedList<int, List<int>> EpsilonTransition = new SortedList<int, List<int>>();
            /*EpsilonTransition.Add(1, new List<int> { 2 });
            EpsilonTransition.Add(4, new List<int> { 7 });
            EpsilonTransition.Add(7, new List<int> { 12 });
            EpsilonTransition.Add(8, new List<int> { 10, 12 });
            EpsilonTransition.Add(9, new List<int> { 11 });
            EpsilonTransition.Add(10, new List<int> { 12 });*/
            EpsilonTransition.Add(4, new List<int> { 3 });
            EpsilonTransition.Add(3, new List<int> { 2 });

            DeterministicFiniteAutomaton DFA = new DeterministicFiniteAutomaton(states, Alphabet, dft);
            NondeterministicFiniteAutomaton NFA = new NondeterministicFiniteAutomaton(states, Alphabet, dft, EpsilonTransition);
            if(NFA.Accepts(input))
            {
                Console.WriteLine("Prijima");
            }
            else
            {
                Console.WriteLine("Neprijima");
            }

            string dot = NFA.GetDotSourceCode();
            NFA.DeleteEpsilonTransitions();
            DeterministicFiniteAutomaton DFA2 = NFA.ConvertToDeterministicFiniteAutomaton();
            //DFA.DeleteEquivalentStates();
            //DFA.Save2Xml();
            //NFA.Save2Xml();
            //NFA.DeleteUnnecessaryStates();
            //NFA.DeleteUnattainableStates();

            NondeterministicFiniteAutomaton NFA2 = XmlAutomataReader.ReadFromXml("test.xml");

           /* states.Add(new State(1, "q0", true, true));
            states.Add(new State(2, "q1", false, false));
            states.Add(new State(3, "q2", false, false));
            states.Add(new State(4, "q3", false, true));
            states.Add(new State(5, "q4", false, true));
            
            List<DeltaFunctionTriplet> dft = new List<DeltaFunctionTriplet>();
            dft.Add(new DeltaFunctionTriplet(1, 'b', 1));
            dft.Add(new DeltaFunctionTriplet(1, 'a', 2));
            dft.Add(new DeltaFunctionTriplet(2, 'a', 4));
            dft.Add(new DeltaFunctionTriplet(2, 'b', 5));
            dft.Add(new DeltaFunctionTriplet(3, 'a', 1));
            dft.Add(new DeltaFunctionTriplet(3, 'b', 4));
            dft.Add(new DeltaFunctionTriplet(4, 'a', 1));
            dft.Add(new DeltaFunctionTriplet(4, 'b', 3));
            dft.Add(new DeltaFunctionTriplet(5, 'a', 4));
            dft.Add(new DeltaFunctionTriplet(5, 'b', 5));

            DeterministicFiniteAutomaton DFA = new DeterministicFiniteAutomaton(states, Alphabet, dft);
            if(DFA.Accepts(input))
            {
                Console.WriteLine("prijima");
            }
            else
            {
                Console.WriteLine("neprijima");
            }*/

        }   
    }
}
