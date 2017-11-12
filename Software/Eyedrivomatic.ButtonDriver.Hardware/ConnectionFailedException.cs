using System;

namespace Eyedrivomatic.ButtonDriver.Hardware
{
    public class ConnectionFailedException : ApplicationException
    {
        public ConnectionFailedException(string message) : base(message)
        { }
    }
}