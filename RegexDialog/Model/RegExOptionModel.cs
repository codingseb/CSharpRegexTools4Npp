using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegExOptionViewModel : INotifyPropertyChanged
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

        private RegexOptions regexOptions = RegexOptions.None;

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

        #region INotifyPropertyChanged Membres

        /// <summary>
        /// Génère l'évènement PropertyChanged pour la propriété spécifiée
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
