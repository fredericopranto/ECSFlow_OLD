/*
 * Created on Nov 26, 2004
 *
 */
namespace br.unicamp.ic.sed.mobilemedia.filesystemmgr.spec.dt
{

    using IImageData = br.unicamp.ic.sed.mobilemedia.main.spec.dt.IImageData;

    /// <summary>
    /// @author trevor
    /// 
    /// This class holds meta data associated with a photo or image. There is a one-to-one
    /// relationship between images and image metadata. (ie. Every photo in MobileMedia will
    /// have a corresonding ImageData object). 
    /// It stores the recordId of the image record in RMS, the recordID of the metadata record
    /// the name of the photo album(s) it belongs to, the text label, associated phone numbers
    /// etc.
    /// 
    /// </summary>
    public class ImageData : IImageData
    {

        private int recordId; //imageData recordId
        private int foreignRecordId; //image recordId
        private string parentAlbumName; //Should we allow single image to be part of multiple albums?
        private string imageLabel;

        /// <summary>
        /// Constructor </summary>
        /// <param name="foreignRecordId"> </param>
        /// <param name="parentAlbumName"> </param>
        /// <param name="imageLabel"> </param>
        public ImageData(int foreignRecordId, string parentAlbumName, string imageLabel) : base()
        {
            this.foreignRecordId = foreignRecordId;
            this.parentAlbumName = parentAlbumName;
            this.imageLabel = imageLabel;
        }

        /* (non-Javadoc)
		 * @see br.unicamp.ic.sed.mobilemedia.filesystemmgr.spec.dt.IImageData#getRecordId()
		 */
        public virtual int RecordId
        {
            get
            {
                return recordId;
            }
            set
            {
                this.recordId = value;
            }
        }


        /* (non-Javadoc)
		 * @see br.unicamp.ic.sed.mobilemedia.filesystemmgr.spec.dt.IImageData#getForeignRecordId()
		 */
        public virtual int ForeignRecordId
        {
            get
            {
                return foreignRecordId;
            }
            set
            {
                this.foreignRecordId = value;
            }
        }


        /* (non-Javadoc)
		 * @see br.unicamp.ic.sed.mobilemedia.filesystemmgr.spec.dt.IImageData#getImageLabel()
		 */
        public virtual string ImageLabel
        {
            get
            {
                return imageLabel;
            }
            set
            {
                this.imageLabel = value;
            }
        }


        /* (non-Javadoc)
		 * @see br.unicamp.ic.sed.mobilemedia.filesystemmgr.spec.dt.IImageData#getParentAlbumName()
		 */
        public virtual string ParentAlbumName
        {
            get
            {
                return parentAlbumName;
            }
            set
            {
                this.parentAlbumName = value;
            }
        }

    }

}