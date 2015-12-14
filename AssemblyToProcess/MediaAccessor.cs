using System;
using System.Collections;/** 
 * @author trevor
 * This is the main data access class. It handles all the connectivity with the
 * RMS record stores to fetch and save data associated with MobilePhoto TODO:
 * Refactor into stable interface for future updates. We may want to access data
 * from RMS, or eventually direct from the 'file system' on devices that support
 * the FileConnection optional API.
 */
namespace lancs.mobilemedia.core.ui.datamodel
{
    public abstract class MediaAccessor
    {
        protected string album_label;
        protected string info_label;
        protected string default_album_name;
        protected Hashtable mediaInfoTable = new Hashtable();
        protected static Hashtable passwordTable = new Hashtable();
        protected string[] albumNames;
        //private RecordStore mediaRS = null;
        //private RecordStore mediaInfoRS = null;
        //private RecordStore passwordRS = null;
        public MediaAccessor(string album_label, string info_label, string default_album_name)
        {
            this.album_label = album_label;
            this.info_label = info_label;
            this.default_album_name = default_album_name;
        }

        internal string[] getAlbumNames()
        {
            throw new NotImplementedException();
        }

        /** 
 * Load all existing photo albums that are defined in the record store.
 * @throws InvalidImageDataException
 * @throws PersistenceMechanismException
 */
        public void loadAlbums()
        {
            //String[] currentStores = RecordStore.listRecordStores();
            //if (currentStores != null)
            //{
            //    System.out.println("MediaAccessor::loadAlbums: Found: " + currentStores.length + " existing record stores");
            //    String[] temp = new String[currentStores.length];
            //    int count = 0;
            //    for (int i = 0; i < currentStores.length; i++)
            //    {
            //        String curr = currentStores[i];
            //        System.out.println("MediaAccessor::loadAlbums: Current store" + curr + "=" + album_label);
            //        if (curr.startsWith(album_label))
            //        {
            //            curr = curr.substring(4);
            //            temp[i] = curr;
            //            count++;
            //        }
            //    }
            //    albumNames = new String[count];
            //    int count2 = 0;
            //    for (int i = 0; i < temp.length; i++)
            //    {
            //        if (temp[i] != null)
            //        {
            //            albumNames[count2] = temp[i];
            //            count2++;
            //        }
            //    }
            //}
            //else
            //{
            //    System.out.println("MediaAccessor::loadAlbums: 0 record stores exist. Creating default one.");
            //    resetRecordStore();
            //    loadAlbums();
            //}
        }

        internal void addMediaData(string label, string path, string album)
        {
            throw new NotImplementedException();
        }

        internal void deleteAlbum(string albumName)
        {
            throw new NotImplementedException();
        }

        internal void createNewAlbum(string albumName)
        {
            throw new NotImplementedException();
        }

        internal void deleteSingleMediaFromRMS(string mediaName, string storeName)
        {
            throw new NotImplementedException();
        }

        internal MediaData[] loadMediaDataFromRMS(string recordName)
        {
            throw new NotImplementedException();
        }

        internal void resetRecordStore()
        {
            throw new NotImplementedException();
        }

        internal MediaData getMediaInfo(string imageName)
        {
            throw new NotImplementedException();
        }

        internal bool updateMediaInfo(MediaData oldData, MediaData newData)
        {
            throw new NotImplementedException();
        }

        internal byte[] loadMediaBytesFromRMS(string recordName, int recordId)
        {
            throw new NotImplementedException();
        }

        internal void addImageData(string photoname, byte[] imgdata, string albumname)
        {
            throw new NotImplementedException();
        }
    }
}