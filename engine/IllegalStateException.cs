using System;
using System.Collections.Generic;
using System.Text;

namespace engine
{
    public class IllegalStateException : Exception
    {
        public IllegalStateException(string str) : base(str) { }
    }
}
