using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class Transition
    {
        public uint StartState { get; set; }
        public char Token { get; set; }
        public uint? EndState{get;set;}

        public Transition(uint startState, char token, uint endState)
        {
            this.StartState = startState;
            this.Token = token;
            this.EndState = endState;
        }
    }
}
