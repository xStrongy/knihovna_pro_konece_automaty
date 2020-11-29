using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class Transition
    {
        public int StartState { get; set; }
        public char Token { get; set; }
        public int? EndState{get;set;}

        public Transition(int startState, char token, int endState)
        {
            this.StartState = startState;
            this.Token = token;
            this.EndState = endState;
        }
    }
}
