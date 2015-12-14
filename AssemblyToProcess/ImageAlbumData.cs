namespace lancs.mobilemedia.core.ui.datamodel
{
    public class ImageAlbumData : AlbumData
    {
       // public ImageAlbumData(){
       //     mediaAccessor = new ImageMediaAccessor(this);
       // }
       // /** 
       //* Get a particular image (by name) from a photo album. The album name corresponds
       //* to a record store.
       //* @throws ImageNotFoundException 
       //* @throws PersistenceMechanismException 
       //*/
       // public Image getImageFromRecordStore(  string recordStore,  string imageName) throws ImageNotFoundException, PersistenceMechanismException
       // {
       //     MediaData imageInfo = null;
       //     imageInfo = mediaAccessor.getMediaInfo(imageName);
       //     int imageId = imageInfo.getForeignRecordId();
       //     string album = imageInfo.getParentAlbumName();
       //     Image imageRec = ((ImageMediaAccessor)mediaAccessor).loadSingleImageFromRMS(album, imageId);
       //     return imageRec;
       // }
    }
}