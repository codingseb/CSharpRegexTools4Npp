using System;
using System.Windows;
using Microsoft.VisualStudio.Threading;
using RegexDialog;
using StreamJsonRpc;

namespace CSharpRegexTools4VsCode
{
    class JsonRpcHandler
    {
        private readonly Application app;
        private readonly JoinableTaskContext jtc;
        private readonly JoinableTaskFactory jtf;
        private JsonRpc rpc;
        private RegExToolDialog dialog;

        public JsonRpcHandler(Application app)
        {
            this.app = app;
            // JoinableTaskContext must be created on the STA/UI thread
            jtc = new JoinableTaskContext();
            jtf = jtc.Factory;
        }

        public void StartListening()
        {
            var stdin = Console.OpenStandardInput();
            var stdout = Console.OpenStandardOutput();

            var messageHandler = new HeaderDelimitedMessageHandler(stdout, stdin);
            rpc = new JsonRpc(messageHandler);

            rpc.AddLocalRpcMethod("window/show", new Action(OnWindowShow));
            rpc.AddLocalRpcMethod("window/hide", new Action(OnWindowHide));
            rpc.AddLocalRpcMethod("shutdown", new Action(OnShutdown));

            rpc.StartListening();
            rpc.Completion.Wait();
        }

        private void OnWindowShow()
        {
            app.Dispatcher.Invoke(() =>
            {
                if (dialog != null)
                {
                    dialog.Activate();
                    return;
                }

                dialog = new RegExToolDialog
                {
                    GetText = () => InvokeOnRpcSync<string>("editor/getText"),

                    SetText = text => InvokeOnRpcSync("editor/setText", new { text }),

                    SetTextInNew = text => InvokeOnRpcSync("editor/setTextInNew", new { text }),

                    SetSelectedText = text => InvokeOnRpcSync("editor/setSelectedText", new { text }),

                    GetSelectedText = () => InvokeOnRpcSync<string>("editor/getSelectedText"),

                    SetPosition = (index, length) => InvokeOnRpcSync("editor/setPosition", new { index, length }),

                    SetSelection = (index, length) => InvokeOnRpcSync("editor/setSelection", new { index, length }),

                    GetSelectionStartIndex = () => InvokeOnRpcSync<int>("editor/getSelectionStartIndex"),

                    GetSelectionLength = () => InvokeOnRpcSync<int>("editor/getSelectionLength"),

                    SaveCurrentDocument = () => InvokeOnRpcSync("editor/saveCurrentDocument"),

                    SetCurrentTabInCSharpHighlighting = () => InvokeOnRpcSync("editor/setCSharpHighlighting"),

                    TryOpen = (fileName, onlyIfAlreadyOpen) =>
                        InvokeOnRpcSync<bool>("editor/tryOpen", new { fileName, onlyIfAlreadyOpen }),

                    GetCurrentFileName = () => InvokeOnRpcSync<string>("editor/getCurrentFileName")
                };

                dialog.Closed += (s, e) => dialog = null;
                dialog.Show();
            });
        }

        private void OnWindowHide()
        {
            app.Dispatcher.Invoke(() =>
            {
                dialog?.Hide();
            });
        }

        private void OnShutdown()
        {
            app.Dispatcher.Invoke(() =>
            {
                dialog?.Close();
                app.Shutdown();
            });
        }

        /// <summary>
        /// Invoke a JSON-RPC request from the WPF UI thread without deadlocking.
        /// JoinableTaskFactory.Run properly pumps messages and handles re-entrancy.
        /// </summary>
        private T InvokeOnRpcSync<T>(string method, object argument = null)
        {
            return jtf.Run(async () =>
            {
                if (argument != null)
                    return await rpc.InvokeWithParameterObjectAsync<T>(method, argument);
                else
                    return await rpc.InvokeAsync<T>(method);
            });
        }

        /// <summary>
        /// Invoke a JSON-RPC notification (void return) from the WPF UI thread without deadlocking.
        /// </summary>
        private void InvokeOnRpcSync(string method, object argument = null)
        {
            jtf.Run(async () =>
            {
                if (argument != null)
                    await rpc.InvokeWithParameterObjectAsync(method, argument);
                else
                    await rpc.InvokeAsync(method);
            });
        }
    }
}
