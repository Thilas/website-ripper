using System.IO;
using CommandLine;

namespace WebsiteRipper.CommandLine
{
    enum CreateNewExitCode
    {
        OutputAlreadyExistsError = 1,
    }

    [Verb("create-new", HelpText = "Create a new ripped website. If the output already exists, an error is raised.")]
    sealed class CreateNewVerb : RipVerb
    {
        protected override RipMode RipMode { get { return RipMode.CreateNew; } }

        protected override void Process()
        {
            if (Directory.Exists(Output))
                throw new VerbInvalidOperationException("Output already exists.", (int)CreateNewExitCode.OutputAlreadyExistsError);
            base.Process();
        }
    }
}
