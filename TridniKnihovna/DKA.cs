using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class DKA
    {
        private string nazev;
        private List<Stav> stavy;                       // list stavů
        private List<Prechod> prechody;                 // list prechodu
        private List<char> tokeny;                      // list znaku, ktere bude automat prijimat (napr.: a,b ... atd)
        public List<Stav> KonecneStavy;                 // list konecnych stavu
        private int aktualniStav;
        public DKA(string nazev, List<char> tokeny)
        {
            this.nazev = nazev;
            this.tokeny = tokeny;
            this.prechody = new List<Prechod>();
            this.stavy = new List<Stav>();
            this.KonecneStavy = new List<Stav>();
        }
    }
}
