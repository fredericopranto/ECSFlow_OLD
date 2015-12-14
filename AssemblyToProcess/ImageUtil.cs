using br.unicamp.ic.sed.mobilemedia.filesystemmgr.spec.dt;
using br.unicamp.ic.sed.mobilemedia.main.spec.dt;
using System;

namespace br.unicamp.ic.sed.mobilemedia.filesystemmgr.impl
{

    
    /// <summary>
    /// @author trevor This is a utility class. It performs conversions between Image
    ///         objects and byte arrays, and Image metadata objects and byte arrays.
    ///         Byte arrays are the main format for storing data in RMS, and for
    ///         sending data over the wire.
    /// </summary>
    public class ImageUtil
    {

        // Delimiter used in record store data to separate fields in a string.
        protected internal const string DELIMITER = "*";

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageUtil() : base()
        {
        }
        
        /// 
        /// <summary>
        /// Convert the byte array from a retrieved RecordStore record into the
        /// ImageInfo ((renamed ImageData) object Order of the string will look like
        /// this: <recordId>*<foreignRecordId>*<albumName>*<imageLabel> Depending
        /// on the optional features, additional fields may be: <phoneNum>
        /// </summary>
        /// <exception cref="InvalidArrayFormatException"> </exception>
        //[cosmos][MD Sce. 2]add a new parameter to method to use in after aspect.
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public br.unicamp.ic.sed.mobilemedia.main.spec.dt.IImageData getImageInfoFromBytes(byte[] bytes) throws br.unicamp.ic.sed.mobilemedia.filesystemmgr.spec.excep.InvalidArrayFormatException
        public virtual IImageData getImageInfoFromBytes(sbyte[] bytes)
        {

            string iiString = StringHelperClass.NewString(bytes);



            // Track our position in the String using delimiters
            // Ie. Get chars from beginning of String to first Delim
            int startIndex = 0;
            endIndex = iiString.IndexOf(DELIMITER, StringComparison.Ordinal);

            // Get recordID int value as String - everything before first
            // delimeter
            string intString = iiString.Substring(startIndex, endIndex - startIndex);


            // Get 'foreign' record ID corresponding to the image table
            startIndex = endIndex + 1;
            endIndex = iiString.IndexOf(DELIMITER, startIndex, StringComparison.Ordinal);
            string fidString = iiString.Substring(startIndex, endIndex - startIndex);


            // Get Album name (recordstore) - next delimeter
            startIndex = endIndex + 1;
            endIndex = iiString.IndexOf(DELIMITER, startIndex, StringComparison.Ordinal);
            string albumLabel = iiString.Substring(startIndex, endIndex - startIndex);

            startIndex = endIndex + 1;
            endIndex = iiString.IndexOf(DELIMITER, startIndex, StringComparison.Ordinal);

            if (endIndex == -1)
            {
                endIndex = iiString.Length;
            }

            string imageLabel = "";
            imageLabel = iiString.Substring(startIndex, endIndex - startIndex);
            //System.out.println("[rid]="+intString+"[fid]="+fidString+"[album]="+albumLabel+"[imageLabel]="+imageLabel);

            int? x = Convert.ToInt32(fidString);
            //ImageData ii = new ImageData(x.intValue(), albumLabel, imageLabel);



            IImageData ii = createImageData(x.Value, albumLabel, imageLabel, bytes, endIndex);

            Console.WriteLine("[ImageUtil.getImageInfoFromBytes(..)] intString=" + intString);
            x = Convert.ToInt32(intString);
            ii.RecordId = x.Value;

            Console.WriteLine("[ImageUtil.getImageInfoFromBytes(..)] before return");

            return ii;
        }

        /// <summary>
        ///***
        /// Method add just to expose some informations to aspects.
        /// @author Marcelo
        /// Scenario 2 - Sorting by View
        /// 
        /// Tags:[cosmos][add]
        /// 
        /// </summary>
        public virtual IImageData createImageData(int foreignRecordId, string parentAlbumName, string imageLabel, sbyte[] bytes, int endIndex)
        {
            return new ImageData(foreignRecordId, parentAlbumName, imageLabel);
        }

        /// 
        /// <summary>
        /// Convert the ImageInfo (renamed ImageData) object into bytes so we can
        /// store it in RMS Order of the string will look like this: <recordId>*<foreignRecordId>*<albumName>*<imageLabel>
        /// Depending on the optional features, additional fields may be: <phoneNum> </summary>
        /// <exception cref="InvalidImageDataException">  </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public byte[] getBytesFromImageInfo(br.unicamp.ic.sed.mobilemedia.main.spec.dt.IImageData ii) throws br.unicamp.ic.sed.mobilemedia.filesystemmgr.spec.excep.InvalidImageDataException
        public virtual sbyte[] getBytesFromImageInfo(IImageData ii)
        {

            // Take each String and get the bytes from it, separating fields with a
            // delimiter
            string byteString = "";

            // Convert the record ID for this record
            int i = ii.RecordId;
            int? j = new int?(i);
            byteString = byteString + j.ToString();
            byteString = byteString + DELIMITER;

            // Convert the 'Foreign' Record ID field for the corresponding Image
            // record store
            int i2 = ii.ForeignRecordId;
            int? j2 = new int?(i2);
            byteString = byteString + j2.ToString();
            byteString = byteString + DELIMITER;

            // Convert the album name field
            byteString = byteString + ii.ParentAlbumName;
            byteString = byteString + DELIMITER;

            // Convert the label (name) field
            byteString = byteString + ii.ImageLabel;




            // Convert the phone number field
            return GetBytes(byteString);

        }

        static sbyte[] GetBytes(string str)
        {
            sbyte[] bytes = new sbyte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private int endIndex = 0;

        protected internal virtual int EndIndex
        {
            set
            {
                this.endIndex = value;
            }
            get
            {
                return endIndex;
            }
        }

    }
}