using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Multiverse
{
    /// <summary>
    /// Constantly waits for new 
    /// </summary>
    public class Processor
    {
        public string MonitorDir;
        public string BackupDir;
        private FileSystemWatcher Watcher;

        public Processor(string monitor, string backup)
        {
            MonitorDir = monitor;
            BackupDir = backup;
            Watcher = new FileSystemWatcher(MonitorDir);
            Watcher.NotifyFilter = NotifyFilters.DirectoryName;
            Watcher.Created += new FileSystemEventHandler(eventRaised);
            Watcher.EnableRaisingEvents = true;
        }

        private void eventRaised(object sender, System.IO.FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    Console.WriteLine(string.Format("File {0} has been modified\r\n", e.FullPath));

                    break;
                case WatcherChangeTypes.Created:
                    Console.WriteLine(string.Format("File {0} has been created\r\n", e.FullPath));
                    if (!ParentExist(e.Name))
                    {
                        CreateBackupFolder(e.Name);
                    }
                    CreateBackupItem(e.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    Console.WriteLine(string.Format("File {0} has been deleted\r\n", e.FullPath));
                    break;
                case WatcherChangeTypes.Renamed:
                    Console.WriteLine(string.Format("File {0} has been renamed\r\n", e.FullPath));
                    break;
            }
        }

        public void ChangeRaisingEvents(bool to)
        {
            Watcher.EnableRaisingEvents = to;
        }

        #region フォルダ操作
        private bool ParentExist(string name)
        {
            return Directory.Exists(string.Format(@"{0}\{1}", BackupDir, CreateParentFolderName(name)));
        }
        private void CreateBackupFolder(string name)
        {
            Directory.CreateDirectory(string.Format(@"{0}\{1}", BackupDir, CreateParentFolderName(name)));
        }
        private void CreateBackupItem(string from)
        {
            DirectoryInfo dir = new DirectoryInfo(from);
            Directory.Move(from, string.Format(@"{0}\[{1}]mv\{2}", BackupDir, dir.Name, CreateChildFolderName(dir)));

        }
        private string CreateParentFolderName(string name)
        {
            return string.Format("[{0}]mv", name);
        }
        private string CreateChildFolderName(DirectoryInfo di)
        {
            //[2013.12.7][23.53][1]
            DateTime now = DateTime.Now;
            string fullpath = string.Format(@"{0}\{1}", BackupDir, CreateParentFolderName(di.Name));
            Status st = new Status(fullpath);
            

            int childs = GetLatestID(di.Name);

            return string.Format("[{0}.{1}.{2}][{3}.{4}][{5}]", now.Year, now.Month, now.Day, now.Hour, now.Minute, childs);
        }

        private int GetLatestID(string itemname)
        {
            Status st = new Status(BackupDir);
            string fullpath = string.Format(@"{0}\{1}", BackupDir, CreateParentFolderName(itemname));
            List<ChildData> childs = Status.ConvertAllToChildData(st.GetChildren(new DirectoryInfo(fullpath))).ToList();
            childs.Sort((a, b) => a.ID.CompareTo(b.ID));
            return childs[0].ID;
        }
        #endregion
        

    }
}
