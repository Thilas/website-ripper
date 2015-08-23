using CommandLine;

namespace WebsiteRipper.CommandLine
{
    [Verb("update-or-create", HelpText = "Update a ripped website if the output exists; otherwise, a new ripped website should be created.")]
    sealed class UpdateOrCreateVerb : RipVerb
    {
        protected override RipMode RipMode { get { return RipMode.UpdateOrCreate; } }
    }
}
