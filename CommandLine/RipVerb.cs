using CommandLine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WebsiteRipper.CommandLine
{
    [Verb("rip", HelpText = "Rip a website from a base url")]
    sealed class RipVerb : Verb
    {
        [Value(0, Required = true)]
        public string Url { get; set; }

        [Option('o', "output", Default = ".", HelpText = "Output location")]
        public string Output { get; set; }

        [Option('l', "langs", Separator = ',', HelpText = "Languages")]
        public IEnumerable<string> Langs { get; set; }

        IEnumerable<CultureInfo> Languages
        {
            get
            {
                var languages = Langs.Select(lang =>
                {
                    try { return new CultureInfo(lang); }
                    catch
                    {
                        Console.Error.WriteLine("Invalid language: {0}", lang);
                        return null;
                    }
                }).Where(language => language != null);
                return languages.Any() ? languages : null;
            }
        }

        [Option(Default = false, HelpText = "Overwrite existing resource(s)")]
        public bool Overwrite { get; set; }

        RipMode RipMode { get { return Overwrite ? RipMode.Create : RipMode.Create; } }

        [Option('t', "timeout", Default = 30, HelpText = "Time-out value in seconds for each download")]
        public int Timeout { get; set; }

        [Option('b', "isBase", Default = false, HelpText = "Download resources below base url only")]
        public bool IsBase { get; set; }

        [Option('d', "maxDepth", Default = 0, HelpText = "Max download depth")]
        public int MaxDepth { get; set; }

        [Option('i', "include", HelpText = "Download resources matching this regex pattern")]
        public string Include { get; set; }

        ProgressConsole _progressConsole;

        protected override void Process()
        {
            var languages = Languages;
            var ripper = languages == null ? new Ripper(Url, Output) : new Ripper(Url, Output, languages);
            ripper.Timeout = (int)TimeSpan.FromSeconds(Timeout).TotalMilliseconds;
            ripper.IsBase = IsBase;
            ripper.MaxDepth = MaxDepth;
            ripper.IncludePattern = Include;

            if (!Silent)
            {
                Console.WriteLine("Rip website: {0}", Url);
                Console.WriteLine("to: {0}", ripper.Resource.NewUrl);
            }
            var rippingTask = ripper.RipAsync(RipMode);
            _progressConsole = new ProgressConsole(Silent, rippingTask, () =>
            {
                if (!Silent)
                {
                    Console.WriteLine("Ripping {0}", rippingTask.IsCanceled || rippingTask.IsFaulted ? rippingTask.Status.ToString() : "completed");
                    if (rippingTask.IsFaulted) Console.Error.WriteLine("Fault: {0}", rippingTask.Exception);
                }
            }, () =>
            {
                ripper.Cancel();
            });
            ripper.DownloadProgressChanged += ripper_DownloadProgressChanged;
            _progressConsole.Wait();
            ripper.DownloadProgressChanged -= ripper_DownloadProgressChanged;
        }

        void ripper_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _progressConsole.WriteProgress(string.Format("- {0}", e.Url), e.ProgressPercentage);
        }
    }
}
