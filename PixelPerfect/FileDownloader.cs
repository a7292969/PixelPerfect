using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PixelPerfect
{
    public class FileDownloader
    {
        private List<FileToDownload> files;
        private long totalSize, downloadedSize;
        private int downloadingIndex;
        private WebClient webClient;

        public delegate void OnCompletedEventHandler(object sender);
        public delegate void OnProgressChangedEventHandler(object sender, ProgressEventArgs e);

        public event OnCompletedEventHandler OnCompleted;
        public event OnProgressChangedEventHandler OnProgressChanged;

        public FileDownloader(List<FileToDownload> files)
        {
            this.files = files;

            totalSize = 0;
            foreach (FileToDownload file in files)
            {
                totalSize += file.size;
            }

            webClient = new WebClient();
            webClient.DownloadProgressChanged += changed;
            webClient.DownloadFileCompleted += completed;
        }

        public void Start()
        {
            downloadedSize = 0;
            downloadingIndex = 0;

            downloadByIndex();
        }

        private void changed(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadedSize += e.BytesReceived;

            OnProgressChanged?.Invoke(this, new ProgressEventArgs(downloadedSize, totalSize, files[downloadingIndex].name));
        }

        private void completed(object sender, AsyncCompletedEventArgs e)
        {
            downloadingIndex++;
            downloadByIndex();
        }

        private void downloadByIndex()
        {
            while (downloadingIndex < files.Count)
            {
                string path = files[downloadingIndex].path;
                string normalHash = files[downloadingIndex].hash;

                if (File.Exists(path) && Utils.computeFileHash(path) == normalHash)
                {
                    downloadingIndex++;
                }
                else
                {
                    Directory.CreateDirectory(Directory.GetParent(path).FullName);
                    webClient.DownloadFileAsync(new Uri(files[downloadingIndex].url), path);
                    break;
                }
            }

            if (downloadingIndex == files.Count)
                OnCompleted?.Invoke(this);
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public long TotalDownloaded { get; private set; }
        public long ToDownload { get; private set; }
        public string CurrentFileName { get; private set; }

        public ProgressEventArgs(long totalDownloaded, long toDownload, string currentFilename)
        {
            TotalDownloaded = totalDownloaded;
            ToDownload = toDownload;
            CurrentFileName = currentFilename;
        }
    }
}
