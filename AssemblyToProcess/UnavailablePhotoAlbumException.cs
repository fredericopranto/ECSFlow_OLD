using System;

namespace lancs.mobilemedia.lib.exceptions
{
    public class UnavailablePhotoAlbumException : Exception
    {
        private Exception cause;
        public UnavailablePhotoAlbumException()
        {
        }
        public UnavailablePhotoAlbumException(string arg0) : base(arg0)
        {

        }
        public UnavailablePhotoAlbumException(Exception arg0)
        {
            cause = arg0;
        }
        public Exception getCause()
        {
            return cause;
        }
    }
}