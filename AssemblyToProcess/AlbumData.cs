// Conversion output is limited to 2048 chars
// Share Varycode on Facebook and tweet on Twitter
// to double the limits.


using lancs.mobilemedia.lib.exceptions;
using System;
/** 
* @author tyoung
* This class represents the data model for Photo Albums. A Photo Album object
* is essentially a list of photos or images, stored in a Hashtable. Due to
* constraints of the J2ME RecordStore implementation, the class stores a table
* of the images, indexed by an identifier, and a second table of image metadata
* (ie. labels, album name etc.)
* This uses the ImageAccessor class to retrieve the image data from the
* recordstore (and eventually file system etc.)
*/
namespace lancs.mobilemedia.core.ui.datamodel
{
    public abstract class AlbumData
    {
        protected MediaAccessor mediaAccessor;
        /** 
       * Load any photo albums that are currently defined in the record store
       */
        public string[] getAlbumNames()
        {
            try
            {
                mediaAccessor.loadAlbums();
            }
            catch (InvalidImageDataException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            catch (PersistenceMechanismException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return mediaAccessor.getAlbumNames();
        }
        /** 
  * Get all images for a given Photo Album that exist in the Record Store.
  * @throws UnavailablePhotoAlbumException 
  * @throws InvalidImageDataException 
  * @throws PersistenceMechanismException 
  */
        public MediaData[] getMedias(string recordName)
        {
            MediaData[] result;
            try
            {
                result = mediaAccessor.loadMediaDataFromRMS(recordName);
            }
            catch (PersistenceMechanismException e)
            {
                throw new UnavailablePhotoAlbumException(e);
            }
            catch (InvalidImageDataException e)
            {
                throw new UnavailablePhotoAlbumException(e);
            }
            return result;
        }
        /** 
       * Define a new user photo album. This results in the creation of a new
       * RMS Record store.
       * @throws PersistenceMechanismException 
       * @throws InvalidPhotoAlbumNameException 
       */
        public void createNewAlbum(string albumName)
        {
            mediaAccessor.createNewAlbum(albumName);
        }
        /** 
       * @param albumName
       * @throws PersistenceMechanismException
       */
        public void deleteAlbum(string albumName)
        {
            mediaAccessor.deleteAlbum(albumName);
        }
        /** 
       * @param label
       * @param path
       * @param album
       * @throws InvalidImageDataException
       * @throws PersistenceMechanismException
       */
        public void addNewMediaToAlbum(string label, string path, string album)
        {
            mediaAccessor.addMediaData(label, path, album);
        }
        /** 
        * Delete a photo from the photo album. This permanently deletes the image from the record store
        * @throws ImageNotFoundException 
        * @throws PersistenceMechanismException 
*/
        public void deleteMedia(string mediaName, string storeName)
        {
            mediaAccessor.deleteSingleMediaFromRMS(mediaName, storeName);
        }


        /** 
        * Reset the image data for the application. This is a wrapper to the ImageAccessor.resetImageRecordStore
        * method. It is mainly used for testing purposes, to reset device data to the default album and photos.
        * @throws PersistenceMechanismException 
        * @throws InvalidImageDataException 
*/
        public void resetMediaData()
        {
            try
            {
                mediaAccessor.resetRecordStore();
            }
            catch (InvalidImageDataException e)
            {
                //e.printStackTrace();
            }
        }
        /** 
        * @param imageName
        * @return
        * @throws ImageNotFoundException
*/
        public MediaData getMediaInfo(string imageName)
        {
            return mediaAccessor.getMediaInfo(imageName);
        }
        /** 
        * @param recordName
        * @return
        * @throws PersistenceMechanismException
        * @throws InvalidImageDataException
*/
        public MediaData[] loadMediaDataFromRMS(string recordName)
        {
            return mediaAccessor.loadMediaDataFromRMS(recordName);
        }
        /** 
        * @param oldData
        * @param newData
        * @return
        * @throws InvalidImageDataException
        * @throws PersistenceMechanismException
*/
        public bool updateMediaInfo(MediaData oldData, MediaData newData)
        {
            return mediaAccessor.updateMediaInfo(oldData, newData);
        }
        /** 
        * @param recordName
        * @param recordId
        * @return
        * @throws PersistenceMechanismException
*/
        public byte[] loadMediaBytesFromRMS(string recordName, int recordId)
        {
            return mediaAccessor.loadMediaBytesFromRMS(recordName, recordId);
        }


        /** 
        * @param photoname
        * @param imgdata
        * @param albumname
        * @throws InvalidImageDataException
        * @throws PersistenceMechanismException
*/
        public void addImageData(string photoname, byte[] imgdata, string albumname)
        {
            if (mediaAccessor is ImageMediaAccessor) mediaAccessor.addImageData(photoname, imgdata, albumname);
        }
    }
}