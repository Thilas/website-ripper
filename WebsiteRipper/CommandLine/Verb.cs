using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.CommandLine
{
    enum ExitCode
    {
        Success = 0,
        UnexpectedError = -1,
        ArgumentsError = -2
    }

    abstract class Verb
    {
        static readonly Lazy<IEnumerable<Type>> _verbsLazy = new Lazy<IEnumerable<Type>>(() =>
        {
            var abstractVerbType = typeof(Verb);
            var verbAttributeType = typeof(VerbAttribute);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(abstractVerbType) && type.IsDefined(verbAttributeType, false))
                .OrderBy(type => type.GetCustomAttribute<VerbAttribute>(false).Name, StringComparer.OrdinalIgnoreCase)
                .ToList().AsEnumerable();
        });

        internal static int Process(IEnumerable<string> args)
        {
            var exitCode = ExitCode.Success;
            try
            {
                Parser.Default.ParseArguments(args, _verbsLazy.Value.ToArray())
                    .WithParsed<Verb>(verb => verb.Process())
                    .WithNotParsed(_ => exitCode = ExitCode.ArgumentsError);
            }
            catch (VerbInvalidOperationException exception)
            {
                Console.Error.WriteLine("Error: {0}", exception.Message);
                exitCode = (ExitCode)exception.ExitCode;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Unexpected error: {0}", exception.Message);
                exitCode = ExitCode.UnexpectedError;
            }
            finally
            {
#if DEBUG
                Console.WriteLine();
                Console.WriteLine("Exit code: {0}", exitCode);
                Console.ReadLine();
#endif
            }
            return (int)exitCode;
        }

        abstract protected void Process();
    }
}
