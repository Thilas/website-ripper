using System;
using System.ComponentModel;

namespace WebsiteRipper
{
    public sealed class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public Uri Uri { get; private set; }
        public long BytesReceived { get; private set; }
        public long TotalBytesToReceive { get; private set; }

        internal DownloadProgressChangedEventArgs(Uri uri, long bytesReceived, long totalBytesToReceive)
            : base(GetProgressPercentage(bytesReceived, totalBytesToReceive), null)
        {
            Uri = uri;
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }

        static int GetProgressPercentage(long bytesReceived, long totalBytesToReceive)
        {
            if (totalBytesToReceive < 0L) return 0;
            if (totalBytesToReceive == 0L) return 100;
            var progressPercentage = (int)(100L * bytesReceived / totalBytesToReceive);
            if (progressPercentage < 0) return 0;
            if (progressPercentage > 100) return 100;
            return progressPercentage;
        }
    }
}
