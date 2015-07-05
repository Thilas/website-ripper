using CommandLine;
using System;
using WebsiteRipper.Core;

namespace WebsiteRipper.CommandLine
{
    [Verb("update", HelpText = "Update default extensions file")]
    sealed class UpdateVerb : Verb
    {
        protected override void Process()
        {
            if (!Silent) Console.WriteLine("Update default extensions file");
            DefaultExtensions.Update();
            if (!Silent) Console.WriteLine("Updating completed");
        }
    }
}
