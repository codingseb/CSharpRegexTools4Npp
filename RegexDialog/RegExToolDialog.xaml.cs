using CSScriptLibrary;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace RegexDialog
{
    /// <summary>
    /// Logique d'interaction pour RegExToolDialog.xaml
    /// </summary>
    public partial class RegExToolDialog : Window
    {
        private readonly List<RegExOptionViewModel> regExOptionViewModelsList = new List<RegExOptionViewModel>();

        private readonly List<Regex> bracketsRegexList = (new Regex[]
            {
                new Regex(@"(?<!(?<![\\])([\\]{2})*[\\])[\(\)]", RegexOptions.Compiled),
                new Regex(@"(?<!(?<![\\])([\\]{2})*[\\])[\[\]]", RegexOptions.Compiled),
                new Regex(@"(?<!(?<![\\])([\\]{2})*[\\])[{}]", RegexOptions.Compiled),
                new Regex(@"(?<!(?<![\\])([\\]{2})*[\\])[<>]", RegexOptions.Compiled)
            }).ToList();

        private readonly ObservableCollection<string> regexHistory = new ObservableCollection<string>();
        private readonly ObservableCollection<string> replaceHistory = new ObservableCollection<string>();

        private readonly string[] openingBrackets = new string[] { "(", "[", "{", "<" };

        private string lastMatchesText = "";
        private int lastSelectionStart;
        private int lastSelectionLength;

        private bool mustSelectEditor;

        private readonly BracketColorizer currentBracketColorizer = new BracketColorizer();
        private readonly BracketColorizer matchingBracketColorizer = new BracketColorizer();

        private static readonly Regex cSharpReplaceSpecialZoneCleaningRegex = new Regex(@"(?<=^|\s)\#(?<name>\w+)(?=\s).*(?<=\s)\#end\k<name>(?=\s|$)\s*", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex cSharpReplaceGlobalPartRegex = new Regex(@"(?<=^|\s)\#global(?=\s)(?<global>.*)(?<=\s)\#endglobal(?=\s|$)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex cSharpReplaceBeforePartRegex = new Regex(@"(?<=^|\s)\#before(?=\s)(?<before>.*)(?<=\s)\#endbefore(?=\s|$)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex cSharpReplaceAfterPartRegex = new Regex(@"(?<=^|\s)\#after(?=\s)(?<after>.*)(?<=\s)\#endafter(?=\s|$)", RegexOptions.Singleline | RegexOptions.Compiled);

        public object ReplaceScript
        {
            get
            {
                Match beforeMatch = cSharpReplaceBeforePartRegex.Match(ReplaceEditor.Text);
                Match afterMatch = cSharpReplaceAfterPartRegex.Match(ReplaceEditor.Text);

                return CSScript.Evaluator.LoadCode(Res.CSharpReplaceContainer
                    .Replace("//code", cSharpReplaceSpecialZoneCleaningRegex.Replace(ReplaceEditor.Text, string.Empty))
                    .Replace("//global", cSharpReplaceGlobalPartRegex.Match(ReplaceEditor.Text).Groups["global"].Value)
                    .Replace("//before", beforeMatch.Success ? beforeMatch.Groups["before"].Value : "return text;")
                    .Replace("//after", afterMatch.Success ? afterMatch.Groups["after"].Value : "return text;"));
            }
        }

        public delegate bool TryOpenDelegate(string fileName, bool onlyIfAlreadyOpen);
        public delegate void SetPositionDelegate(int index, int length);

        /// <summary>
        /// Fonction de récupération du texte à utiliser comme input pour l'expression régulière
        /// public delegate string GetTextDelegate()
        /// </summary>
        public Func<string> GetText { get; set; }

        /// <summary>
        /// Fonction envoyant le résultat du replace dans une chaine texte
        /// public delegate void SetTextDelegate(string text)
        /// </summary>
        public Action<string> SetText { get; set; }

        /// <summary>
        /// Fonction de récupération du texte sélectionné à utiliser comme input pour l'expression régulière
        /// public delegate string GetTextDelegate()
        /// </summary>
        public Func<string> GetSelectedText { get; set; }

        /// <summary>
        /// Fonction envoyant le résultat du replace dans une chaine texte lorsque à remplacer dans la sélection
        /// public delegate void SetTextDelegate(string text)
        /// </summary>
        public Action<string> SetSelectedText { get; set; }

        /// <summary>
        /// Fonction envoyant le résultat de l'extraction des matches
        /// public delegate void SetTextDelegate(string text)
        /// </summary>
        public Action<string> SetTextInNew { get; set; }

        /// <summary>
        /// Try to Open or show in front in the editor the specified fileName
        /// </summary>
        public TryOpenDelegate TryOpen { get; set; }

        /// <summary>
        /// Save the document in the current tab
        /// </summary>
        public Action SaveCurrentDocument { get; set; }

        /// <summary>
        /// Get the name of the current fileName in the editor
        /// </summary>
        public Func<string> GetCurrentFileName { get; set; }

        /// <summary>
        /// Fonction permettant de faire une sélection dans le text source
        /// public delegate void SetPositionDelegate(int index, int length)
        /// </summary>
        public SetPositionDelegate SetPosition { get; set; } = (_, __) => { };

        /// <summary>
        /// Fonction permettant d'ajouter une sélection de texte (La multi sélection doit être active sur le composant final)
        /// </summary>
        public SetPositionDelegate SetSelection { get; set; }

        /// <summary>
        /// Fonction qui récupère la position du début de la sélection dans le texte
        /// </summary>
        public Func<int> GetSelectionStartIndex { get; set; }

        /// <summary>
        /// Fonction qui récupère la longueur de la sélection
        /// </summary>
        public Func<int> GetSelectionLength { get; set; }

        /// <summary>
        /// L'expression régulière éditée dans la boite de dialogue
        /// </summary>
        public string RegexPatternText
        {
            get
            {
                return RegexEditor.Text;
            }

            set
            {
                RegexEditor.Text = value;
            }
        }

        /// <summary>
        /// Le text de remplacement à utiliser pour le replace de'expression régulière
        /// </summary>
        public string ReplacePatternText
        {
            get
            {
                return ReplaceEditor.Text;
            }

            set
            {
                ReplaceEditor.Text = value;
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        public RegExToolDialog()
        {
            InitializeComponent();

            Init();
        }

        /// <summary>
        /// Initialisation des propriétés des éléments GUI
        /// </summary>
        private void Init()
        {
            // Initialisation des delegates de base
            GetText = () => string.Empty;

            SetText = (string _) => { };

            SetTextInNew = (string _) => MessageBox.Show("Not Implemented");

            // Application de la coloration syntaxique pour les expressions régulières
            XmlReader reader = XmlReader.Create(new StringReader(Res.Regex_syntax_color));

            RegexEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

            // Application de la coloration syntaxique pour les chaines de remplacement
            XmlReader reader2 = XmlReader.Create(new StringReader(Res.Replace_syntax_color));

            ReplaceEditor.SyntaxHighlighting = HighlightingLoader.Load(reader2, HighlightingManager.Instance);

            // Abonnement au changement de position du curseur de texte pour la coloration des parentèses
            RegexEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            // Construit la liste des options pour les expressions régulières
            BuildRegexOptionsCheckBoxs();

            // Construit l'arbre des éléments de languages d'expression régulière.
            BuildRegexLanguageElements();

            // Construit l'arbre des éléments de languages de replace.
            BuildReplaceLanguageElements();

            // Rétablit le contenu des éditeur
            RegexEditor.Text = Config.Instance.RegexEditorText;
            ReplaceEditor.Text = Config.Instance.ReplaceEditorText;

            Left = Config.Instance.DialogLeft ?? Left;
            Top = Config.Instance.DialogTop ?? Top;
            Width = Config.Instance.DialogWidth ?? Width;
            Height = Config.Instance.DialogHeight ?? Height;

            if (Top < SystemParameters.VirtualScreenTop)
            {
                Top = SystemParameters.VirtualScreenTop;
            }

            if (Left < SystemParameters.VirtualScreenLeft)
            {
                Left = SystemParameters.VirtualScreenLeft;
            }

            if (Left + Width > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            {
                Left = SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - Width;
            }

            if (Top + Height > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                Top = SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop - Height;
            }

            WindowState = Config.Instance.DialogMaximized ? WindowState.Maximized : WindowState.Normal;

            FirstColumn.Width = Config.Instance.GridFirstColumnWidth;
            SecondColumn.Width = Config.Instance.GridSecondColumnWidth;
            ThirdColumn.Width = Config.Instance.GridThirdColumnWidth;
            RegexEditorRow.Height = Config.Instance.GridRegexEditorRowHeight;
            ReplaceEditorRow.Height = Config.Instance.GridReplaceEditorRowHeight;

            // Set Treeview Matches Result base contextMenu
            MatchResultsTreeView.ContextMenu = MatchResultsTreeView.Resources["cmMatchResultsMenu"] as ContextMenu;
        }

        private void BuildRegexOptionsCheckBoxs()
        {
            Enum.GetValues(typeof(RegexOptions))
                .Cast<RegexOptions>()
                .ToList()
                .ForEach(regexOption =>
                {
                    if (regexOption != RegexOptions.None && regexOption != RegexOptions.Compiled)
                    {
                        RegExOptionViewModel reovm = new RegExOptionViewModel
                        {
                            RegexOptions = regexOption
                        };

                        regExOptionViewModelsList.Add(reovm);
                    }
                });

            icRegexOptions.ItemsSource = regExOptionViewModelsList;
            miRegexOptions.ItemsSource = regExOptionViewModelsList;
        }

        private void BuildRegexLanguageElements()
        {
            RegexLanguageElements root = JsonConvert.DeserializeObject<RegexLanguageElements>(Res.RegexLanguageElements);
            RegexLanguagesElementsTreeView.ItemsSource = root.Data;
        }

        private void BuildReplaceLanguageElements()
        {
            ReplaceLanguageElements root = JsonConvert.DeserializeObject<ReplaceLanguageElements>(Res.ReplaceLanguageElements);
            ReplaceLanguageElementsListView.ItemsSource = root.Data;
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            try
            {
                if (RegexEditor.TextArea.TextView.LineTransformers.Contains(currentBracketColorizer))
                    RegexEditor.TextArea.TextView.LineTransformers.Remove(currentBracketColorizer);
                if (RegexEditor.TextArea.TextView.LineTransformers.Contains(matchingBracketColorizer))
                    RegexEditor.TextArea.TextView.LineTransformers.Remove(matchingBracketColorizer);

                Dictionary<string, int> posStringToMatchingPosDict = new Dictionary<string, int>();

                bracketsRegexList.ForEach(regex =>
                {
                    List<Match> matches = regex.Matches(RegexEditor.Text).Cast<Match>().ToList();
                    Stack<Match> stackMatches = new Stack<Match>();

                    matches.ForEach(match =>
                    {
                        if (openingBrackets.Contains(match.Value))
                        {
                            stackMatches.Push(match);
                        }
                        else if (stackMatches.Count > 0)
                        {
                            Match beginingMatch = stackMatches.Pop();

                            posStringToMatchingPosDict[beginingMatch.Index.ToString()] = match.Index;
                            posStringToMatchingPosDict[match.Index.ToString()] = beginingMatch.Index;
                        }
                    });
                });

                int pos = RegexEditor.TextArea.Caret.Offset;

                bool handled = false;

                if (posStringToMatchingPosDict.ContainsKey((pos - 1).ToString()))
                {
                    currentBracketColorizer.StartOffset = pos - 1;
                    currentBracketColorizer.EndOffset = pos;
                    matchingBracketColorizer.StartOffset = posStringToMatchingPosDict[(pos - 1).ToString()];
                    matchingBracketColorizer.EndOffset = posStringToMatchingPosDict[(pos - 1).ToString()] + 1;

                    handled = true;
                }
                else if (posStringToMatchingPosDict.ContainsKey((pos).ToString()))
                {
                    currentBracketColorizer.StartOffset = pos;
                    currentBracketColorizer.EndOffset = pos + 1;
                    matchingBracketColorizer.StartOffset = posStringToMatchingPosDict[(pos).ToString()];
                    matchingBracketColorizer.EndOffset = posStringToMatchingPosDict[(pos).ToString()] + 1;

                    handled = true;
                }

                if (handled)
                {
                    RegexEditor.TextArea.TextView.LineTransformers.Add(currentBracketColorizer);
                    RegexEditor.TextArea.TextView.LineTransformers.Add(matchingBracketColorizer);
                }
            }
            catch { }

            RegexEditor.TextArea.TextView.InvalidateVisual();
        }

        private void ShowMatchesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowMatches();
            }
            catch { }
        }

        /// <summary>
        /// Ajoute les champs textes courant spécifié à leur historique
        /// </summary>
        /// <param name="historyNbr">1 = seulement regex, 2 = seulement replace, 0 = tout</param>
        private void SetToHistory(int historyNbr = 0)
        {
            if (historyNbr == 0 || historyNbr == 1)
            {
                if (Config.Instance.RegexHistory.Contains(RegexEditor.Text))
                    Config.Instance.RegexHistory.Remove(RegexEditor.Text);

                if (RegexEditor.Text.Length > 0)
                    Config.Instance.RegexHistory.Insert(0, RegexEditor.Text);

                while (Config.Instance.RegexHistory.Count > Config.Instance.HistoryToKeep)
                {
                    Config.Instance.RegexHistory.RemoveAt(Config.Instance.RegexHistory.Count - 1);
                }
            }

            if (historyNbr == 0 || historyNbr == 2)
            {
                if (Config.Instance.ReplaceHistory.Contains(ReplaceEditor.Text))
                    Config.Instance.ReplaceHistory.Remove(ReplaceEditor.Text);

                if (ReplaceEditor.Text.Length > 0)
                    Config.Instance.ReplaceHistory.Insert(0, ReplaceEditor.Text);

                while (Config.Instance.ReplaceHistory.Count > Config.Instance.HistoryToKeep)
                {
                    Config.Instance.ReplaceHistory.RemoveAt(Config.Instance.ReplaceHistory.Count - 1);
                }
            }

            if (historyNbr == 0 && Directory.Exists(Config.Instance.TextSourceDirectoryPath))
            {
                string keepValue = Config.Instance.TextSourceDirectoryPath;

                if (Config.Instance.TextSourceDirectoryPathHistory.Contains(keepValue))
                    Config.Instance.TextSourceDirectoryPathHistory.Remove(keepValue);

                if (keepValue.Length > 0)
                    Config.Instance.TextSourceDirectoryPathHistory.Insert(0, keepValue);

                while (Config.Instance.TextSourceDirectoryPathHistory.Count > Config.Instance.HistoryToKeep)
                {
                    Config.Instance.TextSourceDirectoryPathHistory.RemoveAt(Config.Instance.TextSourceDirectoryPathHistory.Count - 1);
                }

                Config.Instance.TextSourceDirectoryPath = keepValue;
            }

            if (historyNbr == 0)
            {
                string keepValue = Config.Instance.TextSourceDirectorySearchFilter;

                if (Config.Instance.TextSourceDirectorySearchFilterHistory.Contains(keepValue))
                    Config.Instance.TextSourceDirectorySearchFilterHistory.Remove(keepValue);

                if (keepValue.Length > 0)
                    Config.Instance.TextSourceDirectorySearchFilterHistory.Insert(0, keepValue);

                while (Config.Instance.TextSourceDirectorySearchFilterHistory.Count > Config.Instance.HistoryToKeep)
                {
                    Config.Instance.TextSourceDirectorySearchFilterHistory.RemoveAt(Config.Instance.TextSourceDirectorySearchFilterHistory.Count - 1);
                }

                Config.Instance.TextSourceDirectorySearchFilter = keepValue;
            }

            Config.Instance.Save();
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private RegexOptions GetRegexOptions()
        {
            return regExOptionViewModelsList.FindAll(re => re.Selected)
                .Aggregate(RegexOptions.None,
                (total, next) => total |= next.RegexOptions);
        }

        private string GetCurrentText()
        {
            if (Config.Instance.TextSourceOn == RegexTextSource.CurrentSelection)
            {
                return GetSelectedText();
            }
            else
            {
                return GetText();
            }
        }

        private void ShowMatches()
        {
            try
            {
                lastSelectionStart = 0;
                lastSelectionLength = 0;

                using (Dispatcher.DisableProcessing())
                {
                    SetToHistory();

                    int i = 0;
                    int countAllCaptures = 0;

                    Regex regex = new Regex(RegexEditor.Text, GetRegexOptions());

                    List<RegexResult> GetMatchesFor(string text, string fileName = "", int selectionIndex = 0)
                    {
                        MatchCollection matches = regex.Matches(text);

                        return matches
                            .Cast<Match>()
                            .ToList()
                            .FindAll(match =>
                            {
                                countAllCaptures++;

                                return match.Length > 0 || Config.Instance.ShowEmptyMatches;
                            })
                            .ConvertAll(match =>
                            {
                                RegexResult result = new RegexMatchResult(regex, match, i, fileName, selectionIndex);

                                i++;

                                return result;
                            });
                    }

                    if (Config.Instance.TextSourceOn == RegexTextSource.Directory)
                    {
                        int ft = 0;
                        int ff = 0;

                        MatchResultsTreeView.ItemsSource = GetFiles()
                            .Select(fileName =>
                            {
                                ft++;

                                List<RegexResult> temp = GetMatchesFor(File.ReadAllText(fileName), fileName);

                                if (temp.Count > 0)
                                    ff++;

                                return new RegexFileResult(regex, null, Config.Instance.TextSourceDirectoryShowNotMatchedFiles ? ft : ff, fileName)
                                {
                                    Children = temp
                                };
                            })
                            .Where(regexFileResult => Config.Instance.TextSourceDirectoryShowNotMatchedFiles || regexFileResult.Children.Count > 0);

                        MatchesResultLabel.Content = $"{i} matches [Index,Length] + {countAllCaptures - i} empties matches found in {ff}/{ft} files";
                    }
                    else
                    {
                        lastMatchesText = GetText();

                        if (Config.Instance.TextSourceOn == RegexTextSource.CurrentSelection)
                        {
                            lastSelectionStart = GetSelectionStartIndex?.Invoke() ?? 0;
                            lastSelectionLength = GetSelectionLength?.Invoke() ?? 0;

                            MatchResultsTreeView.ItemsSource = GetMatchesFor(GetCurrentText(), selectionIndex: lastSelectionStart);
                        }
                        else
                        {
                            MatchResultsTreeView.ItemsSource = GetMatchesFor(GetCurrentText());
                        }

                        MatchesResultLabel.Content = $"{i} matches [Index,Length] + {countAllCaptures - i} empties matches";
                    }

                    if (i > 0)
                    {
                        ((RegexResult)MatchResultsTreeView.Items[0]).IsSelected = true;
                        MatchResultsTreeView.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetToHistory();

                int files = 0;
                string text;

                Regex regex = new Regex(RegexEditor.Text, GetRegexOptions());

                int nbrOfElementToReplace = 0;

                if (CSharpReplaceCheckbox.IsChecked.GetValueOrDefault())
                {
                    dynamic script = ReplaceScript;

                    int index = -1;

                    switch (Config.Instance.TextSourceOn)
                    {
                        case RegexTextSource.Directory:
                            if (!Config.Instance.OpenFilesForReplace && MessageBox.Show("This will modify files directly on the disk.\r\nModifications can not be cancel\r\nDo you want to continue ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                return;

                            GetFiles().ForEach(fileName =>
                            {
                                if (Config.Instance.OpenFilesForReplace)
                                {
                                    if (TryOpen?.Invoke(fileName, false) ?? false)
                                    {
                                        text = script.Before(GetText());
                                        int matchesCount = regex.Matches(text).Count;

                                        if (matchesCount > 0)
                                        {
                                            index = 0;

                                            SetText(script.After(regex.Replace(text, match =>
                                            {
                                                index++;
                                                nbrOfElementToReplace++;
                                                return script.Replace(match, index, fileName, nbrOfElementToReplace, files);
                                            })));

                                            try
                                            {
                                                SaveCurrentDocument?.Invoke();
                                            }
                                            catch { }

                                            files++;
                                        }
                                    }
                                    else
                                    {
                                        text = script.Before(File.ReadAllText(fileName));
                                        int matchesCount = regex.Matches(text).Count;

                                        if (matchesCount > 0)
                                        {
                                            index = 0;

                                            File.WriteAllText(fileName, script.After(regex.Replace(text, match =>
                                            {
                                                index++;
                                                nbrOfElementToReplace++;
                                                return script.Replace(match, index, fileName, nbrOfElementToReplace, files);
                                            })));

                                            files++;
                                        }
                                    }
                                }
                            });
                            break;
                        case RegexTextSource.CurrentSelection:
                            text = script.Before(GetCurrentText());
                            nbrOfElementToReplace = regex.Matches(text).Count;
                            lastSelectionStart = GetSelectionStartIndex?.Invoke() ?? 0;
                            lastSelectionLength = GetSelectionLength?.Invoke() ?? 0;
                            SetSelectedText(script.After(regex.Replace(text, match =>
                            {
                                index++;
                                return script.Replace(match, index, GetCurrentFileName?.Invoke() ?? string.Empty, index, 0);
                            })));
                            break;
                        default:
                            text = script.Before(GetCurrentText());
                            nbrOfElementToReplace = regex.Matches(text).Count;
                            SetText(script.After(regex.Replace(text, match =>
                            {
                                index++;
                                return script.Replace(match, index, GetCurrentFileName?.Invoke() ?? string.Empty, index, 0);
                            })));
                            break;
                    }
                }
                else
                {
                    switch (Config.Instance.TextSourceOn)
                    {
                        case RegexTextSource.Directory:
                            if (!Config.Instance.OpenFilesForReplace && MessageBox.Show("This will modify files directly on the disk.\r\nModifications can not be cancel\r\nDo you want to continue ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                return;

                            string replaceText = ReplaceEditor.Text;

                            GetFiles().ForEach(fileName =>
                            {
                                if (Config.Instance.OpenFilesForReplace)
                                {
                                    if (TryOpen?.Invoke(fileName, false) ?? false)
                                    {
                                        text = GetText();
                                        int matchesCount = regex.Matches(text).Count;
                                        nbrOfElementToReplace += matchesCount;
                                        if (matchesCount > 0)
                                        {
                                            SetText(regex.Replace(text, replaceText));

                                            try
                                            {
                                                SaveCurrentDocument?.Invoke();
                                            }
                                            catch { }

                                            files++;
                                        }
                                    }
                                }
                                else
                                {
                                    text = File.ReadAllText(fileName);
                                    int matchesCount = regex.Matches(text).Count;
                                    nbrOfElementToReplace += matchesCount;

                                    if (matchesCount > 0)
                                    {
                                        File.WriteAllText(fileName, regex.Replace(text, replaceText));

                                        files++;
                                    }
                                }
                            });

                            break;
                        case RegexTextSource.CurrentSelection:
                            text = GetCurrentText();
                            nbrOfElementToReplace = regex.Matches(text).Count;
                            SetSelectedText(regex.Replace(text, ReplaceEditor.Text));
                            break;
                        default:
                            text = GetCurrentText();
                            nbrOfElementToReplace = regex.Matches(text).Count;
                            SetText(regex.Replace(text, ReplaceEditor.Text));
                            break;
                    }
                }

                if (Config.Instance.TextSourceOn == RegexTextSource.Directory)
                    MessageBox.Show(nbrOfElementToReplace.ToString() + $" elements have been replaced in {files} files");
                else
                    MessageBox.Show(nbrOfElementToReplace.ToString() + " elements have been replaced");

                ShowMatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = GetCurrentText();
                Regex regex = new Regex(RegexEditor.Text, GetRegexOptions());
                List<Match> matches = regex.Matches(text)
                    .Cast<Match>()
                    .ToList();

                if (Config.Instance.TextSourceOn == RegexTextSource.CurrentSelection)
                {
                    lastSelectionStart = GetSelectionStartIndex?.Invoke() ?? 0;
                    lastSelectionLength = GetSelectionLength?.Invoke() ?? 0;
                }
                else
                {
                    lastSelectionStart = 0;
                    lastSelectionLength = 0;
                }

                if (matches.Count > 0)
                    SetPosition(matches[0].Index + lastSelectionStart, 0);
                else
                    SetPosition(0, 0);

                matches.ForEach(match =>
                {
                    try
                    {
                        SetSelection(match.Index + lastSelectionStart, match.Length);
                    }
                    catch { }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExtractMatchesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int globalIndex = 0;
                Regex regex = new Regex(RegexEditor.Text, GetRegexOptions());
                int fileIndex = 0;
                dynamic script = null;

                if (CSharpReplaceCheckbox.IsChecked.GetValueOrDefault())
                {
                    script = ReplaceScript;
                }

                void Extract(string text, string fileName = "")
                {
                    List<Match> matches = regex.Matches((string)script?.Before(text) ?? text)
                        .Cast<Match>()
                        .ToList();

                    if (matches.Count > 0 || Config.Instance.TextSourceDirectoryShowNotMatchedFiles)
                    {
                        if (Config.Instance.PrintFileNameWhenExtract)
                            sb.Append("\r\n").AppendLine(fileName);

                        if (CSharpReplaceCheckbox.IsChecked.GetValueOrDefault())
                        {
                            int index = 0;

                            matches.ForEach(match =>
                            {
                                sb.Append(script.Replace(match, index, fileName, globalIndex, fileIndex));
                                globalIndex++;
                                index++;
                            });
                        }
                        else
                        {
                            matches.ForEach(match => sb.AppendLine(match.Value));
                        }

                        fileIndex++;
                    }
                }

                if (Config.Instance.TextSourceOn == RegexTextSource.Directory)
                {
                    GetFiles().ForEach(fileName => Extract(File.ReadAllText(fileName), fileName));
                }
                else
                {
                    Extract(GetCurrentText(), GetCurrentFileName?.Invoke() ?? string.Empty);
                }

                try
                {
                    string result = sb.ToString();
                    SetTextInNew(script?.After(result) ?? result);
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private List<string> GetFiles()
        {
            string filter = Config.Instance.TextSourceDirectorySearchFilter.Trim();

            if (filter.Equals(string.Empty))
                filter = "*";

            List<string> result = new List<string>();

            filter.Split(';', ',', '|')
                .ToList()
                .ForEach(pattern => result.AddRange(Directory.GetFiles(Config.Instance.TextSourceDirectoryPath, pattern, Config.Instance.TextSourceDirectorySearchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)));

            return result;
        }

        private void IsMatchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetToHistory();
                if (Config.Instance.TextSourceOn == RegexTextSource.Directory)
                {
                    List<string> files = GetFiles();

                    bool found = false;
                    string fileName = string.Empty;

                    for (int i = 0; i < files.Count && !found; i++)
                    {
                        found = Regex.IsMatch(File.ReadAllText(files[i]), RegexEditor.Text, GetRegexOptions());

                        if (found)
                        {
                            fileName = files[i];
                            break;
                        }
                    }

                    MessageBox.Show(found ? $"Yes (Found in \"{fileName}\")" : $"No (In Any files of \"{Config.Instance.TextSourceDirectoryPath}\")");
                }
                else
                {
                    MessageBox.Show(this, Regex.IsMatch(GetCurrentText(), RegexEditor.Text, GetRegexOptions()) ? "Yes" : "No");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RegexEditor_TextChanged(object sender, EventArgs e)
        {
            Config.Instance.RegexEditorText = RegexEditor.Text;

            try
            {
                cmiReplaceGroupByNumber.Items.Clear();
                cmiReplaceGroupByName.Items.Clear();

                Regex regex = new Regex(RegexEditor.Text, GetRegexOptions());

                regex.GetGroupNames().ToList()
                    .ForEach(groupName => cmiReplaceGroupByName.Items.Add("${" + groupName + "}"));

                regex.GetGroupNumbers().ToList()
                    .ForEach(groupNumber => cmiReplaceGroupByNumber.Items.Add("$" + groupNumber.ToString()));
            }
            catch { }
            finally
            {
                cmiReplaceGroupByNumber.IsEnabled = cmiReplaceGroupByNumber.Items.Count > 0;
                cmiReplaceGroupByName.IsEnabled = cmiReplaceGroupByName.Items.Count > 0;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPosition();

            RegexEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        }

        private void ReplaceEditor_TextChanged(object sender, EventArgs e)
        {
            Config.Instance.ReplaceEditorText = ReplaceEditor.Text;
        }

        private void MatchResultsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                try
                {
                    RegexResult regexResult = e.NewValue as RegexResult;

                    if (regexResult?.FileName.Length > 0)
                    {
                        if ((TryOpen?.Invoke(regexResult.FileName, true) ?? false) && !(regexResult is RegexFileResult))
                            SetPosition(regexResult.Index, regexResult.Length);
                    }
                    else if (regexResult != null && lastMatchesText.Equals(GetText()))
                    {
                        SetPosition(regexResult.Index, regexResult.Length);
                    }
                }
                catch
                {
                    SetPosition(0, 0);
                }

                e.Handled = true;

                if (MatchResultsTreeView.SelectedValue != null)
                {
                    if (MatchResultsTreeView.SelectedValue is RegexGroupResult)
                        MatchResultsTreeView.ContextMenu = MatchResultsTreeView.Resources["cmMatchResultsGroupItemMenu"] as ContextMenu;
                    else
                        MatchResultsTreeView.ContextMenu = MatchResultsTreeView.Resources["cmMatchResultsItemMenu"] as ContextMenu;
                }
                else
                {
                    MatchResultsTreeView.ContextMenu = MatchResultsTreeView.Resources["cmMatchResultsMenu"] as ContextMenu;
                }
            }
            catch { }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is TreeViewItem treeViewItem && treeViewItem.DataContext is RegexResult regexResult)
                {
                    if (regexResult.FileName.Length > 0
                        && !GetCurrentFileName().Equals(regexResult.FileName, StringComparison.OrdinalIgnoreCase)
                        && (TryOpen?.Invoke(regexResult.FileName, false) ?? false)
                        && !(regexResult is RegexFileResult))
                    {
                        SetPosition?.Invoke(regexResult.Index, regexResult.Length);
                    }

                    e.Handled = true;
                }
            }
            catch { }
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter
                    && sender is TreeViewItem treeViewItem
                    && treeViewItem.DataContext is RegexResult regexResult
                    && regexResult.FileName.Length > 0
                    && !GetCurrentFileName().Equals(regexResult.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    if ((TryOpen?.Invoke(regexResult.FileName, false) ?? false)
                        && !(regexResult is RegexFileResult))
                    {
                        SetPosition?.Invoke(regexResult.Index, regexResult.Length);
                    }

                    e.Handled = true;
                }
            }
            catch { }
        }

        private void RegexLanguageElement_StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount >= 2 && sender is FrameworkElement)
                {
                    RegexLanguageElement rle = (RegexLanguageElement)((FrameworkElement)sender).DataContext;

                    int moveCaret = 0;

                    if (RegexEditor.SelectionLength > 0)
                    {
                        RegexEditor.Document.Remove(RegexEditor.SelectionStart, RegexEditor.SelectionLength);
                        moveCaret = rle.Value.Length;
                    }

                    RegexEditor.Document.Insert(RegexEditor.TextArea.Caret.Offset, rle.Value);

                    RegexEditor.TextArea.Caret.Offset += moveCaret;
                    RegexEditor.SelectionStart = RegexEditor.TextArea.Caret.Offset;
                    RegexEditor.SelectionLength = 0;

                    mustSelectEditor = true;

                    e.Handled = true;
                }
            }
            catch
            { }
        }

        private void RegexLanguageElement_StackPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mustSelectEditor)
            {
                RegexEditor.Focus();
                mustSelectEditor = false;
            }
        }

        private void ReplaceLanguageElement_StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount >= 2 && sender is FrameworkElement)
                {
                    ReplaceLanguageElement rle = (ReplaceLanguageElement)((FrameworkElement)sender).DataContext;

                    int moveCaret = 0;

                    if (RegexEditor.SelectionLength > 0)
                    {
                        ReplaceEditor.Document.Remove(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength);
                        moveCaret = rle.Value.Length;
                    }

                    ReplaceEditor.Document.Insert(ReplaceEditor.TextArea.Caret.Offset, rle.Value);

                    ReplaceEditor.TextArea.Caret.Offset += moveCaret;
                    ReplaceEditor.SelectionStart = ReplaceEditor.TextArea.Caret.Offset;
                    ReplaceEditor.SelectionLength = 0;

                    mustSelectEditor = true;

                    e.Handled = true;
                }
            }
            catch
            { }
        }

        private void ReplaceLanguageElement_StackPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (mustSelectEditor)
                {
                    ReplaceEditor.Focus();
                    mustSelectEditor = false;
                }
            }
            catch { }
        }

        private void RegexLanguagesElementsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (RegexLanguagesElementsTreeView.SelectedValue != null)
                {
                    if (RegexLanguagesElementsTreeView.SelectedValue is RegexLanguageElementGroup)
                        tbxRegexLanguageElementDescription.Text = ((RegexLanguageElementGroup)RegexLanguagesElementsTreeView.SelectedValue).Description;
                    if (RegexLanguagesElementsTreeView.SelectedValue is RegexLanguageElement)
                        tbxRegexLanguageElementDescription.Text = ((RegexLanguageElement)RegexLanguagesElementsTreeView.SelectedValue).Description;
                }
            }
            catch { }
        }

        private void ReplaceLanguageElementsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ReplaceLanguageElementsListView.SelectedValue != null
                    && ReplaceLanguageElementsListView.SelectedValue is ReplaceLanguageElement)
                {
                    tbxReplacLanguageElementDescription.Text = ((ReplaceLanguageElement)ReplaceLanguageElementsListView.SelectedValue).Description;
                }
            }
            catch
            { }
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                SaveWindowPosition();

                RegexHistoryPopup.IsOpen = false;
                ReplaceHistoryPopup.IsOpen = false;
            }
            catch { }
        }

        private void SaveWindowPosition()
        {
            try
            {
                Config.Instance.GridFirstColumnWidth = FirstColumn.Width;
                Config.Instance.GridSecondColumnWidth = SecondColumn.Width;
                Config.Instance.GridThirdColumnWidth = ThirdColumn.Width;
                Config.Instance.GridRegexEditorRowHeight = RegexEditorRow.Height;
                Config.Instance.GridReplaceEditorRowHeight = ReplaceEditorRow.Height;

                Config.Instance.GridRegexLanguageElementsFirstRowHeight = RegexLanguageElementFirstRow.Height;

                Config.Instance.DialogLeft = Left;
                Config.Instance.DialogTop = Top;
                Config.Instance.DialogWidth = ActualWidth;
                Config.Instance.DialogHeight = ActualHeight;

                Config.Instance.DialogMaximized = WindowState == WindowState.Maximized;

                Config.Instance.Save();
            }
            catch { }
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RegexEditor.Focus();
                RegexEditor.TextArea.Focus();
            }
            catch { }
        }

        private void RegexHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegexHistoryPopup.IsOpen = !RegexHistoryPopup.IsOpen;
                if (RegexHistoryPopup.IsOpen)
                {
                    RegexHistoryListBox.Focus();
                    if (RegexHistoryListBox.Items.Count > 0)
                        RegexHistoryListBox.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void RegexHistoryListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var focused = FocusManager.GetFocusedElement(this);

                var item = focused as ListBoxItem;
                if (focused != RegexHistoryButton && (item == null || !RegexHistoryListBox.Items.Contains(item.DataContext)))
                {
                    RegexHistoryPopup.IsOpen = false;
                }
            }
            catch { }
        }

        private void Root_LocationChanged(object sender, EventArgs e)
        {
            try
            {
                SaveWindowPosition();

                RegexHistoryPopup.IsOpen = false;
                ReplaceHistoryPopup.IsOpen = false;
            }
            catch { }
        }

        private void RegexHistoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && RegexHistoryListBox.SelectedValue != null)
                {
                    RegexEditor.Text = RegexHistoryListBox.SelectedValue.ToString();
                    RegexHistoryPopup.IsOpen = false;
                    SetToHistory(1);
                }
                else if (e.Key == Key.Escape)
                {
                    RegexHistoryPopup.IsOpen = false;
                    RegexHistoryButton.Focus();
                }
            }
            catch { }
        }

        private void RegexHistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (RegexHistoryListBox.SelectedValue != null)
                {
                    RegexEditor.Text = RegexHistoryListBox.SelectedValue.ToString();
                    RegexHistoryPopup.IsOpen = false;
                    SetToHistory(1);
                }
            }
            catch { }
        }

        private void ReplaceHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReplaceHistoryPopup.IsOpen = !ReplaceHistoryPopup.IsOpen;
                if (ReplaceHistoryPopup.IsOpen)
                {
                    ReplaceHistoryListBox.Focus();
                    if (ReplaceHistoryListBox.Items.Count > 0)
                        ReplaceHistoryListBox.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void ReplaceHistoryListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var focused = FocusManager.GetFocusedElement(this);

                var item = focused as ListBoxItem;
                if (focused != ReplaceHistoryButton && (item == null || !ReplaceHistoryListBox.Items.Contains(item.DataContext)))
                {
                    ReplaceHistoryPopup.IsOpen = false;
                }
            }
            catch { }
        }

        private void ReplaceHistoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && ReplaceHistoryListBox.SelectedValue != null)
                {
                    ReplaceEditor.Text = ReplaceHistoryListBox.SelectedValue.ToString();
                    ReplaceHistoryPopup.IsOpen = false;
                    SetToHistory(2);
                }
                else if (e.Key == Key.Escape)
                {
                    ReplaceHistoryPopup.IsOpen = false;
                    ReplaceHistoryButton.Focus();
                }
            }
            catch { }
        }

        private void ReplaceHistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ReplaceHistoryListBox.SelectedValue != null)
                {
                    ReplaceEditor.Text = ReplaceHistoryListBox.SelectedValue.ToString();
                    ReplaceHistoryPopup.IsOpen = false;
                    SetToHistory(2);
                }
            }
            catch { }
        }

        private void ShowMatchesMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.Instance.MatchesShowLevel = 1;
                Config.Instance.Save();
                RefreshMatches();
            }
            catch { }
        }

        private void ShowGroupsMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.Instance.MatchesShowLevel = 2;
                Config.Instance.Save();
                RefreshMatches();
            }
            catch { }
        }

        private void ShowCapturesMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.Instance.MatchesShowLevel = 3;
                Config.Instance.Save();
                RefreshMatches();
            }
            catch { }
        }

        private void RefreshMatches()
        {
            try
            {
                if (lastMatchesText.Equals(GetText()))
                {
                    using (Dispatcher.DisableProcessing())
                    {
                        ((List<RegexResult>)MatchResultsTreeView.ItemsSource)
                            .ForEach(regRes => regRes.RefreshExpands());
                    }
                }
                else
                {
                    ShowMatches();
                }
            }
            catch
            { }
        }

        private void ReplaceInEditor_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MatchResultsTreeView.SelectedValue is RegexFileResult regexFileResult)
                {
                    if (TryOpen.Invoke(regexFileResult.FileName, false))
                    {
                        string text = GetText();
                        Regex regex = regexFileResult.Regex;

                        int nbrOfElementToReplace = regex.Matches(text).Count;

                        if (CSharpReplaceCheckbox.IsChecked.GetValueOrDefault())
                        {
                            dynamic script = ReplaceScript;

                            int index = -1;

                            SetText(regex.Replace(text, match =>
                            {
                                index++;
                                return script.Replace(match, index, regexFileResult.FileName, index + (regexFileResult.Children.Count > 0 ? regexFileResult.Children[0].RegexElementNb : 0), regexFileResult.RegexElementNb - 1);
                            }));

                            SaveCurrentDocument?.Invoke();
                        }
                        else
                        {
                            SetText(regex.Replace(text, ReplaceEditor.Text));
                            SaveCurrentDocument?.Invoke();
                        }

                        MessageBox.Show(nbrOfElementToReplace.ToString() + " elements has been replaced");
                    }
                }
                else if (MatchResultsTreeView.SelectedValue is RegexResult regexResult)
                {
                    if (!string.IsNullOrEmpty(regexResult.FileName) && Config.Instance.OpenFilesForReplace && !(TryOpen?.Invoke(regexResult.FileName, false) ?? false))
                    {
                        return;
                    }
                    else if (!string.IsNullOrEmpty(regexResult.FileName) && !Config.Instance.OpenFilesForReplace
                        && MessageBox.Show("This will modify the file directly on the disk.\r\nModifications can not be cancel\r\nDo you want to continue ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        return;
                    }

                    string text = !string.IsNullOrEmpty(regexResult.FileName) && !Config.Instance.OpenFilesForReplace ? File.ReadAllText(regexResult.FileName) : GetText();
                    string posValue = text.Substring(regexResult.Index, regexResult.Length);

                    if (regexResult.Value.Equals(posValue))
                    {
                        string beforeMatch = text.Substring(0, regexResult.Index);
                        string afterMatch = text.Substring(regexResult.Index + regexResult.Length);
                        string newText = text;

                        if (CSharpReplaceCheckbox.IsChecked.GetValueOrDefault())
                        {
                            dynamic script = ReplaceScript;

                            if (regexResult is RegexMatchResult regexMatchResult)
                                newText = beforeMatch + script.Replace((Match)regexMatchResult.RegexElement, regexMatchResult.RegexElementNb, regexResult.FileName, regexMatchResult.RegexElementNb, 0) + afterMatch;
                            else if (regexResult is RegexGroupResult regexGroupResult)
                                newText = beforeMatch + script.Replace((Match)regexGroupResult.Parent.RegexElement, (Group)regexGroupResult.RegexElement, regexResult.RegexElementNb, regexResult.FileName, regexResult.RegexElementNb, 0) + afterMatch;
                            else if (regexResult is RegexCaptureResult regexCaptureResult)
                                newText = beforeMatch + script.Replace((Match)regexCaptureResult.Parent.Parent.RegexElement, (Group)regexCaptureResult.Parent.RegexElement, (Capture)regexCaptureResult.RegexElement, regexResult.RegexElementNb, regexResult.FileName, regexResult.RegexElementNb, 0) + afterMatch;
                        }
                        else
                        {
                            Match superMatch = (regexResult.RegexElement as Match) ?? (regexResult.Parent?.RegexElement as Match) ?? (regexResult.Parent?.Parent?.RegexElement as Match);
                            string replaceText = Regex.Replace(ReplaceEditor.Text,
                                @"[$]((?<dollar>[$])|(?<number>\d+)|[{](?<name>[a-zA-Z][a-zA-Z0-9]+)[}]|(?<and>[&])|(?<before>[`])|(?<after>['])|(?<last>[+])|(?<all>[_]))",
                                match =>
                                {
                                    if (match.Groups["dollar"].Success)
                                        return "$";
                                    else if (match.Groups["number"].Success)
                                        return superMatch.Groups[int.Parse(match.Groups["number"].Value)].Value;
                                    else if (match.Groups["name"].Success)
                                        return superMatch.Groups[match.Groups["name"].Value].Value;
                                    else if (match.Groups["and"].Success)
                                        return superMatch.Value;
                                    else if (match.Groups["before"].Success)
                                        return beforeMatch;
                                    else if (match.Groups["after"].Success)
                                        return afterMatch;
                                    else if (match.Groups["last"].Success)
                                        return superMatch.Groups[superMatch.Groups.Count - 1].Value;
                                    else if (match.Groups["all"].Success)
                                        return text;
                                    else
                                        return match.Value;
                                });

                            newText = beforeMatch + replaceText + afterMatch;
                        }

                        if (!string.IsNullOrEmpty(regexResult.FileName) && !Config.Instance.OpenFilesForReplace)
                        {
                            File.WriteAllText(regexResult.FileName, newText);
                        }
                        else
                        {
                            SetText(newText);

                            SaveCurrentDocument?.Invoke();

                            SetPosition(regexResult.Index, ReplaceEditor.Text.Length);
                        }

                        ShowMatches();
                    }
                    else
                    {
                        MessageBox.Show("Text changed since last matches search.\nReload the search before replace", "Text changed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void InsertValueInReplaceField_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MatchResultsTreeView.SelectedValue is RegexFileResult regexfileResult)
                {
                    ReplaceEditor.Document.Replace(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength, regexfileResult.FileName);
                }
                else if (MatchResultsTreeView.SelectedValue is RegexResult regexResult)
                {
                    ReplaceEditor.Document.Replace(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength, regexResult.Value);
                }
            }
            catch { }
        }

        private void InsertGroupByNumberInReplaceField_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MatchResultsTreeView.SelectedValue is RegexGroupResult regexGroupResult)
                {
                    ReplaceEditor.Document.Replace(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength, "$" + regexGroupResult.RegexElementNb.ToString());
                }
            }
            catch { }
        }

        private void InsertGroupByNameInReplaceField_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MatchResultsTreeView.SelectedValue is RegexGroupResult regexGroupResult)
                {
                    ReplaceEditor.Document.Replace(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength, "${" + regexGroupResult.GroupName + "}");
                }
            }
            catch { }
        }

        private void InsertInReplaceFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReplaceEditor.Document.Replace(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength, ((MenuItem)e.OriginalSource).Header.ToString());
            }
            catch { }
        }

        private void PutInRegexHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetToHistory(1);
            }
            catch { }
        }

        private void PutInReplaceHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetToHistory(2);
            }
            catch { }
        }

        private void New_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetToHistory();

                RegexEditor.Text = string.Empty;
                ReplaceEditor.Text = string.Empty;

                regExOptionViewModelsList.ForEach(optionModel => optionModel.Selected = false);
            }
            catch { }
        }

        private void Open_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenRegex();
            }
            catch { }
        }

        private void OpenRegex()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Title = "Open a Regex",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "Regex files|*.regex",
                    FilterIndex = 0
                };

                bool? result = dialog.ShowDialog(this);

                if (result == true && File.Exists(dialog.FileName))
                {
                    try
                    {
                        SetToHistory();

                        XmlDocument xmlDoc = new XmlDocument();
                        string content = File.ReadAllText(dialog.FileName);
                        xmlDoc.LoadXml(content);

                        XmlElement root = xmlDoc.DocumentElement;

                        CSharpReplaceCheckbox.IsChecked = content.Contains("<!--ReplaceIsCSharp-->");
                        RegexEditor.Text = root.SelectSingleNode("//FindPattern").InnerText;
                        ReplaceEditor.Text = root.SelectSingleNode("//ReplacePattern").InnerText;

                        string[] xOptions = root.SelectSingleNode("//Options").InnerText.Split(' ');

                        regExOptionViewModelsList.ForEach((RegExOptionViewModel optionModel) => optionModel.Selected = xOptions.Contains(optionModel.Name));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch { }
        }

        private void Save_as_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    DefaultExt = "regex",
                    Filter = "Regex files|*.regex",
                    FilterIndex = 0
                };

                bool? result = dialog.ShowDialog(this);

                if (result == true)
                {
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();

                        XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", null, null);

                        XmlElement root = xmlDoc.CreateElement("SavedRegex");
                        xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
                        xmlDoc.AppendChild(root);

                        xmlDoc.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        xmlDoc.DocumentElement.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");

                        XmlElement findPatternElement = xmlDoc.CreateElement("FindPattern");
                        XmlElement replacePatternElement = xmlDoc.CreateElement("ReplacePattern");
                        XmlElement optionsElement = xmlDoc.CreateElement("Options");

                        root.AppendChild(findPatternElement);
                        if (CSharpReplaceCheckbox.IsChecked ?? false)
                            root.AppendChild(xmlDoc.CreateComment("ReplaceIsCSharp"));
                        root.AppendChild(replacePatternElement);
                        root.AppendChild(optionsElement);

                        XmlText findPatternText = xmlDoc.CreateTextNode(RegexEditor.Text);
                        XmlText replacePatternText = xmlDoc.CreateTextNode(ReplaceEditor.Text);

                        string sOptionsText = regExOptionViewModelsList
                            .Aggregate("", (total, next) => total + (next.Selected ? next.Name + " " : ""))
                            .Trim();

                        XmlText optionsText = xmlDoc.CreateTextNode(sOptionsText);

                        findPatternElement.AppendChild(findPatternText);
                        replacePatternElement.AppendChild(replacePatternText);
                        optionsElement.AppendChild(optionsText);

                        xmlDoc.Save(dialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch { }
        }

        private void Exit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch { }
        }

        private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                    e.Handled = true;
                }
                else if (e.Key == Key.F5)
                {
                    ShowMatches();
                    e.Handled = true;
                }
            }
            catch { }
        }

        private void CmiRegexCopyForOnOneLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(RegexPatternIndenter.SetOnOneLine(RegexEditor.SelectionLength > 0 ? RegexEditor.SelectedText : RegexEditor.Text));
            }
            catch { }
        }

        private void CmiRegexCopyForXml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText((RegexEditor.SelectionLength > 0 ? RegexEditor.SelectedText : RegexEditor.Text).EscapeXml());
            }
            catch { }
        }

        private void CmiRegexPasteFromXml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegexEditor.SelectedText = Clipboard.GetText().UnescapeXml();
            }
            catch { }
        }

        private void CmiRegexCut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegexEditor.Cut();
            }
            catch { }
        }

        private void CmiRegexCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegexEditor.Copy();
            }
            catch { }
        }

        private void CmiRegexPaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegexEditor.Paste();
            }
            catch { }
        }

        private void CmiReplaceCut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReplaceEditor.Cut();
            }
            catch { }
        }

        private void CmiReplaceCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReplaceEditor.Copy();
            }
            catch { }
        }

        private void CmiReplacePaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReplaceEditor.Paste();
            }
            catch { }
        }

        private void CmiRegexSelectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegexEditor.SelectAll();
            }
            catch { }
        }

        private void CmiReplaceSelectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReplaceEditor.SelectAll();
            }
            catch { }
        }

        private void CmiRegexIndent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetToHistory(1);

                if (RegexEditor.SelectionLength > 0)
                {
                    RegexEditor.SelectedText = IndentRegexPattern(RegexEditor.SelectedText);
                }
                else
                {
                    RegexEditor.Text = IndentRegexPattern(RegexEditor.Text);
                }

                regExOptionViewModelsList.Find(vm => vm.RegexOptions == RegexOptions.IgnorePatternWhitespace).Selected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string IndentRegexPattern(string pattern)
        {
            return RegexPatternIndenter.IndentRegexPattern(pattern,
                Config.Instance.AutoIndentCharClassesOnOneLine,
                Config.Instance.AutoIndentKeepQuantifiersOnSameLine);
        }

        private void CmiRegexSetOnOneLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetToHistory(1);

                if (RegexEditor.SelectionLength > 0)
                {
                    RegexEditor.SelectedText = RegexPatternIndenter.SetOnOneLine(RegexEditor.SelectedText);
                }
                else
                {
                    RegexEditor.Text = RegexPatternIndenter.SetOnOneLine(RegexEditor.Text);

                    if (regExOptionViewModelsList.Find(vm => vm.RegexOptions == RegexOptions.IgnorePatternWhitespace).Selected
                        && MessageBox.Show("Regex Option IgnorePatternWhitespace is checked.\nUncheck it ?", "IgnorePatternWhitespace",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        regExOptionViewModelsList.Find(vm => vm.RegexOptions == RegexOptions.IgnorePatternWhitespace).Selected = false;
                    }
                }
            }
            catch { }
        }

        private void MiRegexOption_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem mi = sender as MenuItem;

                if (sender != null)
                {
                    mi.IsChecked = !mi.IsChecked;
                }
            }
            catch { }
        }

        private void ClearRegexHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("It will clear the C# Regex field history. This action is not cancelable.\nDo you want to continue?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Config.Instance.RegexHistory.Clear();
                    Config.Instance.Save();
                }
            }
            catch { }
        }

        private void ClearReplaceHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("It will clear the Replace field history. This action is not cancelable.\nDo you want to continue?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Config.Instance.ReplaceHistory.Clear();
                    Config.Instance.Save();
                }
            }
            catch { }
        }

        private void ClearDirectoryHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("It will clear the Directory field history. This action is not cancelable.\nDo you want to continue?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Config.Instance.TextSourceDirectoryPathHistory.Clear();
                    Config.Instance.Save();
                }
            }
            catch { }
        }

        private void ClearDirectoryFilterHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("It will clear the Directory filter field history. This action is not cancelable.\nDo you want to continue?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Config.Instance.TextSourceDirectorySearchFilterHistory.Clear();
                    Config.Instance.Save();
                }
            }
            catch { }
        }

        private void CSharpReplaceCheckbox_IsChecked_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CSharpReplaceCheckbox.IsChecked ?? false)
                {
                    ReplaceEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
                }
                else
                {
                    XmlReader reader2 = XmlReader.Create(new StringReader(Res.Replace_syntax_color));

                    ReplaceEditor.SyntaxHighlighting = HighlightingLoader.Load(reader2, HighlightingManager.Instance);
                }
            }
            catch { }
        }

        private void SpecifiedDirectoryTextSourcePathButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog()
                {
                    Description = "Select source folder",
                    UseDescriptionForTitle = true
                };

                if (Directory.Exists(SpecifiedDirectoryTextSourcePathComboBox.Text))
                    folderBrowserDialog.SelectedPath = SpecifiedDirectoryTextSourcePathComboBox.Text;

                if (folderBrowserDialog.ShowDialog(GetWindow(this)) ?? false)
                {
                    SpecifiedDirectoryTextSourcePathComboBox.Text = folderBrowserDialog.SelectedPath;
                }
            }
            catch { }
        }

        private void RestoreLastMachesSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetPosition?.Invoke(lastSelectionStart, lastSelectionLength);
            }
            catch { }
        }

        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
