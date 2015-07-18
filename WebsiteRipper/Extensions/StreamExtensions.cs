using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteRipper.Extensions
{
    static class StreamExtensions
    {
        public static async Task<long> CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress, CancellationToken cancellationToken)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException("bufferSize");
            if (!source.CanRead && !source.CanWrite) throw new ObjectDisposedException("source");
            if (!destination.CanRead && !destination.CanWrite) throw new ObjectDisposedException("destination");
            if (!source.CanRead) throw new NotSupportedException("Stream does not support reading.");
            if (!destination.CanWrite) throw new NotSupportedException("Stream does not support writing.");
            var buffer = new byte[bufferSize];
            var total = 0L;
            int count;
            while ((count = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
            {
                await destination.WriteAsync(buffer, 0, count, cancellationToken);
                if (progress != null) progress.Report(total += count);
            }
            return total;
        }
    }
}
