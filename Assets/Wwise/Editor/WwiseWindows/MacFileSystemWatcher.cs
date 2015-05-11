/*
 *   FileSystemWatcher Class written to be used in place of System.IO.FileSystemWatcher on Mac OS X
 *   Kevin Heeney
 *   10/21/2008
 *   Questions: koheeney@gmail.com
 *   
 *   This FileSystemWatcher was written to replace the System.IO.FileSystemWatcher in Mono for Mac OS X.
 *      the FileSystemWatcher on Mac (at least Tiger (10.4)) does not catch most file changes.  Only creations and deletions.
 *      OSX 10.5 - Leopard - adds a feature called FSEvents.  I have not tested this, but I needed a solution for Tiger and Leopard.
 * 
 *   Differences between this implementation and typical implementation is that this does not rely on the OS raising any events.
 *   This implementation certainly has more overhead and is not recommended for large solutions; however it seems to work, so something is better than nothing in my opinion.
 * 
 * 
 *   Directories do not raise change for 'LastAccess'.  This is because this routine was accessing the folders and therefore, events were constantly being raised
 *   Rename is not supported.  For renamed files, you will get a deleted and a created event.
 *   Internal BufferSize is ignored.  The property is there to not break code.
 *   I am sure there are other quirks that I am missing.
 *   
 * 
 */

#if UNITY_EDITOR_OSX

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OSX.IO.FileSystemWatcher
{
    class FileSystemWatcher : System.ComponentModel.Component
    {


        private class StaticFSData
        {

            //Name
            private string name;
            public string Name
            { get { return name; } }


            //Full Name
            private string fullname;
            public string FullName
            { get { return fullname; } }


            //ParentDirectory
            private string parentDirectory;
            public string ParentDirectory
            { get { return parentDirectory; } }

            //LastAccessTime
            private DateTime lastAccessTime;
            public DateTime LastAccessTime
            { get { return lastAccessTime; } }

            //LastWriteTime
            private DateTime lastWriteTime;
            public DateTime LastWriteTime
            { get { return lastWriteTime; } }

            //CreationTime
            private DateTime creationTime;
            public DateTime CreationTime
            { get { return creationTime; } }

            //Attributes
            private string attributes;
            public string Attributes
            { get { return attributes; } }

            //Size - Length of File in Bytes - if folder this is 0
            private long size;
            public long Size
            { get { return size; } }

            public StaticFSData(FileSystemInfo f)
            {
                this.name = f.Name;
                this.fullname = f.FullName;
                if (f is FileInfo)
                {
                    this.parentDirectory = ((FileInfo)f).DirectoryName;
                }
                else
                {
                    this.parentDirectory = ((DirectoryInfo)f).Parent.FullName;
                }

                //times are utc - it seemed safer

                this.creationTime = f.CreationTimeUtc;
                this.lastAccessTime = f.LastAccessTimeUtc;
                this.lastWriteTime = f.LastWriteTimeUtc;
                this.attributes = f.Attributes.ToString(); //just convert attributes to string, that is enough to see if they change

                if (f is FileInfo)
                {
                    this.size = ((FileInfo)f).Length;

                }

            }


        }
        private class StaticDirectoryData : StaticFSData
        {
            private List<StaticDirectoryData> directories = new List<StaticDirectoryData>();
            public List<StaticDirectoryData> Directories
            {
                get { return directories; }
            }
            private List<StaticFSData> files = new List<StaticFSData>();
            public List<StaticFSData> Files
            {
                get { return files; }
            }

            public StaticDirectoryData(DirectoryInfo dir, bool includeSubDir, string filter)
                : base(dir)
            {
                if (includeSubDir)
                {
                    DirectoryInfo[] dirs = dir.GetDirectories();
                    for (int q = 0; q < dirs.Length; q++)
                    {
                        directories.Add(new StaticDirectoryData(dirs[q], includeSubDir, filter));
                    }
                }


                if (filter == "") filter = "*";
                FileInfo[] tempFiles = dir.GetFiles(filter);
                for (int q = 0; q < tempFiles.Length; q++)
                {
                    files.Add(new StaticFSData(tempFiles[q]));
                }
            }


            public StaticDirectoryData GetSubDirectory(string dirName)
            {
                for (int n = 0; n < directories.Count; n++)
                {
                    if (directories[n].Name == dirName)
                    {
                        return directories[n];
                    }
                }

                return null; //no match found
            }

            public StaticFSData GetSubFile(string fileName)
            {
                for (int n = 0; n < files.Count; n++)
                {
                    if (files[n].Name == fileName)
                    {
                        return files[n];
                    }
                }

                return null; //no match found
            }

        }


        public FileSystemWatcher()
        {
            InitializeComponent();
        }
        public FileSystemWatcher(string path)
        {
            InitializeComponent();
            Path = path;
        }
        public FileSystemWatcher(string path, string filter)
        {
            InitializeComponent();
            Path = path;
            Filter = filter;
        }


        private bool enableRaisingEvents = false;
        public bool EnableRaisingEvents
        {
            get { return enableRaisingEvents; }
            set { enableRaisingEvents = value; if (value == true) set(); }
        }


        private string filter = "*.*";
        public string Filter
        {
            get { return filter; }
            set { filter = value; set(); }
        }


        private bool includeSubdirectories = false;
        public bool IncludeSubdirectories
        {
            get { return includeSubdirectories; }
            set { includeSubdirectories = value; }
        }


        //Not Implemented - not used.  Left in here for compatibility with existing code.
        private int internalBufferSize = 0;
        public int InternalBufferSize
        {
            get { return 0; }
            set { internalBufferSize = value; }
        }



        private NotifyFilters notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        public NotifyFilters NotifyFilter
        {
            get { return notifyFilter; }
            set { notifyFilter = value; set(); }
        }

        private string path = "";
		private System.Timers.Timer tmrSample;
        private System.ComponentModel.IContainer components;

        public string Path
        {
            get { return path; }
            set { path = value; set(); }
        }

        //the interval in milliseconds that this class will check the structure at path and compare it to what had been sampled before.
        private int timerInterval = 1000;
        public int TimerInterval
        {
            get { return timerInterval; }
            set { timerInterval = value; set(); }
        }


        //set is probably called to often, when path, filter, notify filters and timer interval are set.  So likely 4 times in initialization.  Had it in the EnableRaiseevents set to true, but that was not really compliant with Mono/.NET way.
        private void set()
        {
            //sets up the FileSystemWatcher and timer.

            //This Function handles the comparing of the file System.
            if (path == "")
            {
                //throw new Exception("Path Not Set in FileSystemWatcher!");
                return;
            }
            else
            {
                DirectoryInfo pathDir = new DirectoryInfo(Path);
                if (pathDir.Exists == false)
                {
                    // throw new Exception("Directory at Path does not Exist!");
                    return;
                }
                else
                {
                    //then we have a valid path.
                    directoryData = new StaticDirectoryData(pathDir, IncludeSubdirectories, Filter);
                    //now Directory Data is set, so set the timer.  From here it will be good to go.
                    tmrSample.Interval = TimerInterval;
                    tmrSample.Enabled = true;
                }
            }
        }


        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrSample = new System.Timers.Timer();
            // 
            // tmrSample
            // 
            this.tmrSample.Interval = 1000;
			this.tmrSample.Elapsed += new System.Timers.ElapsedEventHandler(this.tmrSample_Tick);

        }

        private StaticDirectoryData directoryData;

        private void tmrSample_Tick(object sender, EventArgs e)
        {
            if (EnableRaisingEvents == false)
            {
                tmrSample.Enabled = false;
                return;
            }

            StaticDirectoryData newDirectoryData = new StaticDirectoryData(new DirectoryInfo(Path), IncludeSubdirectories, Filter);

            //We now have newDirectoryData and directoryData.  This is where we compare for changes.
            //NotifyFilter will be used to decide what to compare
            CompareStaticDirectoryData(newDirectoryData, directoryData);
            directoryData = newDirectoryData;
        }

        private void CompareStaticFSData(StaticFSData newData, StaticFSData oldData)
        {
            int raiseEventCount = 0; //only flag for the event to be raised through the notify loops below so that 8 changed events won't come up.

            if ((NotifyFilter & NotifyFilters.LastWrite) == NotifyFilters.LastWrite)
            {
                if (newData.LastWriteTime != oldData.LastWriteTime)
                {
                    raiseEventCount++;
                }
            }

            if ((NotifyFilter & NotifyFilters.Attributes) == NotifyFilters.Attributes)
            {
                if (newData.Attributes != oldData.Attributes)
                {
                    raiseEventCount++;
                }
            }

            if ((NotifyFilter & NotifyFilters.CreationTime) == NotifyFilters.CreationTime)
            {
                if (newData.CreationTime != oldData.CreationTime)
                {
                    raiseEventCount++;
                }
            }

            if ((NotifyFilter & NotifyFilters.FileName) == NotifyFilters.FileName) //This shouldn't happen because filename changes will flag delete or creation.
            {
                if (newData.Name != oldData.Name)
                {
                    raiseEventCount++;
                }
            }

            if (newData is StaticDirectoryData == false) //Don't check lastAccess for directories.  This app was updating lastAccess.
            {
                if ((NotifyFilter & NotifyFilters.LastAccess) == NotifyFilters.LastAccess)
                {
                    if (newData.LastAccessTime != oldData.LastAccessTime)
                    {
                        raiseEventCount++;
                    }
                }
            }




            if ((NotifyFilter & NotifyFilters.Size) == NotifyFilters.Size)  //Size only applies to files.  Size will be 0 for directories
            {
                if (newData.Size != oldData.Size)
                {
                    raiseEventCount++;
                }
            }


            if (RaiseAllEvents == true)
            {
                for (int n = 0; n < raiseEventCount; n++)
                {
                    //raise an event for each change registered.
                    if (Changed != null) Changed(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, newData.ParentDirectory, newData.Name));

                }
            }
            else
            {
                //then only raise one event per file no matter how many changes registered
                if (raiseEventCount > 0)
                {
                    if (Changed != null) Changed(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, newData.FullName, newData.Name));
                }
            }
        }

        private void CompareStaticDirectoryData(StaticDirectoryData newData, StaticDirectoryData oldData)
        {
            /*
             * This fucntion loops through the new data and for each item, finds the matching filename or foldername in the old data.  
             *      Once the match is found, compare .  Raise Changed Event if needed  Then Delete from old Data List
             *      If the match is not found, then raise Created
             *      Once done, check old list.  If any are left, then raise Deleted for those items.
             *      
             *  All this should be done comparing againast NotifyFilter
             */

            //First Loop through Files in newData.
            for (int n = 0; n < newData.Files.Count; n++)
            {
                //First check if old file exists.
                StaticFSData oldFile = oldData.GetSubFile(newData.Files[n].Name);
                if (oldFile != null)
                {
                    //file was found in the old structure and therefore has not been recently added.

                    //Now check the NotifyFilters to see what might have changed.

                    //Calls the compare FSData Function which compares the actual LastWrite, Attributes, etc and raises and necessary events.
                    CompareStaticFSData(newData.Files[n], oldFile);


                    oldData.Files.Remove(oldFile); //This removes the old file so that at the end, all files left in oldData were not matched and therefore have been deleted.


                }
                else
                {
                    //file was not found in old structure and therefore has been recently added.
                    //Raise onCreated Event;

                    if (Created != null) Created(this, new FileSystemEventArgs(WatcherChangeTypes.Created, newData.FullName, newData.Files[n].Name));


                }

            }//end for loop looping through files.




            //Now we have looped through all the new files
            for (int n = 0; n < oldData.Files.Count; n++)
            {
                //Any files in this loop have been deleted so raise onDeleted
                if (Deleted != null) Deleted(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, oldData.FullName, oldData.Files[n].Name));
            }






            //Now loop through Folders in NewData
            for (int n = 0; n < newData.Directories.Count; n++)
            {
                //Check if old Folder Exists.
                StaticDirectoryData oldFolder = oldData.GetSubDirectory(newData.Directories[n].Name);
                if (oldFolder != null)
                {
                    //then folder existed in old structure and has not been recently added.


                    //Calls the compare FSData Function which compares the actual LastWrite, Attributes, etc and raises and necessary events.
                    CompareStaticFSData(newData.Directories[n], oldFolder);



                    //now call this same function for the newFolder and the oldFolder
                    CompareStaticDirectoryData(newData.Directories[n], oldFolder);


                    oldData.Directories.Remove(oldFolder); //This removes the old file so that at the end, all files left in oldData were not matched and therefore have been deleted.


                }
                else
                {
                    //then folder did not exist in old structure and has been recently added
                    if (Created != null) Created(this, new FileSystemEventArgs(WatcherChangeTypes.Created, newData.FullName, newData.Directories[n].Name));
                }

            }



        }


        private bool raiseAllEvents = true;
        public bool RaiseAllEvents
        {
            get { return raiseAllEvents; }
            set { raiseAllEvents = value; }
        }



        //Events
		public event FileSystemEventHandler Changed;
		public event FileSystemEventHandler Created;
		public event FileSystemEventHandler Deleted;
    }
}

#endif