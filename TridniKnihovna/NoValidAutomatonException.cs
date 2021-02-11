using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    public class NoValidAutomatonException : Exception
    {
        public NoValidAutomatonException()
        {

        }

        public NoValidAutomatonException(string message)
            : base(message)
        {

        }

        public NoValidAutomatonException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
