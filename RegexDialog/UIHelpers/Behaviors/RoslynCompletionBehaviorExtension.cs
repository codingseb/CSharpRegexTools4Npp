using ICSharpCode.AvalonEdit;
using System.Collections.Generic;
using System.Windows;

namespace RegexDialog.Behaviors
{
    // Attached behavior pour l'utilisation en XAML
    public static class RoslynCompletionBehaviorExtension
    {
        private static readonly Dictionary<TextEditor, RoslynCompletionBehavior> _behaviors = [];

        private static readonly Dictionary<TextEditor, RoslynSignatureHelpBehavior> _signatureHelpBehaviors = [];

        public static RoslynCompletionBehavior GetBehaviorForEditor(TextEditor editor) => _behaviors.TryGetValue(editor, out RoslynCompletionBehavior behavior) ? behavior : null;

        public static RoslynSignatureHelpBehavior GetSignatureHelpBehaviorForEditor(TextEditor editor) => _signatureHelpBehaviors.TryGetValue(editor, out RoslynSignatureHelpBehavior behavior) ? behavior : null;

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
            if (d is not TextEditor editor)
            {
                return;
            }

            var enableCompletion = (bool)e.NewValue;
            var templateCode = GetTemplateCode(editor);

            if (enableCompletion)
            {
                if (!_behaviors.ContainsKey(editor))
                {
                    var behavior = new RoslynCompletionBehavior(editor, templateCode);
                    _behaviors[editor] = behavior;
                }

                if (!_signatureHelpBehaviors.ContainsKey(editor))
                {
                    var signatureHelpBehavior = new RoslynSignatureHelpBehavior(editor, templateCode);
                    _signatureHelpBehaviors[editor] = signatureHelpBehavior;
                }
            }
            else
            {
                if (_behaviors.TryGetValue(editor, out var behavior))
                {
                    behavior.Detach();
                    _behaviors.Remove(editor);
                }

                if (_signatureHelpBehaviors.TryGetValue(editor, out var signatureHelpBehavior))
                {
                    signatureHelpBehavior.Detach();
                    _signatureHelpBehaviors.Remove(editor);
                }
            }
        }

        private static void OnTemplateCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextEditor editor)
            {
                return;
            }

            if (_behaviors.TryGetValue(editor, out var behavior))
            {
                behavior.UpdateTemplateCode((string)e.NewValue);
            }

            if (_signatureHelpBehaviors.TryGetValue(editor, out var signatureHelpBehavior))
            {
                signatureHelpBehavior.UpdateTemplateCode((string)e.NewValue);
            }
        }
    }
}