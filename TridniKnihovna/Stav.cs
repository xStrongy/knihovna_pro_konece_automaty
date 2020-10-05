using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class Stav
    {
        private int oznaceni;

        public Stav() {
            this.oznaceni = 0;

        }

        public Stav(int oznaceni, bool vychozi, bool konecny)
        {
            this.oznaceni = oznaceni;

        }

        public void set_oznaceni(int oznaceni)
        {
            this.oznaceni = oznaceni;
        }

    }
}
