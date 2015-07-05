using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.CommandLine
{
    sealed class ProgressConsole
    {
        readonly int _left = Console.CursorLeft;
        readonly int _top = Console.CursorTop;

        int _lastTop = 0;

        readonly bool _silent;
        readonly Task _task;
        readonly Task _reportTask;
        readonly Action _cancelAction;

        public ProgressConsole(bool silent, Task task, Action reportAction, Action cancelAction)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (reportAction == null) throw new ArgumentNullException("reportAction");
            if (cancelAction == null) throw new ArgumentNullException("cancelAction");
            if (!_silent) Console.CursorVisible = false;
            _silent = silent;
            _task = task;
            _reportTask = _task.ContinueWith(_ =>
            {
                if (!_silent)
                {
                    Console.SetCursorPosition(_left, _top + _lastTop);
                    Console.CursorVisible = true;
                }
                reportAction();
            });
            _cancelAction = cancelAction;
        }

        public void Wait()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            _reportTask.Wait();
            Console.CancelKeyPress -= Console_CancelKeyPress;
        }

        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _cancelAction();
            try { _task.Wait(); }
            catch
            {
                if (!_task.IsCanceled) throw;
            }
            _reportTask.Wait();
        }

        static readonly object _cursorLock = new object();

        const string DownloadProgressFormat = "{{0,-{0}}} {{1,3}}%";
        static readonly int _urlTotalWidth = Console.WindowWidth - string.Format(string.Format(DownloadProgressFormat, 0), null, null).Length;
        static readonly string _downloadProgressFormat = string.Format(DownloadProgressFormat, _urlTotalWidth);

        public void WriteProgress(string item, int progress)
        {
            if (!_silent)
            {
                lock (_cursorLock)
                {
                    var top = GetTop(item);
                    if (top < 0) return;
                    Console.SetCursorPosition(_left, _top + top);
                    var stringUrl = item.MiddleTruncate(_urlTotalWidth, "...");
                    Console.Write(_downloadProgressFormat, stringUrl, progress);
                    if (progress == 100) ReleaseTop(item);
                }
            }
        }

        readonly Dictionary<string, int> _usedTops = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        readonly List<int> _availableTops = new List<int>();

        int GetTop(string item)
        {
            if (_usedTops.ContainsKey(item)) return _usedTops[item];
            int top;
            if (_availableTops.Count > 0)
                _availableTops.Remove(top = _availableTops.Min());
            else
                top = _lastTop++;
            _usedTops.Add(item, top);
            return top;
        }

        void ReleaseTop(string item)
        {
            _availableTops.Add(_usedTops[item]);
            _usedTops[item] = -1;
        }
    }
}
