using CommandLine;

namespace WebsiteRipper.CommandLine
{
    [Verb("create", HelpText = "Create a new ripped website. If the output already exists, it will be overwritten.")]
    sealed class CreateVerb : RipVerb
    {
        protected override RipMode RipMode { get { return RipMode.Create; } }
    }
}
