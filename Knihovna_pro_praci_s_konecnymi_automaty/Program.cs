﻿using System;
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
            string Alphabet = "ab";
            List<State> states = new List<State>();
            states.Add(new State(1, "q0", true, false));
            states.Add(new State(2, "q1", false, false));
            states.Add(new State(3, "q2", false, false));
            states.Add(new State(4, "q3", false, false));
            states.Add(new State(5, "q4", false, false));
            states.Add(new State(6, "q5", false, false));
            states.Add(new State(7, "q6", false, true));
            states.Add(new State(8, "q7", false, true));
            states.Add(new State(9, "q8", false, true));
            states.Add(new State(10, "q9", false, false));
            states.Add(new State(11, "q10", false, false));
            states.Add(new State(12, "q11", false, true));
            states.Add(new State(13, "q12", false, false));

            List<DeltaFunctionTriplet> dft = new List<DeltaFunctionTriplet>();
            dft.Add(new DeltaFunctionTriplet(1, 'a', 3));
            dft.Add(new DeltaFunctionTriplet(3, 'a', 2));
            dft.Add(new DeltaFunctionTriplet(2, 'a', 4));
            dft.Add(new DeltaFunctionTriplet(2, 'a', 5));
            dft.Add(new DeltaFunctionTriplet(2, 'a', 6));
            dft.Add(new DeltaFunctionTriplet(5, 'a', 7));
            dft.Add(new DeltaFunctionTriplet(6, 'a', 8));
            dft.Add(new DeltaFunctionTriplet(6, 'a', 9));
            dft.Add(new DeltaFunctionTriplet(13, 'a', 11));


            SortedList<int, List<int>> EpsilonTransition = new SortedList<int, List<int>>();
            EpsilonTransition.Add(1, new List<int> { 2});
            EpsilonTransition.Add(4, new List<int> { 7});
            EpsilonTransition.Add(7, new List<int> { 12 });
            EpsilonTransition.Add(8, new List<int> { 10 });
            EpsilonTransition.Add(9, new List<int> { 11 });
            EpsilonTransition.Add(10, new List<int> { 12 });

            NondeterministicFiniteAutomaton NFA = new NondeterministicFiniteAutomaton(states, Alphabet, dft, EpsilonTransition);
            if(NFA.Accepts(input))
            {
                Console.WriteLine("Prijima");
            }
            else
            {
                Console.WriteLine("Neprijima");
            }
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter writer = XmlWriter.Create("test.xml", settings);
            NFA.Save2Xml(writer);
            NFA.DeleteUnattainableStates();

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
