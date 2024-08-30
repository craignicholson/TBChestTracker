﻿using Emgu.CV.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for MoveClanFolderWindow.xaml
    /// </summary>
    public partial class MoveClanFolderWindow : Window, INotifyPropertyChanged
    {

        public string OldClanRootFOlder { get; set; }
        public string NewClanRootFolder {  get; set; }

        private string _status = "";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));  
            }
        }

        private Decimal _progress = 0;
        public Decimal Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        private decimal _max;
        public decimal Max
        {
            get => _max;
            set
            {
                _max = value;
                OnPropertyChanged(nameof(Max)); 
            }
        }
        private string _percent = "0%";
        public string Percent
        {
            get => _percent;
            set
            {
                _percent = value;
                OnPropertyChanged(nameof(Percent));
            }
        }

        private bool bAborted = false;

        private CancellationTokenSource _CancellationTokenSource;

        public MoveClanFolderWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) 
            { 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            if (AbortButton.Text == "Abort")
            {
                bAborted = true;
                this.Close();
            }
            else if(AbortButton.Text == "Close")
            {
                this.Close();
            }
        }

        private async void CopyDirectoryAsync(string sourceDir, string destinationDir, bool recursive, CancellationToken token) 
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (Directory.Exists(destinationDir) == false)
            {
                Directory.CreateDirectory(destinationDir);
            }

            var index = 0;
            bAborted = false;

            foreach(var file in Directory.EnumerateFiles(sourceDir))
            {
                using (FileStream sourceStream = File.Open(file, FileMode.Open))
                {
                    var destFile = $"{destinationDir}{file.Substring(file.LastIndexOf("\\"))}";
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Max = Directory.EnumerateFiles(sourceDir).Count();
                        Status = $"Copying - {destFile}";
                        Progress = index;
                        var percentage = (index / Max) * 100 + "%" ;
                        Percent = percentage;

                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    using (FileStream destinationStream = File.OpenWrite(destFile))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
                if(token.IsCancellationRequested)
                {
                    bAborted = true;
                    break;
                }
                index++;
            }
            index = 0;
            if(bAborted)
            {
                this.Close();

            }
            if(recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectoryAsync(subDir.FullName, newDestinationDir, true, token);
                }
            }

            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Max = 100;
                Status = $"Complete!";
                Progress = 100;
                var percentage = 100 + "%";
                Percent = percentage;
                AbortButton.Text = "Close";
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
     
        private Task BeginCopyingTask(string oldFolder, string newFolder, CancellationToken token)
        {
            return Task.Run(() =>
            {
                CopyDirectoryAsync(oldFolder, newFolder, true, token);
            });
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _CancellationTokenSource = new CancellationTokenSource();
            BeginCopyingTask(OldClanRootFOlder, NewClanRootFolder, _CancellationTokenSource.Token);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = this.bAborted == true ? false : true;
        }
    }
}
