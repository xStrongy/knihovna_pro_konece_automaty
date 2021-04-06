﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class AutomataBuilder
    {
        private string Alphabet;

        public NondeterministicFiniteAutomaton BuildAutomatonFronDerivationOfRegularExpression(HashSet<string> words)
        {
            List<State> states = new List<State>();
            List<DeltaFunctionTriplet> triplets = new List<DeltaFunctionTriplet>();
            SortedList<int, List<int>> Epsilons = new SortedList<int, List<int>>();
            int stateCounter = 0;

            BuildAlphabetFromDerivationOfRegularExpression(words);

            State start = new State(1, "q1", true, false);

            stateCounter++;

            states.Add(start);

            List<int> startingStatesIds = new List<int>();
            List<int> endingStatesIds = new List<int>();

            int automatonCounter = 0;

            foreach(string s in words)
            {
                automatonCounter++;
                State startState = new State(stateCounter + 1, s + automatonCounter, false, false);
                stateCounter++;
                states.Add(startState);
                startingStatesIds.Add(startState.Id);

                for(int i = 0 ; i < s.Length ; i++ )
                {
                    if(i == s.Length - 1)
                    {
                        State endState = new State(stateCounter + 1, "FinalState" + automatonCounter, false, false);
                        endingStatesIds.Add(endState.Id);
                        stateCounter++;
                        states.Add(endState);
                        triplets.Add(new DeltaFunctionTriplet(endState.Id - 1, s[i], endState.Id));
                        break;
                    }

                    string stateName = s.Substring(i + 1) + automatonCounter;
                    State newState = new State(stateCounter + 1, stateName, false, false);
                    stateCounter++;
                    states.Add(newState);
                    triplets.Add(new DeltaFunctionTriplet(newState.Id - 1, s[i], newState.Id));
                }
            }

            foreach(int id in startingStatesIds)
            {
                if(Epsilons.TryGetValue(1, out List<int> value))
                {
                    if(!value.Contains(id))
                    {
                        value.Add(id);
                    }
                }
                else
                {
                    value = new List<int>();
                    value.Add(id);
                    Epsilons.Add(1, value);
                }
            }

            State end = new State(stateCounter + 1, "q" + (stateCounter + 1), false, true);
            states.Add(end);

            foreach (int id in endingStatesIds)
            {
                if (Epsilons.TryGetValue(id, out List<int> value))
                {
                    if (!value.Contains(end.Id))
                    {
                        value.Add(end.Id);
                    }
                }
                else
                {
                    value = new List<int>();
                    value.Add(end.Id);
                    Epsilons.Add(id, value);
                }
            }

            return new NondeterministicFiniteAutomaton(states, Alphabet, triplets, Epsilons);
        }
        public NondeterministicFiniteAutomaton BuildAutomatonFromRegularExpression(
            string RegularExpression, string Alphabet)
        {
            this.Alphabet = Alphabet;
            RegularExpressionParser rep = new RegularExpressionParser(RegularExpression);
            TreeNode root = rep.toParseTree();

            NondeterministicFiniteAutomaton NFA = toNFAfromParseTree(root);

            return NFA;
        }

        public NondeterministicFiniteAutomaton BuildAutomatonFromRegularGrammar(List<string> RegularGrammar)
        {
            List<char> Nonterminals = new List<char>();
            List<State> states = new List<State>();
            List<DeltaFunctionTriplet> triplets = new List<DeltaFunctionTriplet>();
            SortedList<int, List<int>> Epsilons = new SortedList<int, List<int>>();
            int stateCounter = 0;

            foreach (string line in RegularGrammar)
            {
                string[] splitted = line.Split('-');
                Nonterminals.Add(splitted[0].ToCharArray()[0]);
            }

            BuildAlphabetFromRegularGrammar(RegularGrammar);

            foreach (char c in Nonterminals)
            {
                stateCounter++;
                states.Add(new State(stateCounter, c.ToString(), false, false));
            }

            states.Add(new State(stateCounter + 1, "finalState", false, true));

            states.Find(x => x.Id == 1).IsInitial = true;

            foreach (string line in RegularGrammar)
            {
                string expression = line.Replace(" ", "");
                string LeadNonterminal = expression[0].ToString();
                string[] splittedFromPipes = expression.Substring(2).Split('|');

                foreach (string part in splittedFromPipes)
                {
                    if (part.Length == 1 && !part.Equals("ε"))
                    {
                        triplets.Add(new DeltaFunctionTriplet(states.Find(x => x.Label.Equals(LeadNonterminal)).Id,
                            part.ToCharArray()[0], states[states.Count - 1].Id));
                    }

                    if (part.Length == 2)
                    {
                        triplets.Add(new DeltaFunctionTriplet(states.Find(x => x.Label.Equals(LeadNonterminal)).Id,
                            part.ToCharArray()[0], states.Find(x => x.Label == part[1].ToString()).Id));
                    }

                    if (part.Equals("ε"))
                    {
                        if (Epsilons.TryGetValue(states.Find(x => x.Label.Equals(LeadNonterminal)).Id, out List<int> value))
                        {
                            if (!value.Contains(states[states.Count - 1].Id))
                            {
                                value.Add(states[states.Count - 1].Id);
                            }
                        }
                        else
                        {
                            value = new List<int>();
                            value.Add(states[states.Count - 1].Id);
                            Epsilons.Add(states.Find(x => x.Label.Equals(LeadNonterminal)).Id, value);
                        }
                    }
                }
            }

            return new NondeterministicFiniteAutomaton(states, Alphabet, triplets, Epsilons);
        }

        private void BuildAlphabetFromDerivationOfRegularExpression(HashSet<string> words)
        {
            this.Alphabet = "";

            foreach (string s in words)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (!Alphabet.Contains(s[i]))
                    {
                        this.Alphabet += s[i];
                    }
                }
            }
        }

        private void BuildAlphabetFromRegularGrammar(List<string> regularGrammar)
        {
            this.Alphabet = "";

            foreach (string line in regularGrammar)
            {
                string expression = line.Replace(" ", "");
                string[] splitted = expression.Split('-');
                string[] splittedFromPipes = splitted[1].Split('|');

                foreach (string partOfLine in splittedFromPipes)
                {
                    if (!this.Alphabet.Contains(partOfLine[0]) && !partOfLine[0].Equals('ε'))
                    {
                        this.Alphabet += partOfLine[0];
                    }
                }
            }
        }

        private NondeterministicFiniteAutomaton toNFAfromParseTree(TreeNode root)
        {
            if (root.Label.Equals("Expression"))
            {
                NondeterministicFiniteAutomaton term = toNFAfromParseTree(root.Children[0]);
                if (root.Children.Length == 3)
                {
                    return Union(term, toNFAfromParseTree(root.Children[2]));
                }

                return term;
            }

            if (root.Label.Equals("Term"))
            {
                NondeterministicFiniteAutomaton factor = toNFAfromParseTree(root.Children[0]);
                if (root.Children.Length == 2)
                {
                    return Concat(factor, toNFAfromParseTree(root.Children[1]));
                }

                return factor;
            }

            if (root.Label.Equals("Factor"))
            {
                NondeterministicFiniteAutomaton atom = toNFAfromParseTree(root.Children[0]);
                if (root.Children.Length == 2)
                {
                    string meta = root.Children[1].Label;
                    if (meta.Equals("*"))
                    {
                        return Closure(atom);
                    }

                    if (meta.Equals("+"))
                    {
                        return OneOrMore(atom);
                    }

                    if (meta.Equals("?"))
                    {
                        return ZeroOrOne(atom);
                    }
                }

                return atom;
            }

            if (root.Label.Equals("Atom"))
            {
                if (root.Children.Length == 3)
                {
                    return toNFAfromParseTree(root.Children[1]);
                }

                return toNFAfromParseTree(root.Children[0]);
            }

            if (root.Label.Equals("Char"))
            {
                if (root.Children.Length == 2)
                {
                    return FromSymbol(root.Children[1].Label);
                }

                return FromSymbol(root.Children[0].Label);
            }

            return null;
        }

        private NondeterministicFiniteAutomaton ZeroOrOne(NondeterministicFiniteAutomaton NFA)
        {
            List<State> states = new List<State>();
            List<DeltaFunctionTriplet> triplets = new List<DeltaFunctionTriplet>();
            SortedList<int, List<int>> Epsilons = new SortedList<int, List<int>>();

            State start = new State(1, "q1", true, false);

            states.Add(start);

            List<State> statesNFA = NFA.GetStates();

            foreach (State s in statesNFA)
            {
                states.Add(new State(s.Id + 1, "q" + (s.Id + 1), false, false));
            }

            List<DeltaFunctionTriplet> tripletsNFA = NFA.GetTriplets();

            foreach (DeltaFunctionTriplet triplet in tripletsNFA)
            {
                triplets.Add(new DeltaFunctionTriplet(triplet.From + 1, triplet.By, triplet.To + 1));
            }

            SortedList<int, List<int>> epsilonsNFA = NFA.GetEpsilonTransitions();

            foreach (KeyValuePair<int, List<int>> pair1 in epsilonsNFA)
            {
                List<int> endStates = new List<int>();

                foreach (int id in pair1.Value)
                {
                    endStates.Add(id + 1);
                }

                Epsilons.Add(pair1.Key + 1, endStates);
            }

            int firstStateOfNFA = states[1].Id;
            int lastStateOfNFA = states[states.Count - 1].Id;

            State end = new State(lastStateOfNFA + 1, "q" + (lastStateOfNFA + 1), false, true);

            states.Add(end);

            Epsilons.Add(1, new List<int> { firstStateOfNFA, end.Id });

            if (Epsilons.TryGetValue(lastStateOfNFA, out List<int> value))
            {
                value.Add(end.Id);
            }
            else
            {
                value = new List<int>();
                value.Add(end.Id);
                Epsilons.Add(lastStateOfNFA, value);
            }

            return new NondeterministicFiniteAutomaton(states, Alphabet, triplets, Epsilons);
        }

        private NondeterministicFiniteAutomaton OneOrMore(NondeterministicFiniteAutomaton NFA)
        {
            List<State> states = new List<State>();
            List<DeltaFunctionTriplet> triplets = new List<DeltaFunctionTriplet>();
            SortedList<int, List<int>> Epsilons = new SortedList<int, List<int>>();

            State start = new State(1, "q1", true, false);

            states.Add(start);

            List<State> statesNFA = NFA.GetStates();

            foreach (State s in statesNFA)
            {
                states.Add(new State(s.Id + 1, "q" + (s.Id + 1), false, false));
            }

            List<DeltaFunctionTriplet> tripletsNFA = NFA.GetTriplets();

            foreach (DeltaFunctionTriplet triplet in tripletsNFA)
            {
                triplets.Add(new DeltaFunctionTriplet(triplet.From + 1, triplet.By, triplet.To + 1));
            }

            SortedList<int, List<int>> epsilonsNFA = NFA.GetEpsilonTransitions();

            foreach (KeyValuePair<int, List<int>> pair1 in epsilonsNFA)
            {
                List<int> endStates = new List<int>();

                foreach (int id in pair1.Value)
                {
                    endStates.Add(id + 1);
                }

                Epsilons.Add(pair1.Key + 1, endStates);
            }

            int firstStateOfNFA = states[1].Id;
            int lastStateOfNFA = states[states.Count - 1].Id;

            State end = new State(lastStateOfNFA + 1, "q" + (lastStateOfNFA + 1), false, true);

            states.Add(end);

            Epsilons.Add(1, new List<int> { firstStateOfNFA });

            if (Epsilons.TryGetValue(lastStateOfNFA, out List<int> value))
            {
                if (!value.Contains(firstStateOfNFA))
                {
                    value.Add(firstStateOfNFA);
                }

                value.Add(end.Id);
            }
            else
            {
                value = new List<int>();
                value.Add(firstStateOfNFA);
                value.Add(end.Id);
                Epsilons.Add(lastStateOfNFA, value);
            }

            return new NondeterministicFiniteAutomaton(states, Alphabet, triplets, Epsilons);
        }

        private NondeterministicFiniteAutomaton Closure(NondeterministicFiniteAutomaton NFA)
        {
            List<State> states = new List<State>();
            List<DeltaFunctionTriplet> triplets = new List<DeltaFunctionTriplet>();
            SortedList<int, List<int>> Epsilons = new SortedList<int, List<int>>();

            State start = new State(1, "q1", true, false);

            states.Add(start);

            List<State> statesNFA = NFA.GetStates();

            foreach (State s in statesNFA)
            {
                states.Add(new State(s.Id + 1, "q" + (s.Id + 1), false, false));
            }

            List<DeltaFunctionTriplet> tripletsNFA = NFA.GetTriplets();

            foreach (DeltaFunctionTriplet triplet in tripletsNFA)
            {
                triplets.Add(new DeltaFunctionTriplet(triplet.From + 1, triplet.By, triplet.To + 1));
            }

            SortedList<int, List<int>> epsilonsNFA = NFA.GetEpsilonTransitions();

            foreach (KeyValuePair<int, List<int>> pair1 in epsilonsNFA)
            {
                List<int> endStates = new List<int>();

                foreach (int id in pair1.Value)
                {
                    endStates.Add(id + 1);
                }

                Epsilons.Add(pair1.Key + 1, endStates);
            }

            int firstStateOfNFA = states[1].Id;
            int lastStateOfNFA = states[states.Count - 1].Id;

            State end = new State(lastStateOfNFA + 1, "q" + (lastStateOfNFA + 1), false, true);

            states.Add(end);

            Epsilons.Add(1, new List<int> { firstStateOfNFA, end.Id });

            if (Epsilons.TryGetValue(lastStateOfNFA, out List<int> value))
            {
                if (!value.Contains(firstStateOfNFA))
                {
                    value.Add(firstStateOfNFA);
                }

                value.Add(end.Id);
            }
            else
            {
                value = new List<int>();
                value.Add(firstStateOfNFA);
                value.Add(end.Id);
                Epsilons.Add(lastStateOfNFA, value);
            }

            return new NondeterministicFiniteAutomaton(states, Alphabet, triplets, Epsilons);
        }

        private NondeterministicFiniteAutomaton Concat(NondeterministicFiniteAutomaton first,
            NondeterministicFiniteAutomaton second)
        {
            List<State> states = new List<State>(first.GetStates());
            List<DeltaFunctionTriplet> triplets = new List<DeltaFunctionTriplet>(first.GetTriplets());
            SortedList<int, List<int>> Epsilons = new SortedList<int, List<int>>(first.GetEpsilonTransitions());

            int lastStateOfFirst = states[states.Count - 1].Id;

            int firstStateOfSecond = lastStateOfFirst + 1;

            List<State> statesSecond = second.GetStates();

            int numberShift = states.Count;

            foreach (State s in statesSecond)
            {
                states.Add(new State(s.Id + numberShift, "q" + (s.Id + numberShift), false, false));
            }

            List<DeltaFunctionTriplet> tripletsSecond = second.GetTriplets();

            foreach (DeltaFunctionTriplet triplet in tripletsSecond)
            {
                triplets.Add(new DeltaFunctionTriplet(triplet.From + numberShift, triplet.By, triplet.To + numberShift));
            }

            SortedList<int, List<int>> epsilonsSecond = second.GetEpsilonTransitions();

            foreach (KeyValuePair<int, List<int>> pair1 in epsilonsSecond)
            {
                List<int> endStates = new List<int>();

                foreach (int id in pair1.Value)
                {
                    endStates.Add(id + numberShift);
                }

                Epsilons.Add(pair1.Key + numberShift, endStates);
            }

            if (Epsilons.TryGetValue(lastStateOfFirst, out List<int> value))
            {
                if (!value.Contains(firstStateOfSecond))
                {
                    value.Add(firstStateOfSecond);
                }
            }
            else
            {
                value = new List<int>();
                value.Add(firstStateOfSecond);
                Epsilons.Add(lastStateOfFirst, value);
            }

            return new NondeterministicFiniteAutomaton(states, Alphabet, triplets, Epsilons);
        }

        private NondeterministicFiniteAutomaton FromSymbol(string by)
        {
            List<State> states = new List<State>();
            List<DeltaFunctionTriplet> triplets = new List<DeltaFunctionTriplet>();

            State start = new State(1, "q1", true, false);
            State end = new State(2, "q2", false, true);

            states.Add(start);
            states.Add(end);
            char[] symbol = by.ToCharArray();

            DeltaFunctionTriplet dft = new DeltaFunctionTriplet(1, symbol[0], 2);

            triplets.Add(dft);

            return new NondeterministicFiniteAutomaton(states, Alphabet, triplets, new SortedList<int, List<int>>());
        }

        private NondeterministicFiniteAutomaton Union(NondeterministicFiniteAutomaton first,
            NondeterministicFiniteAutomaton second)
        {
            List<State> states = new List<State>();
            List<DeltaFunctionTriplet> triplets = new List<DeltaFunctionTriplet>();
            SortedList<int, List<int>> Epsilons = new SortedList<int, List<int>>();

            State start = new State(1, "q1", true, false);

            states.Add(start);

            List<State> statesFirst = first.GetStates();

            int numberShift = states.Count;

            foreach (State s in statesFirst)
            {
                states.Add(new State(s.Id + numberShift, "q" + (s.Id + numberShift), false, false));
            }

            int firstStateOfFirst = states[1].Id;

            List<DeltaFunctionTriplet> tripletsFirst = first.GetTriplets();

            foreach (DeltaFunctionTriplet triplet in tripletsFirst)
            {
                triplets.Add(new DeltaFunctionTriplet(triplet.From + numberShift, triplet.By, triplet.To + numberShift));
            }

            SortedList<int, List<int>> epsilonsFirst = first.GetEpsilonTransitions();

            foreach (KeyValuePair<int, List<int>> pair1 in epsilonsFirst)
            {
                List<int> endStates = new List<int>();

                foreach (int id in pair1.Value)
                {
                    endStates.Add(id + numberShift);
                }

                Epsilons.Add(pair1.Key + numberShift, endStates);
            }


            int lastStateOfFirst = states[states.Count - 1].Id;
            int firstStateOfSecond = lastStateOfFirst + 1;

            List<State> statesSecond = second.GetStates();
            numberShift = states.Count;

            foreach (State s in statesSecond)
            {
                states.Add(new State(s.Id + numberShift, "q" + (s.Id + numberShift), false, false));
            }

            List<DeltaFunctionTriplet> tripletsSecond = second.GetTriplets();

            foreach (DeltaFunctionTriplet triplet in tripletsSecond)
            {
                triplets.Add(new DeltaFunctionTriplet(triplet.From + numberShift, triplet.By, triplet.To + numberShift));
            }

            SortedList<int, List<int>> epsilonsSecond = second.GetEpsilonTransitions();

            foreach (KeyValuePair<int, List<int>> pair1 in epsilonsSecond)
            {
                List<int> endStates = new List<int>();

                foreach (int id in pair1.Value)
                {
                    endStates.Add(id + numberShift);
                }

                Epsilons.Add(pair1.Key + numberShift, endStates);
            }

            int lastStateOfSecond = states[states.Count - 1].Id;

            State end = new State(lastStateOfSecond + 1, "q" + (lastStateOfSecond + 1), false, true);

            states.Add(end);

            Epsilons.Add(1, new List<int> { firstStateOfFirst, firstStateOfSecond });

            if (Epsilons.TryGetValue(lastStateOfFirst, out List<int> value))
            {
                if (!value.Contains(end.Id))
                {
                    value.Add(end.Id);
                }
            }
            else
            {
                value = new List<int>();
                value.Add(end.Id);
                Epsilons.Add(lastStateOfFirst, value);
            }

            if (Epsilons.TryGetValue(lastStateOfSecond, out List<int> value1))
            {
                if (!value1.Contains(end.Id))
                {
                    value1.Add(end.Id);
                }
            }
            else
            {
                value1 = new List<int>();
                value1.Add(end.Id);
                Epsilons.Add(lastStateOfSecond, value1);
            }

            return new NondeterministicFiniteAutomaton(states, Alphabet, triplets, Epsilons);
        }
    }
}
