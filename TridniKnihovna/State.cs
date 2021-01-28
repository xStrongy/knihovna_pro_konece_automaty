using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public enum TypeOfState {
        Start,
        End,
        Normal,
        StartAndEnd
    }
    public class State
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public TypeOfState Type{ get; set; }
        public State(uint id, TypeOfState type)
        {
            this.Id = id;
            this.Type = type;
        }


    }
}
