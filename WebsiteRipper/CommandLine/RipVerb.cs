using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace WebsiteRipper.CommandLine
{
    abstract class RipVerb : Verb
    {
        protected abstract RipMode RipMode { get; }

        [Value(0, MetaName = "<uri>", Required = true, HelpText = "Base uri of the website to rip.")]
        public string Uri { get; set; }

        [Option('o', "output", MetaValue = "<path>", Default = ".", HelpText = "Output location.")]
        public string Output { get; set; }

        [Option('l', "langs", MetaValue = "<codes>", Separator = ',', HelpText = "List of comma-separated languages.")]
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

        const int TimeoutMultiplier = 1000;

        [Option('t', "timeout", MetaValue = "<seconds>", Default = DefaultExtensions.Timeout / TimeoutMultiplier, HelpText = "Time-out value in seconds for each resources.")]
        public int Timeout { get; set; }

        [Option('b', "is-base", Default = false, HelpText = "Download resources below base uri only.")]
        public bool IsBase { get; set; }

        [Option('d', "max-depth", MetaValue = "<value>", Default = 0, HelpText = "Max download depth for resources.")]
        public int MaxDepth { get; set; }

        [Option('i', "include", MetaValue = "<pattern>", HelpText = "Download resources matching this regex pattern.")]
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

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                var unParserSettings = new UnParserSettings() { PreferShortName = true, GroupSwitches = true };
                yield return new Example("Rip a website", unParserSettings, new CreateVerb()
                {
                    Uri = "http://my.website.com",
                    Output = @"C:\Path\Website",
                    Timeout = 30,
                    MaxDepth = 1
                });
                yield return new Example("Update a ripped website", unParserSettings, new UpdateVerb()
                {
                    Uri = "http://my.website.com",
                    Langs = new[] { "en-US", "en" },
                    Include = @"^http://my\.website\.com/files/"
                });
            }
        }
    }
}
