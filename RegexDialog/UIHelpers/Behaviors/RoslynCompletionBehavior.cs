using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using RegexDialog.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RegexDialog.Behaviors
{
    public class RoslynCompletionBehavior
    {
        private readonly TextEditor _editor;
        private readonly RoslynService _roslynService;
        private CompletionWindow _completionWindow;
        private string _templateCode;

        // Pour le débogage
        private bool _isDebugMode = true;

        public RoslynCompletionBehavior(TextEditor editor, string templateCode)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _templateCode = templateCode ?? throw new ArgumentNullException(nameof(templateCode));
            _roslynService = new RoslynService();

            // Attacher les événements
            _editor.TextArea.TextEntered += TextArea_TextEntered;
            _editor.TextArea.KeyDown += TextArea_KeyDown;

            LogDebug("RoslynCompletionBehavior initialized");
        }

        private async void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            LogDebug($"KeyDown: {e.Key}, Modifiers: {Keyboard.Modifiers}");

            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                LogDebug("Ctrl+Space detected");
                e.Handled = true;
                await ShowCompletionWindowAsync();
            }
        }

        private async void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            LogDebug($"TextEntered: '{e.Text}'");

            // Si la fenêtre est déjà ouverte, laisser l'utilisateur naviguer dedans
            if (_completionWindow != null)
            {
                // Si l'utilisateur tape un caractère qui n'est pas valide pour un identifiant,
                // fermer la fenêtre de complétion
                if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_' && e.Text[0] != '.')
                {
                    LogDebug("Closing completion window due to non-identifier character");
                    _completionWindow.Close();
                }
                return;
            }

            // Déclencher l'autocomplétion dans ces cas :
            // 1. Après un point (accès à un membre)
            // 2. Lorsqu'on commence à taper un identifiant
            if (e.Text == ".")
            {
                LogDebug("Triggering completion after dot");
                await ShowCompletionWindowAsync();
            }
            else if (char.IsLetter(e.Text[0]))
            {
                LogDebug("Triggering completion for identifier");
                await ShowCompletionWindowAsync();
            }
        }

        public async Task ShowCompletionWindowAsync()
        {
            try
            {
                LogDebug("ShowCompletionWindowAsync called");

                if (_completionWindow != null)
                {
                    LogDebug("Closing existing completion window");
                    _completionWindow.Close();
                }

                // Obtenir les suggestions de complétion réelles via Roslyn
                var completionItems = await _roslynService.GetCompletionItemsAsync(
                    _editor.Text,
                    _editor.CaretOffset,
                    _templateCode);

                if (!completionItems.Any())
                {
                    LogDebug("No completion items returned from Roslyn");
                    return;
                }

                LogDebug($"Received {completionItems.Count()} completion items from Roslyn");

                _completionWindow = new CompletionWindow(_editor.TextArea);
                _completionWindow.Closed += (sender, args) =>
                {
                    LogDebug("Completion window closed");
                    _completionWindow = null;
                };

                var data = _completionWindow.CompletionList.CompletionData;
                foreach (var item in completionItems)
                {
                    data.Add(item);
                }

                LogDebug($"Added {data.Count} items to completion window");

                // Définir la position de début et de fin
                int caretOffset = _editor.CaretOffset;
                int startOffset = caretOffset;

                // Si nous sommes après un point, reculer d'un caractère
                if (caretOffset > 0 && _editor.Document.GetCharAt(caretOffset - 1) == '.')
                {
                    startOffset = caretOffset;
                }
                else
                {
                    // Sinon, trouver le début du mot actuel
                    startOffset = FindWordStart(caretOffset);
                }

                LogDebug($"Setting completion window range: {startOffset} to {caretOffset}");
                _completionWindow.StartOffset = startOffset;
                _completionWindow.EndOffset = caretOffset;

                LogDebug("Showing completion window");
                _completionWindow.Show();

                LogDebug("Completion window should be visible now");
            }
            catch (Exception ex)
            {
                LogDebug($"Error in ShowCompletionWindowAsync: {ex.Message}");
                MessageBox.Show($"Error in ShowCompletionWindowAsync: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int FindWordStart(int offset)
        {
            var document = _editor.Document;
            int wordStart = offset;

            // Reculer jusqu'à trouver un caractère qui n'est pas valide pour un identifiant
            while (wordStart > 0)
            {
                char c = document.GetCharAt(wordStart - 1);
                if (!char.IsLetterOrDigit(c) && c != '_')
                    break;
                wordStart--;
            }

            return wordStart;
        }

        private void LogDebug(string message)
        {
            if (_isDebugMode)
            {
                Debug.WriteLine($"[RoslynCompletion] {message}");
                Console.WriteLine($"[RoslynCompletion] {message}");

                // Ajouter un log visuel pour le débogage
                System.Diagnostics.Trace.WriteLine($"[RoslynCompletion] {message}");
            }
        }

        public void UpdateTemplateCode(string templateCode)
        {
            _templateCode = templateCode ?? throw new ArgumentNullException(nameof(templateCode));
        }

        public void Detach()
        {
            LogDebug("Detaching behavior");
            _editor.TextArea.KeyDown -= TextArea_KeyDown;
            _editor.TextArea.TextEntered -= TextArea_TextEntered;

            if (_completionWindow != null)
            {
                _completionWindow.Close();
                _completionWindow = null;
            }
        }
    }

    // Attached behavior pour l'utilisation en XAML
    public static class RoslynCompletionBehaviorExtension
    {
        private static readonly Dictionary<TextEditor, RoslynCompletionBehavior> _behaviors =
            new Dictionary<TextEditor, RoslynCompletionBehavior>();

        public static RoslynCompletionBehavior GetBehaviorForEditor(TextEditor editor) => _behaviors.TryGetValue(editor, out RoslynCompletionBehavior behavior) ? behavior : null;

        #region EnableCompletion Property

        public static readonly DependencyProperty EnableCompletionProperty =
            DependencyProperty.RegisterAttached(
                "EnableCompletion",
                typeof(bool),
                typeof(RoslynCompletionBehaviorExtension),
                new PropertyMetadata(false, OnEnableCompletionChanged));

        public static bool GetEnableCompletion(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableCompletionProperty);
        }

        public static void SetEnableCompletion(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableCompletionProperty, value);
        }

        #endregion

        #region TemplateCode Property

        public static readonly DependencyProperty TemplateCodeProperty =
            DependencyProperty.RegisterAttached(
                "TemplateCode",
                typeof(string),
                typeof(RoslynCompletionBehaviorExtension),
                new PropertyMetadata(string.Empty, OnTemplateCodeChanged));

        public static string GetTemplateCode(DependencyObject obj)
        {
            return (string)obj.GetValue(TemplateCodeProperty);
        }

        public static void SetTemplateCode(DependencyObject obj, string value)
        {
            obj.SetValue(TemplateCodeProperty, value);
        }

        #endregion

        private static void OnEnableCompletionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextEditor editor))
                return;

            var enableCompletion = (bool)e.NewValue;
            var templateCode = GetTemplateCode(editor);

            if (enableCompletion)
            {
                if (!_behaviors.ContainsKey(editor))
                {
                    var behavior = new RoslynCompletionBehavior(editor, templateCode);
                    _behaviors[editor] = behavior;
                }
            }
            else
            {
                if (_behaviors.TryGetValue(editor, out var behavior))
                {
                    behavior.Detach();
                    _behaviors.Remove(editor);
                }
            }
        }

        private static void OnTemplateCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextEditor editor))
                return;

            if (_behaviors.TryGetValue(editor, out var behavior))
            {
                behavior.UpdateTemplateCode((string)e.NewValue);
            }
        }
    }
}