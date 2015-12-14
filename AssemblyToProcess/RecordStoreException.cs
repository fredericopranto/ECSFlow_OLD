using System;

namespace lancs.mobilemedia.lib.exceptions
{
    public class RecordStoreException : Exception
    {
        public RecordStoreException(string arg0) : base(arg0)
        {
            
        }
        public RecordStoreException()
        {
        }
        private Exception cause;
        public RecordStoreException(Exception arg0)
        {
            cause = arg0;
        }
        public Exception getCause()
        {
            return cause;
        }
    }
}