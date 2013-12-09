using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Multiverse
{

    public struct ChildData
    {
        public DateTime DateTime;
        public int ID;

        public ChildData(DateTime datetime, int id = -1)
        {
            DateTime = datetime;
            ID = id;
        }
        public ChildData(string year, string month, string day, string id)
        {
            DateTime = new DateTime(
                int.Parse(year),
                int.Parse(month),
                int.Parse(day)
                );
            ID = int.Parse(id);
        }
        public ChildData(string year, string month, string day, string hour, string minute, string id)
        {
            DateTime = new DateTime(
                int.Parse(year),
                int.Parse(month),
                int.Parse(day),
                int.Parse(hour),
                int.Parse(minute),
                0
                );
            ID = int.Parse(id);
        }

        public static ChildData GetNullChildData()
        {
            ChildData cd = new ChildData()
            {
                ID = -1
            };
            return cd;
        }

        public override string ToString()
        {
            return string.Format("[{0}.{1}.{2}][{3}.{4}][{5}]", DateTime.Year, DateTime.Month, DateTime.Day, DateTime.Hour, DateTime.Minute, ID);
        }
    }
    public struct ParentData
    {
        public string Name;
        public ParentData(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return string.Format("[{0}]mv", Name);
        }
    }

    public struct Status
    {
        //public const string ParentRegex = @"(?<Year>(?:\d{4}))\.(?<Month>\d{1,2})\.(?<Day>\d{1,2})\[(?<ItemName>(\w+\s*\w*))\]mv"; //OLD one.
        //public const string ParentRegex = @"\[(?<ItemName>(\w+\s*\w*))\]mv";
        public const string ParentRegex = @"\[(?<ItemName>([\u0000-\u007F\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FFF]*))\]mv"; //most of the unicode chars available now
        public const string ChildRegex = @"\[(?<Year>(?:\d{4}))\.(?<Month>\d{1,2})\.(?<Day>\d{1,2})\]\[(?<Hour>(?:\d{1,2}))\.(?<Minute>\d{1,2})\]\[(?<ID>\d+)\]";
        public static readonly string[] ParentGroups = { "ItemName" };
        public static readonly string[] ChildGroups = { "Year", "Month", "Day", "Hour", "Minute", "ID" };
        public string ItemName { get; set; }
        public long LastModified { get; set; }
        public long CreatedAt { get; set; }
        public int FolderCount { get; private set; }
        public DirectoryInfo SelfDirectory;


        public Status(string folderpath)
            : this()
        {
            try
            {

                DirectoryInfo di = new DirectoryInfo(folderpath);
                SelfDirectory = di;
                ItemName = di.Name;
                LastModified = di.LastWriteTime.ToFileTime();
                CreatedAt = di.CreationTime.ToFileTime();
                //2013.12.7[multiverse]mv
                //year.month.date[itemname]mv

                //(?<Year>\d{4})\.(?<Month>\d{1,2})\.(?<Day>\d{1,2})\[(?<ItemName>\w+)\]mv
                //Year: \d\d\d\d -> 4 digits
                //Month: \d\d -> 1 or 2 digits
                //Day: \d\d -> 1 or 2 digits
                //ItemName: \w+ -> one or more letter or number. no signs allowed
                //mv: mv

                var regexTest = new Func<DirectoryInfo, bool>(i => Regex.IsMatch(i.Name, ParentRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                DirectoryInfo[] dirs = di.GetDirectories().Where(regexTest).ToArray();

                FolderCount = dirs.Count();

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public DirectoryInfo[] GetParents()
        {
            //[2013.12.7][23.53][0]
            var regexTest = new Func<DirectoryInfo, bool>(i => Regex.IsMatch(i.Name, ParentRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase)); //&& (!i.Name.Trim().Replace("mv", "").Equals("[]"))
            var regexSecond = new Func<DirectoryInfo, bool>(i => !Regex.IsMatch(i.Name, @"\[\s*\]mv", RegexOptions.Compiled | RegexOptions.IgnoreCase)); //remove [   ]mv ones
            return SelfDirectory.GetDirectories().Where(regexTest).Where(regexSecond).ToArray();
            
        }
        public DirectoryInfo[] GetChildren(DirectoryInfo di)
        {
            //[2013.12.7][23.53][0]
            var regexTest = new Func<DirectoryInfo, bool>(i => Regex.IsMatch(i.Name, ChildRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            var dirs = di.GetDirectories();
            var where = dirs.Where(regexTest).ToArray();
            return where;
        }

        public static ChildData ConvertToChildData(DirectoryInfo di)
        {
            GroupCollection gc = Regex.Match(di.Name, new Regex(ChildRegex).ToString()).Groups;
            return new ChildData(gc["Year"].Value, gc["Month"].Value, gc["Day"].Value, gc["Hour"].Value, gc["Minute"].Value, gc["ID"].Value);
        }
        public static ParentData ConvertToParentData(DirectoryInfo di)
        {
            GroupCollection gc = Regex.Match(di.Name, new Regex(ParentRegex).ToString()).Groups;
            return new ParentData(gc["ItemName"].Value);
        }


        public bool GetLatestChildFolder(out ChildData c)
        {
            Queue<ChildData> Childs = new Queue<ChildData>();
            var childs = GetChildren(SelfDirectory);
            foreach (DirectoryInfo item in childs)
            {
                Childs.Enqueue(ConvertToChildData(item));
            }

            Childs = new Queue<ChildData>(Childs.OrderByDescending(a => a.DateTime).ThenByDescending(a=>a.ID)); //filters by date first, then sort by latest id
            if (Childs.Count != 0)
            {
                c = Childs.Dequeue();
                return (true);
            }
            c = ChildData.GetNullChildData();
            return false;

        }
        public static IEnumerable<ChildData> ConvertAllToChildData(DirectoryInfo[] dis)
        {
            foreach (DirectoryInfo di in dis)
            {
                yield return ConvertToChildData(di);
            }
        }
        public static IEnumerable<ParentData> ConvertAllToParentData(DirectoryInfo[] dis)
        {
            foreach (DirectoryInfo di in dis)
            {
                yield return ConvertToParentData(di);
            }
        }

    }
}
