using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteRipper.Extensions;
using WebsiteRipper.Internal;

namespace WebsiteRipper
{
    static class Program
    {
        static readonly int _left = Console.CursorLeft;
        static readonly int _top = Console.CursorTop;

        static int _lastTop = 0;

        static void Main(string[] args)
        {
            DefaultExtensions.Update();
            var WebsiteRipper = DefaultExtensions.All;
            var a = true;
            if (a) return;
            var ripper = new Ripper("http://www.centralenum.org/galerie-publique/2014-2015/Raid/Photos%20Diapo/J3/page/4/", @"C:\Users\tdemoulins\Desktop\Perso\Raid\Test")
            {
                Timeout = 5000,
                //ForceRipResources = false,
                MaxDepth = 1
                //IsBase = true,
                //IncludePattern = @"^http://www\.iana\.org/assignments/media-types/.+/.+$"
            };
            Console.CursorVisible = false;
            ripper.DownloadProgressChanged += ripper_DownloadProgressChanged;
            var rippingTask = ripper.RipAsync(RipMode.Create);
            var reportTask = rippingTask.ContinueWith(task =>
            {
                if (task == null) throw new ArgumentNullException("task");
                Console.SetCursorPosition(_left, _top + _lastTop);
                Console.CursorVisible = true;
                Console.WriteLine("Ripping {0}", task.IsCanceled || task.IsFaulted ? task.Status.ToString() : "completed");
                if (task.IsFaulted) Console.WriteLine("Fault: {0}", task.Exception);
                Console.WriteLine("...");
                Console.ReadLine();
            });
            Console.CancelKeyPress += (sender, e) =>
            {
                ripper.DownloadProgressChanged -= ripper_DownloadProgressChanged;
                ripper.Cancel();
                try { rippingTask.Wait(); }
                catch
                {
                    if (!rippingTask.IsCanceled) throw;
                }
                reportTask.Wait();
            };
            reportTask.Wait();
        }

        static void ripper_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            WriteDownloadProgress(e.Url, e.ProgressPercentage);
        }

        static readonly object _cursorLock = new object();

        const string DownloadProgressFormat = "{{0,-{0}}} {{1,3}}%";
        static readonly int _urlTotalWidth = Console.WindowWidth - string.Format(string.Format(DownloadProgressFormat, 0), null, null).Length;
        static readonly string _downloadProgressFormat = string.Format(DownloadProgressFormat, _urlTotalWidth);

        static void WriteDownloadProgress(Uri url, int progress)
        {
            lock (_cursorLock)
            {
                var top = GetTop(url);
                if (top < 0) return;
                Console.SetCursorPosition(_left, _top + top);
                var stringUrl = url.ToString().MiddleTruncate(_urlTotalWidth, "...");
                Console.Write(_downloadProgressFormat, stringUrl, progress);
                if (progress == 100) ReleaseTop(url);
            }
        }

        static readonly Dictionary<Uri, int> _usedTops = new Dictionary<Uri, int>();
        static readonly List<int> _availableTops = new List<int>();

        static int GetTop(Uri url)
        {
            if (_usedTops.ContainsKey(url)) return _usedTops[url];
            int top;
            if (_availableTops.Count > 0)
                _availableTops.Remove(top = _availableTops.Min());
            else
                top = _lastTop++;
            _usedTops.Add(url, top);
            return top;
        }

        static void ReleaseTop(Uri url)
        {
            _availableTops.Add(_usedTops[url]);
            _usedTops[url] = -1;
        }
    }
}
