using System;
using System.Threading;
using System.Windows;
using RegexDialog;

namespace CSharpRegexTools4VsCode
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Use a separate AppData folder so VS Code and Notepad++ configs don't conflict
            PathUtils.AppDataFolderName = "CSharpRegexTools4VsCode";

            var app = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };

            var handler = new JsonRpcHandler(app);

            var rpcThread = new Thread(() => handler.StartListening())
            {
                IsBackground = true,
                Name = "JsonRpcListener"
            };
            rpcThread.Start();

            app.Run();
        }
    }
}
