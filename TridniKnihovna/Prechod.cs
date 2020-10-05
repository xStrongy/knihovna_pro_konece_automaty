using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class Prechod
    {
        public int StartStav { get; set; }
        public string Token { get; set; }
        public int KonecStav{get;set;}

        public Prechod(int startStav, string token, int konecStav)
        {
            this.StartStav = startStav;
            this.Token = token;
            this.KonecStav = konecStav;
        }
    }
}
