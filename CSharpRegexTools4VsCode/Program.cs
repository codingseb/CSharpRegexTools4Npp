using System;
using System.Threading;
using System.Windows;

namespace CSharpRegexTools4VsCode
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
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
