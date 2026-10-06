using System;
using System.Collections.Generic;
using System.Text;

namespace Transfer.Domain.Common
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
        public DomainException(string message,Exception exception) : base(message,exception) { }
    }
}
