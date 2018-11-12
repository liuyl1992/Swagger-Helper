using Lunz.Swagger;
using Lunz.Swagger.Models;
using Lunz.Swagger.Models.Ocelot;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace Lunz.SwaggerHelper
{
    [Command(Name = "generate", Description = "Swagger 生成器工具。")]
    [HelpOption]
    [Subcommand("ocelot", typeof(Ocelot))]
    public class SwaggerGenerator
    {
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("请指定一个子命令。");
            console.WriteLine();
            app.ShowHelp();
            return 1;
        }

        [Command(Name = "ocelot", Description = "Ocelot")]
        [HelpOption]
        private class Ocelot
        {
            [Option("-s|--savePath", "Swagger 文档文件保存的路径。", CommandOptionType.SingleValue)]
            [LegalFilePath]
            public string SavePath { get; }
            [Option("-c|--configPath", "用于生成 Ocelot Swagger 文档的配置文件所在的路径。", CommandOptionType.SingleValue)]
            [LegalFilePath]
            public string ConfigPath { get; }
            [Option("-o|--ocelotPath", "Ocelot 配置文件所在的路径。", CommandOptionType.SingleValue)]
            [LegalFilePath]
            public string OcelotPath { get; }

            private void OnExecute(CommandLineApplication app, IConsole console)
            {
                #region 验证

                var deleteFile = false;
                if (File.Exists(SavePath))
                {
                    deleteFile = Prompt.GetYesNo($"{SavePath}{Environment.NewLine}文件已经存在，您确认覆盖当前文件吗？", false, ConsoleColor.Yellow);
                    if (!deleteFile)
                    {
                        return;
                    }
                }

                if (!File.Exists(ConfigPath))
                {
                    console.ForegroundColor = ConsoleColor.Yellow;
                    console.WriteLine("生成 Ocelot Swagger 文档的配置文件不存在。");
                    console.WriteLine(ConfigPath);
                    console.ResetColor();
                    return;
                }

                if (!File.Exists(OcelotPath))
                {
                    console.ForegroundColor = ConsoleColor.Yellow;
                    console.WriteLine("Ocelot 配置文件不存在。");
                    console.WriteLine(OcelotPath);
                    console.ResetColor();
                    return;
                }

                #endregion

                var generator = new OcelotSwaggerGenerator()
                {
                    ThrowException = false
                };

                var settings = OcelotSwaggerSettings.FromFile(ConfigPath);
                var configuration = Configuration.FromFile(OcelotPath);

                var doc = generator.Generate(settings, configuration).GetAwaiter().GetResult();

                try
                {
                    var json = doc.ToJson();

                    if (deleteFile)
                    {
                        File.Delete(SavePath);
                    }

                    using (var sw = new StreamWriter(SavePath))
                    {
                        sw.Write(json);
                    }

                    console.ForegroundColor = ConsoleColor.Green;
                    console.WriteLine("已完成 Swagger 文档生成，请查看：");
                    console.WriteLine(SavePath);
                    console.ResetColor();
                }
                catch (Exception ex)
                {
                    console.ForegroundColor = ConsoleColor.Red;
                    console.WriteLine(ex.Message);
                    console.WriteLine("生成失败，请查看日志。");
                    console.ResetColor();
                }
            }
        }
    }
}
