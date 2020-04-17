using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace VietOCR.NET.Utilities
{
    /// <summary>
    /// Monitors a folder for new image files.
    /// </summary>
    public class Watcher
    {
        private Queue<String> queue;
        private Regex filters = new Regex(@".*\.(tif|tiff|jpg|jpeg|gif|png|bmp|pdf)$", RegexOptions.IgnoreCase);

        private FileSystemWatcher watcher;

        public string Path
        {
            get { return watcher.Path; }
            set { watcher.Path = value; }
        }

        public bool Enabled
        {
            get { return watcher.EnableRaisingEvents; }
            set { watcher.EnableRaisingEvents = value; }
        }

        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Watcher(Queue<String> q, string watchFolder)
        {
            queue = q;

            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = watchFolder;
            /* Watch for changes in LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Only watch tif files.
            //watcher.Filter = "*.tif"; // commented out since multiple filters not possible with Filter property

            // Add event handlers.
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(OnChanged);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                if (filters.IsMatch(e.Name))
                {
                    // Wait if file is still being written
                    FileInfo fileInfo = new FileInfo(e.FullPath);
                    while (IsFileLocked(fileInfo))
                    {
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine("New file: " + e.FullPath);
                    queue.Enqueue(e.FullPath);
                }
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        /// <summary>
        /// Check if file is locked as in being used or written by another process.
        /// https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use/
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    //stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}