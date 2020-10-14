using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class Transition
    {
        public int StartState { get; set; }
        public string Token { get; set; }
        public int EndState{get;set;}

        public Transition(int startState, string token, int endState)
        {
            this.StartState = startState;
            this.Token = token;
            this.EndState = endState;
        }
    }
}
