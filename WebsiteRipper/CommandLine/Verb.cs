using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using WebsiteRipper.Downloaders;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.CommandLine
{
    enum ExitCode
    {
        Success = 0,
        UnexpectedError = -1,
        ArgumentsError = -2
    }

    // TODO: Add usage examples for each verb
    abstract class Verb
    {
        static readonly Lazy<IEnumerable<Type>> _verbsLazy = new Lazy<IEnumerable<Type>>(() =>
        {
            var abstractVerbType = typeof(Verb);
            var verbAttributeType = typeof(VerbAttribute);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(abstractVerbType) && type.IsDefined(verbAttributeType, false))
                .Select(type => new { Type = type, Attribute = type.GetCustomAttribute<VerbAttribute>(false) })
                .Where(verb => verb.Attribute != null)
                .OrderBy(verb => verb.Attribute.Name, StringComparer.OrdinalIgnoreCase)
                .Select(verb => verb.Type)
                .ToList();
        });

        internal static int Process(IEnumerable<string> args)
        {
            var exitCode = Parser.Default.ParseArguments(args, _verbsLazy.Value.ToArray())
                .Return((Verb verb) => verb.TryProcess(), _ => ExitCode.ArgumentsError);
#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Exit code: {0}", exitCode);
            Console.ReadLine();
#endif
            return (int)exitCode;
        }

        [Option('u', "user-agent", HelpText = "Downloader user agent")]
        public string UserAgent { set { HttpDownloader.UserAgent = value; } }

        ExitCode TryProcess()
        {
            try
            {
                Process();
                return ExitCode.Success;
            }
            catch (VerbInvalidOperationException exception)
            {
                Console.Error.WriteLine("Error: {0}", exception.Message);
                return (ExitCode)exception.ExitCode;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Unexpected error: {0}", exception.Message);
                return ExitCode.UnexpectedError;
            }
        }

        abstract protected void Process();
    }
}
