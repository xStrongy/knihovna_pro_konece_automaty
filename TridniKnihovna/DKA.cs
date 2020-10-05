using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class DKA
    {
        public string Nazev { get; set; }
        private List<Stav> stavy;                       // list stavů
        private List<Prechod> prechody;                 // list prechodu
        private List<char> tokeny;                      // list znaku, ktere bude automat prijimat (napr.: a,b ... atd)
        public int AktualniStav { get; private set; }
        public DKA(string nazev, List<char> tokeny)
        {
            this.Nazev = nazev;
            this.tokeny = tokeny;
            this.prechody = new List<Prechod>();
            this.stavy = new List<Stav>();
        }
    }
}
