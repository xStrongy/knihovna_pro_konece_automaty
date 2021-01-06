using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Transactions;

namespace TridniKnihovna
{
    public class NFA : FA
    {
        List<State> currentStates = new List<State>();
        public NFA() { }
        public NFA(string nazev, List<char> tokens)
        {
            this.Name = nazev;
            this.tokens = tokens;
            tokens.Add('e');
            this.transitions = new List<Transition>();
            this.states = new List<State>();
        }

        public bool accepts(string input)
        {
            List<State> helpList = new List<State>();
            foreach(State s in states)
            {
                if (s.Type == TypeOfState.Start)
                    currentStates.Add(s);
            }

            if (input == "" && states[0].Type == TypeOfState.End)
                return true;
            if (input == "" && states[0].Type != TypeOfState.End)
                return false;

            for (int i = 0; i < input.Length ;i++)
            {
                if(hasEpsilonTransition(currentStates)==true)
                {
                    goThroughEpsilon(currentStates);
                    i--;
                }
                else
                {
                    foreach(State s in currentStates)
                    {
                        foreach(Transition t in transitions)
                        {
                            if(t.StartState == s.Id && t.Token == input[i])
                            {
                                if (t.EndState == null)
                                    continue;
                                helpList.Add(states.Find(x => x.Id == t.EndState));
                            }
                        }
                    }
                    currentStates.Clear();
                    foreach(State s in helpList)
                    {
                        currentStates.Add(s);
                    }
                }
            }

            foreach(State s in currentStates)
            {
                if (s.Type == TypeOfState.End)
                    return true;
            }
            return false;
        }

        private bool hasEpsilonTransition(List<State> list)
        {
            foreach(State s in list)
            {
                foreach(Transition t in transitions)
                {
                    if (t.Token == 'e' && t.StartState == s.Id)
                        return true;
                }
            }
            return false;
        }
        private void goThroughEpsilon(List<State> list)
        {
            List<State> helpList = new List<State>();
            foreach(State s in list)
            {
                foreach(Transition t in transitions)
                {
                    if (t.StartState == s.Id && t.Token == 'e')
                        helpList.Add(states.Find(x => x.Id == t.EndState));
                }
            }
        }
    }
}
