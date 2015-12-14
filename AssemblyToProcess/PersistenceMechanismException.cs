using System;

namespace lancs.mobilemedia.lib.exceptions
{
    public class PersistenceMechanismException : Exception
    {
        public PersistenceMechanismException(string arg0) : base(arg0)
        {
            
        }
        public PersistenceMechanismException()
        {
        }
        private Exception cause;
        public PersistenceMechanismException(Exception arg0)
        {
            cause = arg0;
        }
        public Exception getCause()
        {
            return cause;
        }
    }
}