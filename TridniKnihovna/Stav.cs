using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public enum TypStavu {
        Pocatecni,
        Konecny,
        Normalni
    }
    public class Stav
    {
        public int Oznaceni { get; set; }
        public TypStavu Typ{ get; set; }

        public Stav(int oznaceni, TypStavu typ)
        {
            this.Oznaceni = oznaceni;
            this.Typ = typ;
        }


    }
}
