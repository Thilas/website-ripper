using CommandLine;
using System;

namespace WebsiteRipper.CommandLine
{
    [Verb("update", HelpText = "Update default extensions file")]
    sealed class UpdateVerb : Verb
    {
        protected override void Process()
        {
            Console.WriteLine("Update default extensions file");
            DefaultExtensions.Update();
            Console.WriteLine("Updating completed");
        }
    }
}
