using FreePIE.Core.Persistence;
using FreePIE.Core.Persistence.Paths;
using FreePIE.Core.Services;
using Ninject;

namespace FreePIE.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Core.ScriptEngine.Globals.ScriptHelpers.DiagnosticHelper.Version = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version
                .ToString();

            var kernel = ServiceBootstrapper.Create();
            kernel.Get<ConsoleHost>().Start(args);
        }
    }
}
