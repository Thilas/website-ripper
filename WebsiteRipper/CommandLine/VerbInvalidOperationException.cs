using System;

namespace WebsiteRipper.CommandLine
{
    sealed class VerbInvalidOperationException : Exception
    {
        public int ExitCode { get; private set; }

        internal VerbInvalidOperationException(int exitCode, Exception innerException)
            : base(null, innerException)
        {
            if (exitCode <= 0) throw new ArgumentOutOfRangeException("exitCode");
            ExitCode = exitCode;
        }

        public override string Message { get { return InnerException.Message; } }
    }
}
