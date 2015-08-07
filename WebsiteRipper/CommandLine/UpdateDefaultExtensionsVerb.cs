using System;
using CommandLine;

namespace WebsiteRipper.CommandLine
{
    [Verb("update-default-extensions", HelpText = "Update default extensions file.")]
    sealed class UpdateDefaultExtensionsVerb : Verb
    {
        [Option("skip-iana", HelpText = "Do not update iana mime types file.")]
        public bool SkipIanaMimeTypes { get; set; }

        protected override void Process()
        {
            Console.WriteLine("Update default extensions file");
            DefaultExtensions.Update(SkipIanaMimeTypes);
            Console.WriteLine("Updating completed");
        }
    }
}
