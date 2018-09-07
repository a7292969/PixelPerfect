using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PixelPerfect
{
    public class FileDownloader
    {
        private List<FileToDownload> files, allFiles;
        private long totalSize, downloadedSize, prevDownloadedSize, prevDownloadingIndex;
        private int downloadingIndex;
        private WebClient webClient;
        private MainWindow mw;

        public delegate void OnCompletedEventHandler(object sender);
        public delegate void OnProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);

        public event OnCompletedEventHandler OnCompleted;
        public event OnProgressChangedEventHandler OnProgressChanged;

        public FileDownloader(List<FileToDownload> files, MainWindow mw)
        {
            allFiles = files;
            this.files = new List<FileToDownload>();
            this.mw = mw;

            webClient = new WebClient();
            webClient.DownloadProgressChanged += changed;
            webClient.DownloadFileCompleted += completed;
        }

        public async Task Start()
        {
            prevDownloadedSize = 0;
            prevDownloadingIndex = -1;
            downloadedSize = 0;
            downloadingIndex = 0;

            await Task.Run(() => {
                totalSize = 0;
                files.Clear();
                foreach (FileToDownload file in allFiles)
                {
                    if (!File.Exists(file.path))
                    {
                        totalSize += file.size;
                        files.Add(file);
                    }
                }
            });

            downloadByIndex();
        }

        private void changed(object sender, DownloadProgressChangedEventArgs e)
        {
            if (prevDownloadingIndex != downloadingIndex)
            {
                if (files[downloadingIndex].size != e.TotalBytesToReceive)
                    Console.WriteLine(files[downloadingIndex].name + " " + files[downloadingIndex].size + " " + e.TotalBytesToReceive + " " + totalSize);

                prevDownloadedSize = 0;
                //totalSize -= files[downloadingIndex].size - e.TotalBytesToReceive;
            }

            downloadedSize += e.BytesReceived - prevDownloadedSize;
            prevDownloadedSize = e.BytesReceived;
            prevDownloadingIndex = downloadingIndex;

            try
            {
                mw.Dispatcher.Invoke(() => OnProgressChanged?.Invoke(this, new ProgressChangedEventArgs(downloadedSize, totalSize, files[downloadingIndex].name)));
            } catch { Console.WriteLine("ERR"); }
        }

        private void completed(object sender, AsyncCompletedEventArgs e)
        {
            downloadingIndex++;
            downloadByIndex();
        }

        private void downloadByIndex()
        {
            if (downloadingIndex == files.Count)
            {
                mw.Dispatcher.Invoke(() => OnCompleted?.Invoke(this));
                return;
            }

            string path = files[downloadingIndex].path;
            Directory.CreateDirectory(Directory.GetParent(path).FullName);
            webClient.DownloadFileAsync(new Uri(files[downloadingIndex].url), path);
        }
    }

    public class ProgressChangedEventArgs : EventArgs
    {
        public long Downloaded { get; private set; }
        public long TotalSize { get; private set; }
        public string CurrentFileName { get; private set; }

        public ProgressChangedEventArgs(long downloaded, long totalSize, string currentFilename)
        {
            Downloaded = downloaded;
            TotalSize = totalSize;
            CurrentFileName = currentFilename;
        }
    }
}
