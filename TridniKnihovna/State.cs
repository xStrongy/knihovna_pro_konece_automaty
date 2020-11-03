using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public enum TypeOfState {
        Start,
        End,
        Normal
    }
    public class State
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TypeOfState Type{ get; set; }
        public State(int id, TypeOfState type)
        {
            this.Id = id;
            this.Type = type;
        }


    }
}
