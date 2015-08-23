using System.IO;
using CommandLine;

namespace WebsiteRipper.CommandLine
{
    enum UpdateExitCode
    {
        OutputDoesNotExistError = 1,
    }

    [Verb("update", HelpText = "Update an existing ripped website. If the output does not exist, an error is raised.")]
    sealed class UpdateVerb : RipVerb
    {
        protected override RipMode RipMode { get { return RipMode.Update; } }

        protected override void Process()
        {
            if (!Directory.Exists(Output))
                throw new VerbInvalidOperationException("Output does not exist.", (int)UpdateExitCode.OutputDoesNotExistError);
            base.Process();
        }
    }
}
