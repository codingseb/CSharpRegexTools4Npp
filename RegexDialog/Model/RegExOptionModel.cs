using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegExOptionViewModel : NotifyPropertyChangedBaseClass
    {
        private static readonly Dictionary<string, string> optionNameToDescriptionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Res.RegexOptionsDescriptions);

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
            }
        }

        /// <summary>
        /// L'option d'expression régulière représentée
        /// </summary>
        public RegexOptions RegexOptions { get; set; }

        /// <summary>
        /// Nom à affiché
        /// </summary>
        public string Name => Enum.GetName(typeof(RegexOptions), RegexOptions);

        public string Description => optionNameToDescriptionDictionary[Name];
    }
}
