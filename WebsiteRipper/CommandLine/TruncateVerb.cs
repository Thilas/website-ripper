using System.IO;
using CommandLine;

namespace WebsiteRipper.CommandLine
{
    enum TruncateExitCode
    {
        OutputDoesNotExistError = 1,
    }

    [Verb("truncate", HelpText = "Truncate and update an existing ripped website. If the output does not exist, an error is raised.")]
    sealed class TruncateVerb : RipVerb
    {
        protected override RipMode RipMode { get { return RipMode.Truncate; } }

        protected override void Process()
        {
            if (!Directory.Exists(Output))
                throw new VerbInvalidOperationException("Output does not exist.", (int)TruncateExitCode.OutputDoesNotExistError);
            base.Process();
        }
    }
}
