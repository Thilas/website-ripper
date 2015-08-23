using System;

namespace WebsiteRipper.CommandLine
{
    sealed class VerbInvalidOperationException : Exception
    {
        public int ExitCode { get; private set; }

        internal VerbInvalidOperationException(string message, int exitCode, Exception innerException = null)
            : base(message, innerException)
        {
            if (exitCode <= 0) throw new ArgumentOutOfRangeException("exitCode");
            ExitCode = exitCode;
        }

        public override string Message { get { return InnerException.Message; } }
    }
}
