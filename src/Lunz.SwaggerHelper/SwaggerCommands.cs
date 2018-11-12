using McMaster.Extensions.CommandLineUtils;
using System.Reflection;

namespace Lunz.SwaggerHelper
{
    [Command(Description = "Swagger 辅助工具集。")]
    [VersionOptionFromMember("--version|-v", MemberName = nameof(GetVersion))]
    [Subcommand("generate", typeof(SwaggerGenerator))]
    public class SwaggerCommands
    {
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("请指定一个命令。");
            console.WriteLine();
            app.ShowHelp();

            return 1;
        }

        private static string GetVersion()
            => typeof(SwaggerCommands).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
