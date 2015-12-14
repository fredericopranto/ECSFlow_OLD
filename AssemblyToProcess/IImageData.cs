namespace br.unicamp.ic.sed.mobilemedia.main.spec.dt
{

    public interface IImageData
    {

        /// <returns> Returns the recordId. </returns>
        int RecordId { get; set; }


        /// <returns> Returns the foreignRecordId. </returns>
        int ForeignRecordId { get; set; }


        /// <returns> Returns the imageLabel. </returns>
        string ImageLabel { get; set; }


        /// <returns> Returns the parentAlbumName. </returns>
        string ParentAlbumName { get; set; }

    }
}