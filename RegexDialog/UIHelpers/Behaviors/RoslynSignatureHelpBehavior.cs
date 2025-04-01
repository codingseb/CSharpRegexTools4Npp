using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.CodeAnalysis;
using Mono.CSharp;
using RegexDialog.Model;
using RegexDialog.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RegexDialog.Behaviors
{
    public class RoslynSignatureHelpBehavior
    {
        private readonly TextEditor _editor;
        private readonly RoslynService _roslynService;
        private string _templateCode;
        private MethodSignatureTooltip _signatureTooltip;
        private readonly bool _isDebugMode = true;

        public RoslynSignatureHelpBehavior(TextEditor editor, string templateCode)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _templateCode = templateCode ?? throw new ArgumentNullException(nameof(templateCode));
            _roslynService = new RoslynService();

            // Attacher les événements
            _editor.TextArea.TextEntered += TextArea_TextEntered;
            _editor.TextArea.PreviewKeyDown += TextArea_KeyDown;
            _editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            LogDebug("RoslynSignatureHelpBehavior initialized");
        }

        private async void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            LogDebug($"TextEntered: '{e.Text}'");

            // Afficher l'aide à la signature lors de l'ouverture d'une parenthèse ou de la saisie d'une virgule
            if (e.Text == "(" || e.Text == ",")
            {
                await ShowSignatureHelpAsync();
            }
        }

        private async void Caret_PositionChanged(object sender, EventArgs e)
        {
            var caretOffset = _editor.CaretOffset;
            var document = _editor.Document;

            if (_signatureTooltip != null && _signatureTooltip.IsOpen)
            {
                // Vérifier si nous sommes toujours dans une liste d'arguments

                // Trouver la parenthèse fermante correspondante
                int openParenCount = 1;
                int closeParenOffset = -1;

                for (int i = _signatureTooltip.OpenParenOffset + 1; i < document.TextLength; i++)
                {
                    char c = document.GetCharAt(i);
                    if (c == '(')
                        openParenCount++;
                    else if (c == ')')
                    {
                        openParenCount--;
                        if (openParenCount == 0)
                        {
                            closeParenOffset = i;
                            break;
                        }
                    }
                }

                // Si nous avons trouvé la parenthèse fermante et que le caret est entre les parenthèses
                if (closeParenOffset != -1 &&
                    caretOffset > _signatureTooltip.OpenParenOffset &&
                    caretOffset <= closeParenOffset)
                {
                    // Mettre à jour l'index du paramètre actuel
                    await UpdateSignatureHelpAsync();
                }
                else
                {
                    // Fermer le tooltip si nous ne sommes plus dans la liste d'arguments
                    _signatureTooltip.IsOpen = false;
                    _signatureTooltip = null;
                }
            }
            else if(caretOffset > 0 && (document.Text[caretOffset - 1] == '(' || document.Text[caretOffset - 1] == ','))
            {
                if(_editor.IsInitialized)
                    await ShowSignatureHelpAsync();
            }
        }

        private async void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (_signatureTooltip == null || !_signatureTooltip.IsOpen)
                return;

            // Naviguer entre les surcharges avec les touches fléchées haut/bas
            if (e.Key == Key.Up)
            {
                _signatureTooltip.SelectPreviousOverload();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                _signatureTooltip.SelectNextOverload();
                e.Handled = true;
            }
            // Fermer l'aide à la signature avec Escape
            else if (e.Key == Key.Escape)
            {
                _signatureTooltip.IsOpen = false;
                _signatureTooltip = null;
                e.Handled = true;
            }
            // Mettre à jour la mise en évidence des paramètres lors de la navigation dans l'appel de méthode
            else if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Back || e.Key == Key.Delete)
            {
                await Task.Delay(10); // Petit délai pour laisser l'éditeur se mettre à jour
                await UpdateSignatureHelpAsync();
            }
        }

        private async Task ShowSignatureHelpAsync()
        {
            try
            {
                LogDebug("ShowSignatureHelpAsync called");

                // Obtenir les éléments d'aide à la signature de Roslyn
                var signatureHelpItems = await _roslynService.GetSignatureHelpItemsAsync(
                    _editor.Text,
                    _editor.CaretOffset,
                    _templateCode);

                if (signatureHelpItems == null || !signatureHelpItems.Any())
                {
                    LogDebug("No signature help items returned from Roslyn");
                    return;
                }

                LogDebug($"Received {signatureHelpItems.Count()} signature help items from Roslyn");

                if (_signatureTooltip != null)
                {
                    _signatureTooltip.IsOpen = false;
                }

                // Trouver la position de la parenthèse ouvrante
                int openParenOffset = _editor.CaretOffset - 1;
                while (openParenOffset >= 0)
                {
                    if (_editor.Document.GetCharAt(openParenOffset) == '(')
                        break;
                    openParenOffset--;
                }

                if (openParenOffset < 0)
                {
                    LogDebug("Could not find opening parenthesis");
                    return;
                }

                // Créer et afficher le tooltip
                _signatureTooltip = new MethodSignatureTooltip(_editor)
                {
                    OpenParenOffset = openParenOffset
                };

                _signatureTooltip.SetSignatureHelpItems(signatureHelpItems);

                // Positionner le tooltip près de la parenthèse ouvrante
                var textView = _editor.TextArea.TextView;
                var visualPos = textView.GetVisualPosition(
                    new TextViewPosition(_editor.Document.GetLocation(openParenOffset)),
                    VisualYPosition.LineTop);

                _signatureTooltip.HorizontalOffset = visualPos.X;
                _signatureTooltip.IsOpen = true;
                _signatureTooltip.VerticalOffset = visualPos.Y + textView.DefaultLineHeight;

                LogDebug("Signature help tooltip should be visible now");
            }
            catch (Exception ex)
            {
                LogDebug($"Error in ShowSignatureHelpAsync: {ex.Message}");
                MessageBox.Show($"Error in ShowSignatureHelpAsync: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateSignatureHelpAsync()
        {
            if (_signatureTooltip == null || !_signatureTooltip.IsOpen)
                return;

            try
            {
                // Obtenir l'aide à la signature mise à jour pour actualiser la mise en évidence des paramètres
                var signatureHelpItems = await _roslynService.GetSignatureHelpItemsAsync(
                    _editor.Text,
                    _editor.CaretOffset,
                    _templateCode);

                if (signatureHelpItems == null || !signatureHelpItems.Any())
                {
                    _signatureTooltip.IsOpen = false;
                    _signatureTooltip = null;
                    return;
                }

                // Mettre à jour le tooltip avec les nouvelles données
                _signatureTooltip.UpdateSignatureHelp(signatureHelpItems);
            }
            catch (Exception ex)
            {
                LogDebug($"Error in UpdateSignatureHelpAsync: {ex.Message}");
            }
        }

        private void LogDebug(string message)
        {
            if (_isDebugMode)
            {
                Debug.WriteLine($"[RoslynSignatureHelp] {message}");
                Console.WriteLine($"[RoslynSignatureHelp] {message}");
                Trace.WriteLine($"[RoslynSignatureHelp] {message}");
            }
        }

        public void UpdateTemplateCode(string templateCode)
        {
            _templateCode = templateCode ?? throw new ArgumentNullException(nameof(templateCode));
        }

        public void Detach()
        {
            LogDebug("Detaching behavior");
            _editor.TextArea.TextEntered -= TextArea_TextEntered;
            _editor.TextArea.KeyDown -= TextArea_KeyDown;
            _editor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;

            if (_signatureTooltip != null)
            {
                _signatureTooltip.IsOpen = false;
                _signatureTooltip = null;
            }
        }
    }

    // Tooltip pour afficher les signatures de méthode et les paramètres
    public class MethodSignatureTooltip : ToolTip
    {
        private readonly TextEditor _editor;
        private readonly StackPanel _mainPanel;
        private readonly TextBlock _signatureText;
        private readonly TextBlock _documentationText;
        private readonly TextBlock _navigationHint;
        private List<SignatureHelpItem> _signatureHelpItems;
        private int _selectedOverloadIndex = 0;
        private int _currentParameterIndex = 0;

        public int OpenParenOffset { get; set; }

        public MethodSignatureTooltip(TextEditor editor)
        {
            _editor = editor;

            // Créer les éléments d'interface utilisateur
            _mainPanel = new StackPanel
            {
                Margin = new Thickness(5)
            };

            // Texte de la signature
            _signatureText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5)
            };
            _mainPanel.Children.Add(_signatureText);

            // Documentation
            _documentationText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5),
                MaxWidth = 400
            };
            _mainPanel.Children.Add(_documentationText);

            // Indice de navigation
            _navigationHint = new TextBlock
            {
                Text = "Use ↑↓ to navigate betweeen overloads",
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                FontSize = 10
            };
            _mainPanel.Children.Add(_navigationHint);

            // Configurer le tooltip
            Content = _mainPanel;
            this.PlacementTarget = editor;
            Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            StaysOpen = true;
            IsOpen = false;
        }

        public void SetSignatureHelpItems(IEnumerable<SignatureHelpItem> items)
        {
            _signatureHelpItems = [.. items];
            _selectedOverloadIndex = 0;
            UpdateUI();
        }

        public void UpdateSignatureHelp(IEnumerable<SignatureHelpItem> items)
        {
            var newItems = items.ToList();

            // Essayer de garder la même surcharge sélectionnée si possible
            var currentSignature = _selectedOverloadIndex < _signatureHelpItems.Count
                ? _signatureHelpItems[_selectedOverloadIndex]
                : null;

            _signatureHelpItems = newItems;

            if (currentSignature != null)
            {
                // Trouver la même signature dans la nouvelle liste
                for (int i = 0; i < _signatureHelpItems.Count; i++)
                {
                    if (_signatureHelpItems[i].ToString() == currentSignature.ToString())
                    {
                        _selectedOverloadIndex = i;
                        break;
                    }
                }
            }

            // Mettre à jour l'index du paramètre à partir de la nouvelle aide à la signature
            if (_signatureHelpItems.Count > 0 && _selectedOverloadIndex < _signatureHelpItems.Count)
            {
                _currentParameterIndex = _signatureHelpItems[_selectedOverloadIndex].ArgumentIndex;
            }

            UpdateUI();
        }

        public void SelectNextOverload()
        {
            if (_signatureHelpItems.Count > 0)
            {
                _selectedOverloadIndex = (_selectedOverloadIndex + 1) % _signatureHelpItems.Count;
                UpdateUI();
            }
        }

        public void SelectPreviousOverload()
        {
            if (_signatureHelpItems.Count > 0)
            {
                _selectedOverloadIndex = (_selectedOverloadIndex - 1 + _signatureHelpItems.Count) % _signatureHelpItems.Count;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            _signatureText.Inlines.Clear();

            if (_signatureHelpItems == null || _signatureHelpItems.Count == 0)
                return;

            // Obtenir la signature actuelle
            var currentSignature = _signatureHelpItems[_selectedOverloadIndex];
            _currentParameterIndex = currentSignature.ArgumentIndex;

            // Afficher le nombre de surcharges
            if (_signatureHelpItems.Count > 1)
            {
                _signatureText.Inlines.Add(new Run($"({_selectedOverloadIndex + 1} sur {_signatureHelpItems.Count}) ")
                {
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Gray
                });
            }

            // Formater la signature avec mise en évidence des paramètres
            _signatureText.Inlines.Add(new Run(currentSignature.PrefixDisplayParts));

            for (int i = 0; i < currentSignature.Parameters.Count; i++)
            {
                var param = currentSignature.Parameters[i];

                if (i > 0)
                {
                    _signatureText.Inlines.Add(new Run(currentSignature.SeparatorDisplayParts));
                }

                // Mettre en évidence le paramètre actuel
                var paramRun = new Run(param.DisplayParts);

                if (i == _currentParameterIndex)
                {
                    paramRun.FontWeight = FontWeights.Bold;
                    paramRun.Background = new SolidColorBrush(Color.FromArgb(50, 100, 100, 255));
                }

                _signatureText.Inlines.Add(paramRun);
            }

            _signatureText.Inlines.Add(new Run(currentSignature.SuffixDisplayParts));

            // Mettre à jour la documentation
            if (_currentParameterIndex >= 0 && _currentParameterIndex < currentSignature.Parameters.Count)
            {
                var param = currentSignature.Parameters[_currentParameterIndex];
                _documentationText.Text = $"{param.Name}: {param.Documentation}";
            }
            else
            {
                _documentationText.Text = currentSignature.Documentation;
            }

            // Afficher l'indice de navigation uniquement s'il y a plusieurs surcharges
            _navigationHint.Visibility = _signatureHelpItems.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}