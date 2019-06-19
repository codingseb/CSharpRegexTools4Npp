using System;
using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegExOptionViewModel : NotifyPropertyChangedBaseClass
    {
        /// <summary>
        /// Sélectionné
        /// </summary>
        public bool Selected
        {
            get
            {
                return Config.Instance.RegexOptionsSelection.ContainsKey(Name) && Config.Instance.RegexOptionsSelection[Name];
            }
            set
            {
                Config.Instance.RegexOptionsSelection[Name] = value;
                Config.Instance.Save();
                NotifyPropertyChanged();
            }
        }

        private RegexOptions regexOptions;

        /// <summary>
        /// L'option d'expression régulière représentée
        /// </summary>
        public RegexOptions RegexOptions
        {
            get { return regexOptions; }
            set
            {
                regexOptions = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Nom à affiché
        /// </summary>
        public string Name
        {
            get
            {
                return Enum.GetName(typeof(RegexOptions), regexOptions);
            }
        }
    }
}
