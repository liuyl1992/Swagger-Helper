using McMaster.Extensions.CommandLineUtils;
using System;

namespace Lunz.SwaggerHelper
{
    class Program
    {
        static void Main(string[] args) => CommandLineApplication.Execute<SwaggerCommands>(args);
    }
}
