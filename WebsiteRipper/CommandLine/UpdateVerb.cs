using System;
using CommandLine;

namespace WebsiteRipper.CommandLine
{
    [Verb("update", HelpText = "Update default extensions file")]
    sealed class UpdateVerb : Verb
    {
        [Option("skip-iana", HelpText = "Do not update iana mime types file")]
        public bool SkipIanaMimeTypes { get; set; }

        protected override void Process()
        {
            Console.WriteLine("Update default extensions file");
            DefaultExtensions.Update(SkipIanaMimeTypes);
            Console.WriteLine("Updating completed");
        }
    }
}
