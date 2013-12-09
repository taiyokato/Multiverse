using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;

namespace Multiverse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Processor Processor;
        private string monitordir, backupdir;

        public MainWindow()
        {
            InitializeComponent();
            UISetup();
            monitordir = @"D:\Dump";
            backupdir = @"D:\TestFolder";
            Status st = new Status(backupdir);
            ParentData[] parents = Status.ConvertAllToParentData(st.GetParents()).ToArray();

            foreach (ParentData p in parents)
            {
                Status st2 = new Status(string.Format(@"{0}\{1}", st.SelfDirectory.FullName, p.ToString()));
                Console.WriteLine(st2.ItemName);
                DirectoryInfo[] dis = st2.GetChildren(st2.SelfDirectory);
                foreach (DirectoryInfo item in dis)
                {
                    Console.WriteLine(item.Name);
                }

                Console.WriteLine("--------------");
                ChildData cd = new ChildData();
                bool success = st2.GetLatestChildFolder(out cd);
                if (success) Console.WriteLine("Latest: " + cd.ToString());


            }
            
            //Directory.CreateDirectory(@"D:\Dump\Multiverse test folder");

        }



        public void ClassInitialize()
        {
            
        }

        public void UISetup()
        {
            backupDirRect.Fill = Brushes.Red;
            monitorDirRect.Fill = Brushes.Red;
        }


        
        #region UI Controls
        public void StartMonitor(object sender, RoutedEventArgs e)
        {
            Processor = new Processor(monitordir, backupdir);

        }

        public void SetMonitor(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.Description = "Select monitor folder";
            DialogResult dr = fbd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                monitordir = fbd.SelectedPath;
                MonitorDirBox.Text = monitordir;
                monitorDirRect.Fill = Brushes.Green;
            }
        }
        public void SetBackup(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.Description = "Select backup folder";
            DialogResult dr = fbd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                backupdir = fbd.SelectedPath;
                backupDirBox.Text = backupdir;
                backupDirRect.Fill = Brushes.Green;
            }
        }
        #endregion

        private void MonitorSwitch_Copy_Click(object sender, RoutedEventArgs e)
        {
            monitordir = @"D:\Dump";
            backupdir = @"D:\TestFolder";
            monitorDirRect.Fill = Brushes.Green;
            backupDirRect.Fill = Brushes.Green;
        }
    }
}
