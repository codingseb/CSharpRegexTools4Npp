using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RegexDialog
{
    internal class Config : INotifyPropertyChanged
    {
        #region Json singleton

        private static readonly string fileName = Path.Combine(PathUtils.AppDataRoamingPath, "Config.json");

        private static Config instance = null;

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    if (File.Exists(fileName))
                    {
                        try
                        {
                            instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(fileName));
                            instance.Init();
                        }
                        catch { }
                    }

                    if (instance == null)
                    {
                        instance = new Config();
                        instance.Save();
                        instance.Init();
                    }
                }

                return instance;
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            try
            {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(this, Formatting.Indented, new StringEnumConverter()));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "errorTitle", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Config()
        { }

        #endregion

        #region On PropertyChanged 

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        

    #endregion

    private void Init()
        {
        }

        public string RegexEditorText { get; set; }
        public string ReplaceEditorText { get; set; }

        public ObservableCollection<string> RegexHistory { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ReplaceHistory { get; set; } = new ObservableCollection<string>();

        public int HistoryToKeep { get; set; } = 100;

        public bool CSharpReplaceMode { get; set; }

        public int OptionTabControlSelectedTabItemIndex { get; set; } = 0;

        public double? DialogLeft { get; set; } = 0;
        public double? DialogTop { get; set; } = 0;
        public double? DialogWidth { get; set; } = 800;
        public double? DialogHeight { get; set; } = 400;
        public bool DialogMaximized { get; set; } = false;

        public GridLength GridFirstColumnWidth { get; set; } = new GridLength(1, GridUnitType.Star);
        public GridLength GridSecondColumnWidth { get; set; } = new GridLength(1, GridUnitType.Star);
        public GridLength GridThirdColumnWidth { get; set; } = new GridLength(1, GridUnitType.Star);
        public GridLength GridRegexEditorRowHeight { get; set; } = new GridLength(1, GridUnitType.Star);
        public GridLength GridReplaceEditorRowHeight { get; set; } = new GridLength(1, GridUnitType.Star);
        public GridLength GridRegexLanguageElementsFirstRowHeight { get; set; } = new GridLength(1, GridUnitType.Star);

        public int MatchesShowLevel { get; set; } = 1;

        public ObservableDictionary<string, bool> RegexOptionsSelection { get; set; } = new ObservableDictionary<string, bool>();

        public RegexTextSource TextSourceOn { get; set; } = RegexTextSource.CurrentTab;

        public string TextSourceDirectoryPath { get; set; } = string.Empty;
        public ObservableCollection<string> TextSourceDirectoryPathHistory { get; set; } = new ObservableCollection<string>();

        public string TextSourceDirectorySearchFilter { get; set; } = string.Empty;
        public ObservableCollection<string> TextSourceDirectorySearchFilterHistory { get; set; } = new ObservableCollection<string>();

        public bool TextSourceDirectorySearchSubDirectories { get; set; } = false;

        public bool TextSourceDirectoryShowNotMatchedFiles { get; set; } = true;

        public bool PrintFileNameWhenExtract { get; set; } = false;

        public bool OpenFilesForReplace { get; set; } = true;

        public bool ShowEmptyMatches { get; set; } = false;

        public bool AutoIndentCharClassesOnOneLine { get; set; } = true;
        public bool AutoIndentKeepQuantifiersOnSameLine { get; set; } = true;

        public bool ShowLinesNumbersRegexEditorOption { get; set; } = false;
        public bool ShowSpaceCharsRegexEditorOption { get; set; } = false;
        public bool ShowEndOfLinesRegexEditorOption { get; set; } = false;

        public bool ShowLinesNumbersReplaceEditorOption { get; set; } = false;
        public bool ShowSpaceCharsReplaceEditorOption { get; set; } = false;
        public bool ShowEndOfLinesReplaceEditorOption { get; set; } = false;
    }
}
