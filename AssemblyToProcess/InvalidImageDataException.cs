using System;

namespace lancs.mobilemedia.lib.exceptions
{
    public class InvalidImageDataException : Exception
    {
        private Exception cause;
        public InvalidImageDataException() : base()
        {

        }
        public InvalidImageDataException(string arg0) : base(arg0)
        {
            
        }
        
        public InvalidImageDataException(Exception arg0)
        {
            cause = arg0;
        }
        public Exception getCause()
        {
            return cause;
        }
    }
}