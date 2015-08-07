using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommandLine;

namespace WebsiteRipper.CommandLine
{
    [Verb("rip", HelpText = "Rip a website from a base uri")]
    sealed class RipVerb : Verb
    {
        [Value(0, MetaName = "Uri", Required = true, HelpText = "Base uri of the website to rip")]
        public string Uri { get; set; }

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
                }).Where(language => language != null).ToList();
                return languages.Any() ? languages : null;
            }
        }

        [Option(Default = false, HelpText = "Update existing resource(s)")]
        public bool Update { get; set; }

        // TODO: Add verbs for each RipMode
        RipMode RipMode { get { return Update ? RipMode.UpdateOrCreate : RipMode.CreateNew; } }

        const int TimeoutMultiplier = 1000;

        [Option('t', "timeout", Default = DefaultExtensions.Timeout / TimeoutMultiplier, HelpText = "Time-out value in seconds for each download")]
        public int Timeout { get; set; }

        [Option('b', "is-base", Default = false, HelpText = "Download resources below base uri only")]
        public bool IsBase { get; set; }

        [Option('d', "max-depth", Default = 0, HelpText = "Max download depth")]
        public int MaxDepth { get; set; }

        [Option('i', "include", HelpText = "Download resources matching this regex pattern")]
        public string Include { get; set; }

        ProgressConsole _progressConsole;

        protected override void Process()
        {
            var languages = Languages;
            var ripper = languages == null ? new Ripper(Uri, Output) : new Ripper(Uri, Output, languages);
            ripper.Timeout = Timeout * TimeoutMultiplier;
            ripper.IsBase = IsBase;
            ripper.MaxDepth = MaxDepth;
            ripper.IncludePattern = Include;

            Console.WriteLine("Rip website: {0}", Uri);
            Console.WriteLine("to: {0}", ripper.Resource.NewUri);
            var rippingTask = ripper.RipAsync(RipMode);
            _progressConsole = new ProgressConsole(rippingTask, () =>
            {
                Console.WriteLine("Ripping {0}", rippingTask.IsCanceled || rippingTask.IsFaulted ? rippingTask.Status.ToString() : "completed");
                if (rippingTask.IsFaulted) Console.Error.WriteLine("Fault: {0}", rippingTask.Exception);
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
            _progressConsole.WriteProgress(string.Format("- {0}", e.Uri), e.ProgressPercentage);
        }
    }
}
