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
        private List<RegExOptionViewModel> regExOptionViewModelsList = new List<RegExOptionViewModel>();
        List<Regex> bracketsRegexList = (new Regex[] 
            { 
                new Regex(@"(?<!(?<![\\])([\\]{2})*[\\])[\(\)]", RegexOptions.Compiled),
                new Regex(@"(?<!(?<![\\])([\\]{2})*[\\])[\[\]]", RegexOptions.Compiled),
                new Regex(@"(?<!(?<![\\])([\\]{2})*[\\])[{}]", RegexOptions.Compiled),
                new Regex(@"(?<!(?<![\\])([\\]{2})*[\\])[<>]", RegexOptions.Compiled)
            }).ToList();

        private ObservableCollection<string> regexHistory = new ObservableCollection<string>();
        private ObservableCollection<string> replaceHistory = new ObservableCollection<string>();

        private string[] openingBrackets = new string[] { "(", "[", "{", "<" };

        private string lastMatchesText = "";
        private int lastSelectionStart = 0;
        private int lastSelectionLength = 0;

        private bool mustSelectEditor = false;

        private BracketColorizer currentBracketColorizer = new BracketColorizer();
        private BracketColorizer matchingBracketColorizer = new BracketColorizer();

        public delegate string GetTextDelegate();
        public delegate string GetCurrentFileNameDelegate();
        public delegate void SetTextDelegate(string text);
        public delegate bool TryOpenDelegate(string fileName, bool onlyIfAlreadyOpen);
        public delegate void SetPositionDelegate(int index, int length);
        public delegate int GetIntDelegate();

        /// <summary>
        /// Fonction de récupération du texte à utiliser comme input pour l'expression régulière
        /// public delegate string GetTextDelegate()
        /// </summary>
        public GetTextDelegate GetText { get; set; }

        /// <summary>
        /// Fonction envoyant le résultat du replace dans une chaine texte
        /// public delegate void SetTextDelegate(string text)
        /// </summary>
        public SetTextDelegate SetText { get; set; }

        /// <summary>
        /// Fonction de récupération du texte sélectionné à utiliser comme input pour l'expression régulière
        /// public delegate string GetTextDelegate()
        /// </summary>
        public GetTextDelegate GetSelectedText { get; set; }

        /// <summary>
        /// Fonction envoyant le résultat du replace dans une chaine texte lorsque à remplacer dans la sélection
        /// public delegate void SetTextDelegate(string text)
        /// </summary>
        public SetTextDelegate SetSelectedText { get; set; }

        /// <summary>
        /// Fonction envoyant le résultat de l'extraction des matches
        /// public delegate void SetTextDelegate(string text)
        /// </summary>
        public SetTextDelegate SetTextInNew { get; set; }

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
        public GetCurrentFileNameDelegate GetCurrentFileName { get; set; }

        /// <summary>
        /// Fonction permettant de faire une sélection dans le text source
        /// public delegate void SetPositionDelegate(int index, int length)
        /// </summary>
        public SetPositionDelegate SetPosition { get; set; } = (x, y) => { };

        /// <summary>
        /// Fonction permettant d'ajouter une sélection de texte (La multi sélection doit être active sur le composant final)
        /// </summary>
        public SetPositionDelegate SetSelection { get; set; }

        /// <summary>
        /// Fonction qui récupère la position du début de la sélection dans le texte
        /// </summary>
        public GetIntDelegate GetSelectionStartIndex { get; set; }
        
        /// <summary>
        /// Fonction qui récupère la longueur de la sélection
        /// </summary>
        public GetIntDelegate GetSelectionLength { get; set; }

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
            //var localDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //CSScript.GlobalSettings.UseAlternativeCompiler = Path.Combine(localDir, "CSSRoslynProvider.dll");
            //CSScript.GlobalSettings.RoslynDir = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.nuget\packages\Microsoft.Net.Compilers\2.2.0\tools");

            // Initialisation des delegates de base
            GetText = delegate()
            { return ""; };

            SetText = delegate(string text)
            { };

            SetTextInNew = delegate(string text)
            {
                MessageBox.Show("Not Implemented");
            };

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

            WindowState = Config.Instance.DialogMaximized ? WindowState.Maximized : WindowState.Normal;

            FirstColumn.Width = new GridLength(Config.Instance.GridFirstColumnWidth ?? FirstColumn.Width.Value);
            SecondColumn.Width = new GridLength(Config.Instance.GridSecondColumnWidth ?? SecondColumn.Width.Value);
            RegexEditorRow.Height = new GridLength(Config.Instance.GridRegexEditorRowHeight ?? RegexEditorRow.Height.Value);
            RegexLanguageElementFirstRow.Height = new GridLength(Config.Instance.GridRegexLanguageElementsFirstRowHeight ?? RegexLanguageElementFirstRow.Height.Value);

            // Set Treeview Matches Result base contextMenu
            MatchResultsTreeView.ContextMenu = MatchResultsTreeView.Resources["cmMatchResultsMenu"] as ContextMenu;
        }

        private void BuildRegexOptionsCheckBoxs()
        {
            Enum.GetValues(typeof(RegexOptions))
                .Cast<RegexOptions>()
                .ToList()
                .ForEach(delegate(RegexOptions regexOption)
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

                bracketsRegexList.ForEach(delegate(Regex regex)
                {
                    List<Match> matches = regex.Matches(RegexEditor.Text).Cast<Match>().ToList();
                    Stack<Match> stackMatches = new Stack<Match>();

                    matches.ForEach(delegate(Match match)
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

                if(handled)
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
            ShowMatches();
        }

        /// <summary>
        /// 
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

                if(ReplaceEditor.Text.Length > 0)
                    Config.Instance.ReplaceHistory.Insert(0, ReplaceEditor.Text);

                while (Config.Instance.ReplaceHistory.Count > Config.Instance.HistoryToKeep)
                {
                    Config.Instance.ReplaceHistory.RemoveAt(Config.Instance.ReplaceHistory.Count - 1);
                }
            }

            if(historyNbr == 0 && Directory.Exists(Config.Instance.TextSourceDirectoryPath))
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

            if(historyNbr == 0)
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

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
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
            if(Config.Instance.TextSourceOn == RegexTextSource.CurrentSelection)
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
                            .FindAll(delegate (Match m)
                            {
                                countAllCaptures++;

                                return m.Length > 0 || Config.Instance.ShowEmptyMatches;
                            })
                            .ConvertAll(delegate (Match m)
                            {
                                RegexResult result = new RegexMatchResult(regex, m, i, fileName, selectionIndex);

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

                        MatchesResultLabel.Content = $"{i} matches [Index,Length] + {(countAllCaptures - i)} empties matches found in {ff}/{ft} files";
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

                        MatchesResultLabel.Content = $"{i} matches [Index,Length] + {(countAllCaptures - i)} empties matches";
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
                string text = GetCurrentText() ; 

                Regex regex = new Regex(RegexEditor.Text, GetRegexOptions());

                int nbrOfElementToReplace = Config.Instance.TextSourceOn == RegexTextSource.Directory ? 0 : regex.Matches(text).Count;

                if (CSharpReplaceCheckbox.IsChecked.GetValueOrDefault())
                {
                    dynamic script = CSScript.Evaluator.LoadCode(Res.CSharpReplaceContainer.Replace("//code", ReplaceEditor.Text));

                    int index = -1;

                    switch(Config.Instance.TextSourceOn)
                    {
                        case RegexTextSource.Directory:
                            if (!Config.Instance.OpenFilesForReplace && MessageBox.Show("This will modify files directly on the disk.\r\nModifications can not be cancel\r\nDo you want to continue ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                return;

                            GetFiles().ForEach(fileName =>
                            {
                                if (Config.Instance.OpenFilesForReplace)
                                {
                                    if(TryOpen?.Invoke(fileName, false) ?? false)
                                    {
                                        text = GetText();
                                        int matchesCount = regex.Matches(text).Count;

                                        if (matchesCount > 0)
                                        {
                                            index = 0;

                                            SetText(regex.Replace(text, match =>
                                            {
                                                index++;
                                                nbrOfElementToReplace++;
                                                return script.Replace(match, index, fileName, nbrOfElementToReplace, files);
                                            }));

                                            try
                                            {
                                                SaveCurrentDocument?.Invoke();
                                            }
                                            catch {}

                                            files++;
                                        }
                                    }
                                    else
                                    {
                                        text = File.ReadAllText(fileName);
                                        int matchesCount = regex.Matches(text).Count;

                                        if (matchesCount > 0)
                                        {
                                            index = 0;

                                            File.WriteAllText(fileName, regex.Replace(text, match =>
                                            {
                                                index++;
                                                nbrOfElementToReplace++;
                                                return script.Replace(match, index, fileName, nbrOfElementToReplace, files);
                                            }));

                                            files++;
                                        }
                                    }
                                }

                            });
                            break;
                        case RegexTextSource.CurrentSelection:
                            lastSelectionStart = GetSelectionStartIndex?.Invoke() ?? 0;
                            lastSelectionLength = GetSelectionLength?.Invoke() ?? 0;
                            SetSelectedText(regex.Replace(text, match =>
                            {
                                index++;
                                return script.Replace(match, index, GetCurrentFileName?.Invoke() ?? string.Empty, index, 0);
                            }));
                            break;
                        default:
                            SetText(regex.Replace(text, match =>
                            {
                                index++;
                                return script.Replace(match, index, GetCurrentFileName?.Invoke() ?? string.Empty, index, 0);
                            }));
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
                            SetSelectedText(regex.Replace(text, ReplaceEditor.Text));
                            break;
                        default:
                            SetText(regex.Replace(text, ReplaceEditor.Text));
                            break;
                    }
                }

                if(Config.Instance.TextSourceOn == RegexTextSource.Directory)
                    MessageBox.Show(nbrOfElementToReplace.ToString() + $" elements have been replaced in {files} files");
                else
                    MessageBox.Show(nbrOfElementToReplace.ToString() + " elements have been replaced");


                ShowMatches();
            }
            catch(Exception ex)
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


                matches.ForEach(delegate(Match match)
                {
                    try
                    {
                        SetSelection(match.Index + lastSelectionStart, match.Length);
                    }
                    catch { }
                });
            }
            catch(Exception ex)
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
                    script = CSScript.Evaluator.LoadCode(Res.CSharpReplaceContainer.Replace("//code", ReplaceEditor.Text));

                    void Extract(string text, string fileName = "")
                {
                    List<Match> matches = regex.Matches(text)
                        .Cast<Match>()
                        .ToList();

                    if (matches.Count > 0 || Config.Instance.TextSourceDirectoryShowNotMatchedFiles)
                    {
                        if (Config.Instance.PrintFileNameWhenExtract)
                            sb.AppendLine("\r\n" + fileName);

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

                if(Config.Instance.TextSourceOn == RegexTextSource.Directory)
                {
                    GetFiles().ForEach(fileName =>
                    {
                        Extract(File.ReadAllText(fileName), fileName);
                    });
                }
                else
                {
                    Extract(GetCurrentText(), GetCurrentFileName?.Invoke() ?? string.Empty);
                }

                try
                {
                    SetTextInNew(sb.ToString());
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
                if(Config.Instance.TextSourceOn == RegexTextSource.Directory)
                {
                    List<string> files = GetFiles();

                    bool found = false;
                    string fileName = string.Empty;

                    for(int i = 0; i < files.Count && !found; i++)
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
            catch(Exception ex)
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
                    .ForEach(delegate(string groupName)
                        {
                            cmiReplaceGroupByName.Items.Add("${" + groupName + "}");
                        });

                regex.GetGroupNumbers().ToList()
                    .ForEach(delegate(int groupNumber)
                    {
                        cmiReplaceGroupByNumber.Items.Add("$" + groupNumber.ToString());    
                    });
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
                RegexResult regexResult = e.NewValue as RegexResult;

                if(regexResult != null && regexResult.FileName.Length > 0)
                {
                    if((TryOpen?.Invoke(regexResult.FileName, true) ?? false) && !(regexResult is RegexFileResult))
                        SetPosition(regexResult.Index, regexResult.Length);
                }
                else if (regexResult != null && lastMatchesText.Equals(GetText()))
                {
                    SetPosition(regexResult.Index, regexResult.Length);
                }
            }
            catch
            {
                SetPosition(0,0);
            }

            if(MatchResultsTreeView.SelectedValue != null)
            {
                if(MatchResultsTreeView.SelectedValue is RegexGroupResult)
                    MatchResultsTreeView.ContextMenu = MatchResultsTreeView.Resources["cmMatchResultsGroupItemMenu"] as ContextMenu;
                else
                    MatchResultsTreeView.ContextMenu = MatchResultsTreeView.Resources["cmMatchResultsItemMenu"] as ContextMenu;
            }
            else
            {
                MatchResultsTreeView.ContextMenu = MatchResultsTreeView.Resources["cmMatchResultsMenu"] as ContextMenu;
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && treeViewItem.DataContext is RegexResult regexResult)
            {
                if (regexResult.FileName.Length > 0 && !GetCurrentFileName().ToLower().Equals(regexResult.FileName.ToLower()))
                {
                    if (TryOpen?.Invoke(regexResult.FileName, false) ?? false)
                    {
                        if(!(regexResult is RegexFileResult))
                            SetPosition(regexResult.Index, regexResult.Length);
                    }
                }

                e.Handled = true;
            }
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TreeViewItem treeViewItem && treeViewItem.DataContext is RegexResult regexResult)
            {
                if (regexResult.FileName.Length > 0 && !GetCurrentFileName().ToLower().Equals(regexResult.FileName.ToLower()))
                {
                    if (TryOpen?.Invoke(regexResult.FileName, false) ?? false)
                    {
                        if (!(regexResult is RegexFileResult))
                            SetPosition(regexResult.Index, regexResult.Length);
                    }

                    e.Handled = true;
                }
            }
        }

        private void RegexLanguageElement_StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && e.ClickCount >= 2 && sender is FrameworkElement)
            {
                try
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
                catch
                {}
            }
        }

        private void RegexLanguageElement_StackPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(mustSelectEditor)
            {
                RegexEditor.Focus();
                mustSelectEditor = false;
            }
        }

        private void ReplaceLanguageElement_StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount >= 2 && sender is FrameworkElement)
            {
                try
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
                catch
                { }
            }

        }

        private void ReplaceLanguageElement_StackPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mustSelectEditor)
            {
                ReplaceEditor.Focus();
                mustSelectEditor = false;
            }
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
                if (ReplaceLanguageElementsListView.SelectedValue != null)
                {
                    if (ReplaceLanguageElementsListView.SelectedValue is ReplaceLanguageElement)
                        tbxReplacLanguageElementDescription.Text = ((ReplaceLanguageElement)ReplaceLanguageElementsListView.SelectedValue).Description;
                }
            }
            catch
            { }
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowPosition();

            RegexHistoryPopup.IsOpen = false;
            ReplaceHistoryPopup.IsOpen = false;
            SetMaxSizes();
        }

        private void SaveWindowPosition()
        {
            Config.Instance.GridFirstColumnWidth = FirstColumn.ActualWidth;
            Config.Instance.GridSecondColumnWidth = SecondColumn.ActualWidth;
            Config.Instance.GridRegexEditorRowHeight = RegexEditorRow.ActualHeight;
            Config.Instance.GridRegexLanguageElementsFirstRowHeight = RegexLanguageElementFirstRow.ActualHeight;

            Config.Instance.DialogLeft = this.Left;
            Config.Instance.DialogTop = this.Top;
            Config.Instance.DialogWidth = this.ActualWidth;
            Config.Instance.DialogHeight = this.ActualHeight;

            Config.Instance.DialogMaximized = this.WindowState == WindowState.Maximized;

            Config.Instance.Save();
        }

        private void SetMaxSizes()
        {
            RegexEditorRow.MaxHeight = Root.ActualHeight - RegexEditor.TransformToAncestor(Root).Transform(new Point(0,0)).Y - 5 - 10;
            if(OptionTabControl.SelectedItem.Equals(RegexTabItem))
                RegexLanguageElementFirstRow.MaxHeight = Root.ActualHeight - RegexLanguagesElementsTreeView.TransformToAncestor(Root).Transform(new Point(0, 0)).Y - 5 - 40;
            if (OptionTabControl.SelectedItem.Equals(ReplaceTabItem))
                ReplaceLanguageElementFirstRow.MaxHeight = Root.ActualHeight - ReplaceLanguageElementsListView.TransformToAncestor(Root).Transform(new Point(0, 0)).Y - 5 - 40;
            FirstColumn.MaxWidth = Math.Max(Root.ActualWidth - 10 - 100, 0);
            SecondColumn.MaxWidth = Math.Max(Root.ActualWidth - Math.Min(FirstColumn.ActualWidth, Root.ActualWidth) - 10 - 40, 0);
        }

        private void RegexEditor_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMaxSizes();    
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            RegexEditor.Focus();
            RegexEditor.TextArea.Focus();
        }

        private void RegexHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            RegexHistoryPopup.IsOpen = !RegexHistoryPopup.IsOpen;
            if (RegexHistoryPopup.IsOpen)
            {
                RegexHistoryListBox.Focus();
                if (RegexHistoryListBox.Items.Count > 0)
                    RegexHistoryListBox.SelectedIndex = 0;
            }
        }

        private void RegexHistoryListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var focused = FocusManager.GetFocusedElement(this);

            var item = focused as ListBoxItem;
            if (focused != RegexHistoryButton && (item == null || !RegexHistoryListBox.Items.Contains(item.DataContext)))
            {
                RegexHistoryPopup.IsOpen = false;
            }
        }

        private void Root_LocationChanged(object sender, EventArgs e)
        {
            SaveWindowPosition();

            RegexHistoryPopup.IsOpen = false;
            ReplaceHistoryPopup.IsOpen = false;
        }

        private void RegexHistoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && RegexHistoryListBox.SelectedValue != null)
            {
                RegexEditor.Text = RegexHistoryListBox.SelectedValue.ToString();
                RegexHistoryPopup.IsOpen = false;
                SetToHistory(1);
            }
            else if(e.Key == Key.Escape)
            {
                RegexHistoryPopup.IsOpen = false;
                RegexHistoryButton.Focus();
            }
        }

        private void RegexHistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(RegexHistoryListBox.SelectedValue != null)
            {
                RegexEditor.Text = RegexHistoryListBox.SelectedValue.ToString();
                RegexHistoryPopup.IsOpen = false;
                SetToHistory(1);
            }
        }

        private void ReplaceHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ReplaceHistoryPopup.IsOpen = !ReplaceHistoryPopup.IsOpen;
            if (ReplaceHistoryPopup.IsOpen)
            {
                ReplaceHistoryListBox.Focus();
                if (ReplaceHistoryListBox.Items.Count > 0)
                    ReplaceHistoryListBox.SelectedIndex = 0;
            }
        }

        private void ReplaceHistoryListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var focused = FocusManager.GetFocusedElement(this);

            var item = focused as ListBoxItem;
            if (focused != ReplaceHistoryButton && (item == null || !ReplaceHistoryListBox.Items.Contains(item.DataContext)))
            {
                ReplaceHistoryPopup.IsOpen = false;
            }
        }

        private void ReplaceHistoryListBox_KeyDown(object sender, KeyEventArgs e)
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

        private void ReplaceHistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(ReplaceHistoryListBox.SelectedValue != null)
            {
                ReplaceEditor.Text = ReplaceHistoryListBox.SelectedValue.ToString();
                ReplaceHistoryPopup.IsOpen = false;
                SetToHistory(2);
            }
        }

        private void ShowMatchesMenu_Click(object sender, RoutedEventArgs e)
        {
            Config.Instance.MatchesShowLevel = 1;
            Config.Instance.Save();
            RefreshMatches();
        }

        private void ShowGroupsMenu_Click(object sender, RoutedEventArgs e)
        {
            Config.Instance.MatchesShowLevel = 2;
            Config.Instance.Save();
            RefreshMatches();
        }

        private void ShowCapturesMenu_Click(object sender, RoutedEventArgs e)
        {
            Config.Instance.MatchesShowLevel = 3;
            Config.Instance.Save();
            RefreshMatches();
        }

        private void RefreshMatches()
        {
            if(lastMatchesText.Equals(GetText()))
            {
                try
                {
                    using (Dispatcher.DisableProcessing())
                    {
                        ((List<RegexResult>)MatchResultsTreeView.ItemsSource)
                            .ForEach(delegate(RegexResult regRes)
                            {
                                regRes.RefreshExpands();
                            });
                    }
                }
                catch
                {}
                
            }
            else
            {
                ShowMatches();
            }
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
                            dynamic script = CSScript.Evaluator.LoadCode(Res.CSharpReplaceContainer.Replace("//code", ReplaceEditor.Text));

                            int index = -1;

                            SetText(regex.Replace(text, match =>
                            {
                                index++;
                                return script.Replace(match, index, regexFileResult.FileName, index + (regexFileResult.Children.Count > 0 ? regexFileResult.Children[0].RegexElementNb : 0), regexFileResult.RegexElementNb - 1);
                            }));
                        }
                        else
                        {
                            SetText(regex.Replace(text, ReplaceEditor.Text));
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
                    else if (!string.IsNullOrEmpty(regexResult.FileName) && !Config.Instance.OpenFilesForReplace &&
                        MessageBox.Show("This will modify the file directly on the disk.\r\nModifications can not be cancel\r\nDo you want to continue ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
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
                            dynamic script = CSScript.Evaluator.LoadCode(Res.CSharpReplaceContainer.Replace("//code", ReplaceEditor.Text));

                            if (regexResult is RegexMatchResult regexMatchResult)
                                newText = beforeMatch + script.Replace((Match)regexMatchResult.RegexElement, regexMatchResult.RegexElementNb, regexResult.FileName, regexMatchResult.RegexElementNb, 0) + afterMatch;
                            else if (regexResult is RegexGroupResult regexGroupResult)
                                newText = beforeMatch + script.Replace((Match)regexGroupResult.Parent.RegexElement, (Group)regexGroupResult.RegexElement , regexResult.RegexElementNb, regexResult.FileName, regexResult.RegexElementNb, 0) + afterMatch;
                            else if (regexResult is RegexCaptureResult regexCaptureResult)
                                newText = beforeMatch + script.Replace((Match)regexCaptureResult.Parent.Parent.RegexElement,(Group)regexCaptureResult.Parent.RegexElement, (Capture)regexCaptureResult.RegexElement , regexResult.RegexElementNb, regexResult.FileName, regexResult.RegexElementNb, 0) + afterMatch;
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
            if (MatchResultsTreeView.SelectedValue is RegexResult regexResult)
            {
                ReplaceEditor.Document.Replace(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength, regexResult.Value);
            }
        }

        private void InsertGroupByNumberInReplaceField_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MatchResultsTreeView.SelectedValue is RegexGroupResult regexGroupResult)
            {
                ReplaceEditor.Document.Replace(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength, "$" + regexGroupResult.RegexElementNb.ToString());
            }
        }

        private void InsertGroupByNameInReplaceField_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MatchResultsTreeView.SelectedValue is RegexGroupResult regexGroupResult)
            {
                ReplaceEditor.Document.Replace(ReplaceEditor.SelectionStart, ReplaceEditor.SelectionLength, "${" + regexGroupResult.GroupName + "}");
            }
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
            SetToHistory(1);
        }

        private void PutInReplaceHistory_Click(object sender, RoutedEventArgs e)
        {
            SetToHistory(2);
        }

        private void New_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetToHistory();

            RegexEditor.Text = "";
            ReplaceEditor.Text = "";

            regExOptionViewModelsList.ForEach(delegate(RegExOptionViewModel optionModel)
            {
                optionModel.Selected = false;
            });
        }

        private void Open_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenRegex();        
        }

        private void OpenRegex()
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

            if(result.HasValue && result.Value)
            {
                if (File.Exists(dialog.FileName))
                {
                    try
                    {
                        SetToHistory();

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(dialog.FileName);

                        XmlElement root = xmlDoc.DocumentElement;

                        RegexEditor.Text = root.SelectSingleNode("//FindPattern").InnerText;
                        ReplaceEditor.Text = root.SelectSingleNode("//ReplacePattern").InnerText;

                        string[] xOptions = root.SelectSingleNode("//Options").InnerText.Split(' ');

                        regExOptionViewModelsList.ForEach(delegate(RegExOptionViewModel optionModel)
                        {
                            optionModel.Selected = xOptions.Contains(optionModel.Name);    
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Save_as_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                DefaultExt = "regex",
                Filter = "Regex files|*.regex",
                FilterIndex = 0
            };

            bool? result = dialog.ShowDialog(this);

            if(result.HasValue && result.Value)
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
                    root.AppendChild(replacePatternElement);
                    root.AppendChild(optionsElement);

                    XmlText findPatternText = xmlDoc.CreateTextNode(RegexEditor.Text);
                    XmlText replacePatternText = xmlDoc.CreateTextNode(ReplaceEditor.Text);

                    string sOptionsText = regExOptionViewModelsList
                        .Aggregate<RegExOptionViewModel, string>("", (total, next) => total + (next.Selected ? next.Name + " " : ""))
                        .Trim();

                    XmlText optionsText = xmlDoc.CreateTextNode(sOptionsText);

                    findPatternElement.AppendChild(findPatternText);
                    replacePatternElement.AppendChild(replacePatternText);
                    optionsElement.AppendChild(optionsText);

                    xmlDoc.Save(dialog.FileName);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.Close();
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                ShowMatches();
                e.Handled = true;
            }
        }

        private void cmiRegexCopyForOnOneLine_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(RegexPatternIndenter.SetOnOneLine(RegexEditor.SelectionLength > 0 ? RegexEditor.SelectedText : RegexEditor.Text));
        }

        private void cmiRegexCopyForXml_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText((RegexEditor.SelectionLength > 0 ? RegexEditor.SelectedText : RegexEditor.Text).EscapeXml());
        }

        private void cmiRegexPasteFromXml_Click(object sender, RoutedEventArgs e)
        {
            RegexEditor.SelectedText = Clipboard.GetText().UnescapeXml();
        }

        private void cmiRegexCut_Click(object sender, RoutedEventArgs e)
        {
            RegexEditor.Cut();
        }

        private void cmiRegexCopy_Click(object sender, RoutedEventArgs e)
        {
            RegexEditor.Copy();
        }

        private void cmiRegexPaste_Click(object sender, RoutedEventArgs e)
        {
            RegexEditor.Paste();
        }

        private void cmiReplaceCut_Click(object sender, RoutedEventArgs e)
        {
            ReplaceEditor.Cut();
        }

        private void cmiReplaceCopy_Click(object sender, RoutedEventArgs e)
        {
            ReplaceEditor.Copy();
        }

        private void cmiReplacePaste_Click(object sender, RoutedEventArgs e)
        {
            ReplaceEditor.Paste();
        }

        private void cmiRegexSelectAll_Click(object sender, RoutedEventArgs e)
        {
            RegexEditor.SelectAll();
        }

        private void cmiReplaceSelectAll_Click(object sender, RoutedEventArgs e)
        {
            ReplaceEditor.SelectAll();
        }

        private void cmiRegexIndent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetToHistory(1);

                if(RegexEditor.SelectionLength > 0)
                {
                    RegexEditor.SelectedText = IndentRegexPattern(RegexEditor.SelectedText);
                }
                else
                    RegexEditor.Text = IndentRegexPattern(RegexEditor.Text);
                
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

        private void cmiRegexSetOnOneLine_Click(object sender, RoutedEventArgs e)
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

        private void miRegexOption_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            if(sender != null)
            {
                mi.IsChecked = !mi.IsChecked;
            }
        }

        private void ClearRegexHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("It will clear the C# Regex field history. This action is not cancelable.\nDo you want to continue?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Config.Instance.RegexHistory.Clear();
                Config.Instance.Save();
            }
        }

        private void ClearReplaceHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("It will clear the Replace field history. This action is not cancelable.\nDo you want to continue?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Config.Instance.ReplaceHistory.Clear();
                Config.Instance.Save();
            }
        }

        private void ClearDirectoryHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("It will clear the Directory field history. This action is not cancelable.\nDo you want to continue?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Config.Instance.TextSourceDirectoryPathHistory.Clear();
                Config.Instance.Save();
            }
        }

        private void ClearDirectoryFilterHistory_Click(object sender, RoutedEventArgs e)
        {
            {
                if (MessageBox.Show("It will clear the Directory filter field history. This action is not cancelable.\nDo you want to continue?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Config.Instance.TextSourceDirectorySearchFilterHistory.Clear();
                    Config.Instance.Save();
                }
            }
        }

        private void CSharpReplaceCheckbox_IsChecked_Changed(object sender, RoutedEventArgs e)
        {
            if (CSharpReplaceCheckbox.IsChecked.GetValueOrDefault(false))
            {
                ReplaceEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            }
            else
            {
                XmlReader reader2 = XmlReader.Create(new StringReader(Res.Replace_syntax_color));

                ReplaceEditor.SyntaxHighlighting = HighlightingLoader.Load(reader2, HighlightingManager.Instance);
            }
        }

        private void SpecifiedDirectoryTextSourcePathButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog()
            {
                Description = "Select source folder",
                UseDescriptionForTitle = true
            };

            if(folderBrowserDialog.ShowDialog(GetWindow(this)).GetValueOrDefault(false))
            {
                SpecifiedDirectoryTextSourcePathComboBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void RestoreLastMachesSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetPosition?.Invoke(lastSelectionStart, lastSelectionLength);
            }
            catch { }
        }
    }
}
