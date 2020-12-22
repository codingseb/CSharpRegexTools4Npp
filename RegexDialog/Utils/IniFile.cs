using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RegexDialog
{
    /// <summary>
    /// Cette classe permet de charger et sauvegarder des fichiers ini.
    /// On accède aux éléments en spécifiant la section et la clé.
    /// On peut également récupérer un HashTable de toute une section.
    /// </summary>
    public class IniFile
    {
        private readonly object lockSave = new object();

        /// <summary>
        /// Caractères de retour à la ligne.
        /// </summary>
        protected const string newline = "\r\n";

        /// <summary>
        /// Dictionnaire des sections
        /// </summary>
        protected Dictionary<string, Section> Sections = new Dictionary<string, Section>();

        /// <summary>
        /// Listes des noms de section pour pouvoir les ordrer (Et faire des Inserts)
        /// </summary>
        protected List<string> ListSections = new List<string>();

        /// <summary>
        /// Dictionnaire des commentaires
        /// </summary>
        protected Dictionary<string, string> linkedComments = new Dictionary<string, string>();

        /// <summary>
        /// Dictionnaire des commentaires placer seul sur une ligne avant une section ou une clé.
        /// </summary>
        protected Dictionary<string, string> beforeCodeCommentsOrEmptyLines = new Dictionary<string, string>();

        /// <summary>
        /// Commentaire d'entête de fichier
        /// </summary>
        protected string fileHeaderCommentsOrEmptyLines = "";

        /// <summary>
        /// Commentaire de pied de page de fichier
        /// </summary>
        protected string fileFooterCommentsOrEmptyLines = "";

        /// <summary>
        /// Variable stockant le chemin d'accès au fichier ini.
        /// </summary>
        protected string sFileName;

        /// <summary>
        /// spécifie si le fichier est en lecture seul ou pas
        /// </summary>
        protected bool readOnly;

        /// <summary>
        /// Spécifie si le fichier est chargé correctement.
        /// </summary>
        protected bool loaded;

        /// <summary>
        /// Spécifie si la section et la clé doivent être créé lors de la lecture d'une valeur
        /// lorsque ceux-ci n'existe pas.
        /// </summary>
        protected bool createIfValueDoNotExist = true;

        /// <summary>
        /// Délégué pour l'évènement CommentChanging de la classe IniFile
        /// </summary>
        /// <param name="sender">L'élément qui a généré l'évènement</param>
        /// <param name="e">Paramètres de l'évènement</param>
        public delegate void CommentChangingEventHandler(object sender, CommentChangingEventArgs e);

        /// <summary>
        /// Délégué pour l'évènement CommentChanged de la classe IniFile
        /// </summary>
        /// <param name="sender">L'élément qui a généré l'évènement</param>
        /// <param name="e">Paramètres de l'évènement</param>
        public delegate void CommentChangedEventHandler(object sender, CommentChangedEventArgs e);

        /// <summary>
        /// Délégué pour l'évènement ValueChanging de la classe IniFile
        /// </summary>
        /// <param name="sender">L'élément qui a généré l'évènement</param>
        /// <param name="e">Paramètres de l'évènement</param>
        public delegate void ValueChangingEventHandler(object sender, ValueChangingEventArgs e);

        /// <summary>
        /// Délégué pour l'évènement ValueChanged de la classe IniFile
        /// </summary>
        /// <param name="sender">L'élément qui a généré l'évènement</param>
        /// <param name="e">Paramètres de l'évènement</param>
        public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);

        /// <summary>
        /// Délégué pour l'évènement IniFileSaving de la classe IniFile
        /// </summary>
        /// <param name="sender">L'élément qui a généré l'évènement</param>
        /// <param name="e">Paramètres de l'évènement</param>
        public delegate void IniFileSavingEventHandler(object sender, IniFileSavingEventArgs e);

        /// <summary>
        /// Délégué pour l'évènement IniFileSaved de la classe IniFile
        /// </summary>
        /// <param name="sender">L'élément qui a généré l'évènement</param>
        /// <param name="e">Paramètres de l'évènement</param>
        public delegate void IniFileSavedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Délégué pour l'évènement IniFileLoadedStateChanged de la classe IniFile
        /// </summary>
        /// <param name="sender">L'élément qui a généré l'évènement</param>
        /// <param name="e">Paramètres de l'évènement</param>
        public delegate void IniFileLoadedStateChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Survient lorsqu'un commentaire est sur le point d'être changé.
        /// </summary>
        public event CommentChangingEventHandler CommentChanging;

        /// <summary>
        /// Survient lorsqu'un commentaire a été changé.
        /// </summary>
        public event CommentChangedEventHandler CommentChanged;

        /// <summary>
        /// Survient lorsqu'une valeur est sur le point d'être changée.
        /// </summary>
        public event ValueChangingEventHandler ValueChanging;

        /// <summary>
        /// Survient lorsqu'une valeur a été changée.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// Survient lorsque le fichier ini est sur le point d'être sauvegardé.
        /// </summary>
        public event EventHandler<IniFileSavingEventArgs> IniFileSaving;

        /// <summary>
        /// Survient lorsque le fichier ini a été sauvegargé.
        /// </summary>
        public event EventHandler<EventArgs> IniFileSaved;

        /// <summary>
        /// Survient lorsque l'état loaded du fichier ini a changé.
        /// </summary>
        public event EventHandler<EventArgs> IniFileLoadedStateChanged;

        /// <summary>
        /// Spécifie si le fichier est chargé correctement.
        /// </summary>
        public bool Loaded
        {
            get { return loaded; }
        }

        /// <summary>
        /// Le chemin d'accès du fichier INI
        /// </summary>
        public virtual string FileName
        {
            get { return sFileName; }
            set { sFileName = value; }
        }

        /// <summary>
        /// Spécifie si la section et la clé doivent être créé lors de la lecture d'une valeur
        /// lorsque ceux-ci n'existe pas. Par défaut est à <c>true</c>
        /// </summary>
        public bool CreateIfValueDoNotExist
        {
            get { return createIfValueDoNotExist; }
            set { createIfValueDoNotExist = value; }
        }

        /// <summary>
        /// Obtient ou attribue le commentaire d'entête du fichier ini
        /// </summary>
        public string FileHeaderComment
        {
            get
            {
                return fileHeaderCommentsOrEmptyLines;
            }

            set
            {
                // Si la dernière ligne n'est pas une ligne vide, on en rajoute une.
                if (!value.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).Last().Replace(" ", "").Replace("\t", "").Equals(""))
                {
                    fileHeaderCommentsOrEmptyLines = value + newline;
                }
                // Sinon on sauvegarde le commentaire tel quel
                else
                {
                    fileHeaderCommentsOrEmptyLines = value;
                }
            }
        }

        /// <summary>
        /// Obtient ou attribue le commentaire de fin du fichier ini
        /// </summary>
        public string FileFooterComment
        {
            get
            {
                return fileFooterCommentsOrEmptyLines;
            }

            set
            {
                fileFooterCommentsOrEmptyLines = value;
            }
        }

        /// <summary>
        /// Retourne la liste des noms de toutes les sections du fichier ini.
        /// </summary>
        public List<string> SectionsNames
        {
            get
            {
                return Sections.Select<KeyValuePair<string, Section>, string>(pair => pair.Key).ToList<string>();
            }
        }

        ////////////////////
        // Constructeurs
        ////////////////////

        /// <summary>
        /// Constructeur de base.
        /// </summary>
        public IniFile() { }

        /// <summary>
        /// Crée une nouvelle instance de IniFile et charge le fichier ini
        /// </summary>
        /// <param name="fileName">Chemin du fichier ini</param>
        public IniFile(string fileName)
        {
            InitIniFile(fileName);
        }

        protected void InitIniFile(string fileName)
        {
            sFileName = fileName;

            if (File.Exists(fileName))
                Load(fileName);
        }

        /// <summary>
        /// Crée une nouvelle instance de IniFile et charge le fichier ini
        /// </summary>
        /// <param name="fileName">Chemin du fichier ini</param>
        /// <param name="encoding">L'encodage de caractère à utiliser pour la lecture du fichier si pas UTF-8</param>
        public IniFile(string fileName, Encoding encoding)
        {
            InitIniFile(fileName, encoding);
        }

        protected void InitIniFile(string fileName, Encoding encoding)
        {
            sFileName = fileName;

            if (File.Exists(fileName))
                Load(fileName, encoding);
        }

        /// <summary>
        /// Crée une nouvelle instance de IniFile et charge le fichier ini
        /// </summary>
        /// <param name="readOnly">Signal que le fichier est en lecture seule.</param>
        /// <param name="fileName">Chemin du fichier ini</param>
        public IniFile(bool readOnly, string fileName)
        {
            InitIniFile(readOnly, fileName);
        }

        protected void InitIniFile(bool readOnly, string fileName)
        {
            sFileName = fileName;

            this.readOnly = readOnly;

            if (File.Exists(fileName))
                Load(fileName);
        }

        /// <summary>
        /// Crée une nouvelle instance de IniFile et charge le fichier ini
        /// </summary>
        /// <param name="readOnly">Signal que le fichier est en lecture seule.</param>
        /// <param name="fileName">Chemin du fichier ini</param>
        /// <param name="encoding">L'encodage de caractère à utiliser pour la lecture du fichier si pas UTF-8</param>
        public IniFile(bool readOnly, string fileName, Encoding encoding)
        {
            InitIniFile(readOnly, fileName, encoding);
        }

        protected void InitIniFile(bool readOnly, string fileName, Encoding encoding)
        {
            sFileName = fileName;

            this.readOnly = readOnly;

            if (File.Exists(fileName))
                Load(fileName, encoding);
        }

        ////////////////////
        // Méthodes
        ////////////////////

        /// <summary>
        /// Génère l'évènement ValueChanging avec les paramètres spécifiés
        /// </summary>
        /// <param name="e">les paramètre à inclure dans l'évènement</param>
        protected void OnValueChanging(ValueChangingEventArgs e)
        {
            ValueChanging?.Invoke(this, e);
        }

        /// <summary>
        /// Génère l'évènement ValueChanged avec les paramètres spécifiés
        /// </summary>
        /// <param name="e">les paramètre à inclure dans l'évènement</param>
        protected void OnValueChanged(ValueChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Génère l'évènement CommentChanging avec les paramètres spécifiés
        /// </summary>
        /// <param name="e">les paramètre à inclure dans l'évènement</param>
        protected void OnCommentChanging(CommentChangingEventArgs e)
        {
            CommentChanging?.Invoke(this, e);
        }

        /// <summary>
        /// Génère l'évènement CommentChanged avec les paramètres spécifiés
        /// </summary>
        /// <param name="e">les paramètre à inclure dans l'évènement</param>
        protected void OnCommentChanged(CommentChangedEventArgs e)
        {
            CommentChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Génère l'évènement IniFileSaving avec les paramètres spécifiés
        /// </summary>
        /// <param name="e">les paramètre à inclure dans l'évènement</param>
        protected void OnIniFileSaving(IniFileSavingEventArgs e)
        {
            IniFileSaving?.Invoke(this, e);
        }

        /// <summary>
        /// Génère l'évènement IniFileSaved avec les paramètres spécifiés
        /// </summary>
        /// <param name="e">les paramètre à inclure dans l'évènement</param>
        protected void OnIniFileSaved(EventArgs e)
        {
            IniFileSaved?.Invoke(this, e);
        }

        /// <summary>
        /// Génère l'évènement IniFileLoadedStateChanged.
        /// </summary>
        protected void OnIniFileLoadedStateChanged()
        {
            IniFileLoadedStateChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Tente de renommer une section
        /// </summary>
        /// <param name="currentSectionName">le nom courrant de la section à renommer</param>
        /// <param name="newSectionName">le nouveau nom à donner à la section</param>
        /// <returns><c>true</c> si  le renommage s'est bien passé, <c>false</c> si le renommage n'est pas possible</returns>
        public bool RenameSection(string currentSectionName, string newSectionName)
        {
            bool result = false;

            if (Sections.ContainsKey(currentSectionName) && !Sections.ContainsKey(newSectionName))
            {
                Section temp = Sections[currentSectionName];

                if (Sections.Remove(currentSectionName))
                {
                    //
                    // Renommage de la section
                    // 
                    Sections.Add(newSectionName, temp);
                    int index = ListSections.IndexOf(currentSectionName);

                    ListSections[index] = newSectionName;

                    //
                    // Suivi des commentaire de la section
                    //

                    if (linkedComments.ContainsKey("[" + currentSectionName + "]"))
                    {
                        string comment = linkedComments["[" + currentSectionName + "]"];

                        linkedComments.Remove("[" + currentSectionName + "]");
                        linkedComments["[" + newSectionName + "]"] = comment;
                    }

                    if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + currentSectionName + "]"))
                    {
                        string comment = beforeCodeCommentsOrEmptyLines["[" + currentSectionName + "]"];

                        beforeCodeCommentsOrEmptyLines.Remove("[" + currentSectionName + "]");
                        beforeCodeCommentsOrEmptyLines["[" + newSectionName + "]"] = comment;
                    }

                    //
                    // Suivi des commentaires des clés de la section.
                    //

                    GetKeysOfSection(newSectionName).ForEach(key =>
                    {
                        if (linkedComments.ContainsKey("[" + currentSectionName + "]" + key))
                        {
                            string comment = linkedComments["[" + currentSectionName + "]" + key];

                            linkedComments.Remove("[" + currentSectionName + "]" + key);
                            linkedComments["[" + newSectionName + "]" + key] = comment;
                        }

                        if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + currentSectionName + "]" + key))
                        {
                            string comment = beforeCodeCommentsOrEmptyLines["[" + currentSectionName + "]" + key];

                            beforeCodeCommentsOrEmptyLines.Remove("[" + currentSectionName + "]" + key);
                            beforeCodeCommentsOrEmptyLines["[" + newSectionName + "]" + key] = comment;
                        }
                    });

                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Ajoute une section [section] au fichier ini
        /// </summary>
        /// <param name="section">Nom de la section à créer</param>
        public void AddSection(string section)
        {
            if (!Sections.ContainsKey(section))
            {
                Sections.Add(section, new Section());   // Ajoute dans le Dictionary
                ListSections.Add(section);              // Ajoute dans la liste
            }
        }

        /// <summary>
        /// Ajoute une section [section] au fichier ini ainsi qu'une clef et une valeur
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clef</param>
        /// <param name="value">Valeur de la clef</param>
        public void AddSection(string section, string key, string value)
        {
            AddSection(section);
            ((Section)Sections[section]).SetKey(key, value);
        }

        /// <summary>
        /// Insere une section [section] apres la section "previousSect"
        /// </summary>
        /// <param name="section">Nom de la section à créer</param>
        /// <param name="previousSect">Nom de la section qui sera la précedente</param>
        public void InsertSection(string section, string previousSect)
        {
            if (!Sections.ContainsKey(section))
            {
                // Ajoute dans le Dictionary
                Sections.Add(section, new Section());

                // Ajoute dans la liste
                if (previousSect != null)
                {
                    if (Sections.ContainsKey(previousSect))
                    {
                        int index = ListSections.IndexOf(previousSect) + 1;     // Insere apres "previousSect
                        if (index < ListSections.Count())
                            ListSections.Insert(index, section);                // Insere dans la liste
                        else ListSections.Add(section);                         // Insere en fin de liste
                    }
                    else ListSections.Add(section);                             // Insere en fin de liste
                }
                else ListSections.Insert(0, section);                           // Insere en debut de liste
            }
        }

        /// <summary>
        /// Retire une section du fichier
        /// </summary>
        /// <param name="section">Nom de la section à enlever</param>
        public void RemoveSection(string section)
        {
            if (Sections.ContainsKey(section))
            {
                Sections.Remove(section);
                ListSections.Remove(section);

                try
                {
                    if (linkedComments.ContainsKey("[" + section + "]"))
                        linkedComments.Remove("[" + section + "]");

                    if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + section + "]"))
                        beforeCodeCommentsOrEmptyLines.Remove("[" + section + "]");
                }
                catch { }
            }
        }

        /// <summary>
        /// Supprime toutes les sections
        /// </summary>
        public void RemoveAllSections()
        {
            Sections.Clear();
            ListSections.Clear();

            linkedComments.Clear();
            beforeCodeCommentsOrEmptyLines.Clear();
        }

        /// <summary>
        /// Récupère la liste des clés d'une section
        /// </summary>
        /// <param name="section">La section en question</param>
        /// <returns>Les clés de les section</returns>
        public List<string> GetKeysOfSection(string section)
        {
            List<string> result = null;

            try
            {
                result = new List<string>(Sections[section].GetKeys().Keys);
            }
            catch
            {
                result = new List<string>();
            }

            return result;
        }

        /// <summary>
        /// Récupère la liste des valeurs de toutes les clés d'une section
        /// </summary>
        /// <param name="section">La section en question</param>
        /// <returns>Les valeurs de toutes les clés de la section</returns>
        public List<string> GetValuesOfSection(string section)
        {
            List<string> result = null;

            try
            {
                result = new List<string>(Sections[section].GetKeys().Values);
            }
            catch
            {
                result = new List<string>();
            }

            return result;
        }

        /// <summary>Ca
        /// Modifie ou crée une valeur d'une clé dans une section
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clef</param>
        /// <param name="value">Valeur de la clef</param>
        public void SetValue(string section, string key, string value)
        {
            string oldValue = "";

            if (this.SectionExist(section) && this.KeyExist(section, key))
                oldValue = this[section][key];

            ValueChangingEventArgs tmpArg = new ValueChangingEventArgs(section, key, oldValue, value);
            OnValueChanging(tmpArg);

            if (!tmpArg.Cancel)
            {
                this[section].SetKey(key, value);

                OnValueChanged(new ValueChangedEventArgs(section, key, oldValue, value));
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clef dans une section
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clef</param>
        /// <param name="defaut">Valeur par défaut si la clef/section n'existe pas</param>
        /// <returns>Valeur de la clef, ou la valeur entrée par défaut</returns>
        public string GetValue(string section, string key, string defaut)
        {
            if (CreateIfValueDoNotExist)
            {
                string val = this[section][key];
                if (val == "")
                {
                    this[section][key] = defaut.ToString();
                    return defaut.ToString();
                }
                else
                    return val;
            }
            else
            {
                return GetValueWithoutCreating(section, key, defaut);
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clef dans une section
        /// Ne crée pas la clé si celle ci n'existe pas. Ceci même si l'attribut CreateIfValueDoNotExist est à true.
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clef</param>
        /// <param name="defaut">Valeur par défaut si la clef/section n'existe pas</param>
        /// <returns>Valeur de la clef, ou la valeur entrée par défaut</returns>
        public string GetValueWithoutCreating(string section, string key, string defaut)
        {
            if (Sections.ContainsKey(section))
            {
                Section askedSection = Sections[section];

                if (askedSection.GetKeys().ContainsKey(key))
                {
                    return askedSection[key];
                }
                else
                {
                    return (string)defaut;
                }
            }
            else
            {
                return (string)defaut;
            }
        }

        /// <summary>
        /// Verifie si la section existe
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public bool SectionExist(string section)
        {
            return Sections.ContainsKey(section);
        }

        /// <summary>
        /// Vérifie si la clé spécifiée existe
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyExist(string section, string key)
        {
            return Sections.ContainsKey(section) && Sections[section].KeyExists(key);
        }

        /// <summary>
        /// Retourne la valeur d'une clef dans une section
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clef</param>
        /// <returns>Valeur de la clef, ou "" si elle n'existe pas</returns>
        public string GetValue(string section, string key)
        {
            return GetValue(section, key, "");
        }

        /// <summary>
        /// Retourne la valeur d'une clef dans une section
        /// Ne crée pas la clé si celle ci n'existe pas. Ceci même si l'attribut CreateIfValueDoNotExist est à true.
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clef</param>
        /// <returns>Valeur de la clef, ou "" si elle n'existe pas</returns>
        public string GetValueWithoutCreating(string section, string key)
        {
            return GetValueWithoutCreating(section, key, "");
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre entier.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public int GetInt(string section, string key, int defaut)
        {
            try
            {
                string res = GetValue(section, key, defaut.ToString());
                return int.Parse(res);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre entier.
        /// Valeur par défaut 0
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public int GetInt(string section, string key)
        {
            return GetInt(section, key, 0);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre entier.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public int GetIntWithoutCreating(string section, string key, int defaut)
        {
            try
            {
                string res = GetValueWithoutCreating(section, key, defaut.ToString());
                return int.Parse(res);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre entier.
        /// Valeur par défaut 0
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public int GetIntWithoutCreating(string section, string key)
        {
            return GetIntWithoutCreating(section, key, 0);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public float GetFloat(string section, string key, float defaut)
        {
            try
            {
                string res = GetValue(section, key, defaut.ToString());
                return float.Parse(res, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante.
        /// Valeur par défaut 0.0f
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public float GetFloat(string section, string key)
        {
            return GetFloat(section, key, 0.0f);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public float GetFloatWithoutCreating(string section, string key, float defaut)
        {
            try
            {
                string res = GetValueWithoutCreating(section, key, defaut.ToString());
                return float.Parse(res, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante.
        /// Valeur par défaut 0.0f
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public float GetFloatWithoutCreating(string section, string key)
        {
            return GetFloatWithoutCreating(section, key, 0.0f);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante en double.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public double GetDouble(string section, string key, double defaut)
        {
            try
            {
                string res = GetValue(section, key, defaut.ToString());
                return double.Parse(res, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante en double.
        /// Valeur par défaut 0.0
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public double GetDouble(string section, string key)
        {
            return GetDouble(section, key, 0.0);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante en double.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public double GetDoubleWithoutCreating(string section, string key, double defaut)
        {
            try
            {
                string res = GetValueWithoutCreating(section, key, defaut.ToString());
                return double.Parse(res, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante en double.
        /// Valeur par défaut 0.0
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public double GetDoubleWithoutCreating(string section, string key)
        {
            return GetDoubleWithoutCreating(section, key, 0.0);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante en decimal.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public decimal GetDecimal(string section, string key, decimal defaut)
        {
            try
            {
                string res = GetValue(section, key, defaut.ToString());
                return decimal.Parse(res, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante en decimal.
        /// Valeur par défaut 0.0M
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public decimal GetDecimal(string section, string key)
        {
            return GetDecimal(section, key, 0.0M);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante en decimal.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public decimal GetDecimalWithoutCreating(string section, string key, decimal defaut)
        {
            try
            {
                string res = GetValueWithoutCreating(section, key, defaut.ToString());
                return decimal.Parse(res, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme de nombre à virgule flottante en decimal.
        /// Valeur par défaut 0.0M
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public decimal GetDecimalWithoutCreating(string section, string key)
        {
            return GetDecimalWithoutCreating(section, key, 0.0M);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme valeur boolean
        /// reconnait "true" et "false" sans tenir compte de la casse ou 0 = false et n'importe quel autre integer = true
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public bool GetBool(string section, string key, bool defaut)
        {
            try
            {
                string res = GetValue(section, key, defaut.ToString());
                if (res.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if (res.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else
                {
                    return int.Parse(res) != 0;
                }
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme valeur boolean
        /// reconnait "true" et "false" sans tenir compte de la casse ou 0 = false et n'importe quel autre integer = true
        /// valeur par défaut false
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public bool GetBool(string section, string key)
        {
            return GetBool(section, key, false);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme valeur boolean
        /// reconnait "true" et "false" sans tenir compte de la casse ou 0 = false et n'importe quel autre integer = true
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>Valeur du paramètre.</returns>
        public bool GetBoolWithoutCreating(string section, string key, bool defaut)
        {
            try
            {
                string res = GetValueWithoutCreating(section, key, defaut.ToString());
                if (res.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if (res.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else
                {
                    return int.Parse(res) != 0;
                }
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme valeur boolean
        /// reconnait "true" et "false" sans tenir compte de la casse ou 0 = false et n'importe quel autre integer = true
        /// valeur par défaut false
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public bool GetBoolWithoutCreating(string section, string key)
        {
            return GetBoolWithoutCreating(section, key, false);
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme d'un objet DateTime
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="format">Format de la date, syntaxe standard pour les format de DateTime</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>la valeur du paramètre</returns>
        public DateTime GetDateTime(string section, string key, string format, DateTime defaut)
        {
            try
            {
                return DateTime.ParseExact(GetValue(section, key, defaut.ToString(format, CultureInfo.InvariantCulture)), format, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme d'un objet DateTime
        /// Valeur par défaut : DateTime.Now
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="format">Format de la date, syntaxe standard pour les format de DateTime</param>
        /// <returns>la valeur du paramètre</returns>
        public DateTime GetDateTime(string section, string key, string format)
        {
            return GetDateTime(section, key, DateTime.Now.ToString(format, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme d'un objet DateTime
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="format">Format de la date, syntaxe standard pour les format de DateTime</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas ou que la valeur fourni n'est pas au bon format.</param>
        /// <returns>la valeur du paramètre</returns>
        public DateTime GetDateTimeWithoutCreating(string section, string key, string format, DateTime defaut)
        {
            try
            {
                return DateTime.ParseExact(GetValueWithoutCreating(section, key, defaut.ToString(format, CultureInfo.InvariantCulture)), format, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaut;
            }
        }

        /// <summary>
        /// Retourne la valeur d'une clé sous forme d'un objet DateTime
        /// Valeur par défaut : DateTime.Now
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="format">Format de la date, syntaxe standard pour les format de DateTime</param>
        /// <returns>la valeur du paramètre</returns>
        public DateTime GetDateTimeWithoutCreating(string section, string key, string format)
        {
            return GetDateTimeWithoutCreating(section, key, DateTime.Now.ToString(format, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retourne une liste de chaine de caractère séparé par le caractère spécifié.
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="sep">Caractère de séparation des éléments de la liste dans le fichier d'ini</param>
        /// <param name="defaut">Liste à retourner par défaut.</param>
        /// <returns>La liste de chaine de caractère</returns>
        public List<string> GetStringList(string section, string key, char sep, List<string> defaut)
        {
            List<string> result = null;

            try
            {
                result = GetValue(section, key, defaut.Count == 0 ? "" : defaut.Aggregate<string, string, string>("", (total, next) => total + next + sep.ToString(), total => total.Substring(0, total.Length - 1))).Split(sep).ToList<string>();
            }
            catch
            {
                result = defaut;
            }

            return result;
        }

        /// <summary>
        /// Retourne une liste de chaine de caractère séparé par le caractère spécifié.
        /// Par défaut une liste vide
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="sep">Caractère de séparation des éléments de la liste dans le fichier d'ini</param>
        /// <returns>La liste de chaine de caractère</returns>
        public List<string> GetStringList(string section, string key, char sep)
        {
            return GetStringList(section, key, sep, new List<string>());
        }

        /// <summary>
        /// Retourne une liste de chaine de caractère séparé par le caractère spécifié.
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="sep">Caractère de séparation des éléments de la liste dans le fichier d'ini</param>
        /// <param name="defaut">Liste à retourner par défaut au format texte.</param>
        /// <returns>La liste de chaine de caractère</returns>
        public List<string> GetStringList(string section, string key, char sep, string defaut)
        {
            List<string> result = null;

            try
            {
                result = GetValue(section, key, defaut).Split(sep).ToList<string>();
            }
            catch
            { }

            return result;
        }

        /// <summary>
        /// Retourne une liste de chaine de caractère séparé par le caractère spécifié.
        /// Ne crée pas la clé si elle n'existe pas.
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="sep">Caractère de séparation des éléments de la liste dans le fichier d'ini</param>
        /// <param name="defaut">Liste à retourner par défaut.</param>
        /// <returns>La liste de chaine de caractère</returns>
        public List<string> GetStringListWithoutCreating(string section, string key, char sep, List<string> defaut)
        {
            List<string> result = null;

            try
            {
                result = GetValueWithoutCreating(section, key, defaut.Count == 0 ? "" : defaut.Aggregate<string, string, string>("", (total, next) => total + next + sep.ToString(), total => total.Substring(0, total.Length - 1))).Split(sep).ToList<string>();
            }
            catch
            {
                result = defaut;
            }

            return result;
        }

        /// <summary>
        /// Retourne une liste de chaine de caractère séparé par le caractère spécifié.
        /// Par défaut une liste vide.
        /// Ne crée pas la clé si elle n'existe pas.
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="sep">Caractère de séparation des éléments de la liste dans le fichier d'ini</param>
        /// <returns>La liste de chaine de caractère</returns>
        public List<string> GetStringListWithoutCreating(string section, string key, char sep)
        {
            return GetStringListWithoutCreating(section, key, sep, new List<string>());
        }

        /// <summary>
        /// Retourne une liste de chaine de caractère séparé par le caractère spécifié.
        /// </summary>
        /// <param name="section">Nom de la section</param>
        /// <param name="key">Nom de la clé</param>
        /// <param name="sep">Caractère de séparation des éléments de la liste dans le fichier d'ini</param>
        /// <param name="defaut">Liste à retourner par défaut au format texte.</param>
        /// <returns>La liste de chaine de caractère</returns>
        public List<string> GetStringListWithoutCreating(string section, string key, char sep, string defaut)
        {
            List<string> result = null;

            try
            {
                result = GetValueWithoutCreating(section, key, defaut).Split(sep).ToList<string>();
            }
            catch
            { }

            return result;
        }

        /// <summary>
        /// Retourne un chemin d'accès d'un dossier ou d'un fichier
        /// si le chemin commence par "." le chemin est relatif à l'application, sinon le chemin est considérer comme absolu.
        /// par défaut retourne une chaine vide.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public string GetPath(string section, string key)
        {
            return GetPath(section, key, "");
        }

        /// <summary>
        /// Retourne un chemin d'accès d'un dossier ou d'un fichier
        /// si le chemin commence par "." le chemin est relatif à l'application, sinon le chemin est considérer comme absolu.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas.</param>
        /// <returns>Valeur du paramètre.</returns>
        public string GetPath(string section, string key, string defaut)
        {
            string path = GetValue(section, key, defaut);

            try
            {
                if (path.StartsWith("."))
                {
                    //path = Application.StartupPath + path.Substring(1, path.Length - 1);
                    path = Path.GetFullPath(Path.Combine(Application.StartupPath, path));
                }
                else
                {
                    path = Path.GetFullPath(path);
                }
            }
            catch
            {
                path = "";
            }


            return path;
        }

        /// <summary>
        /// Retourne un chemin d'accès d'un dossier ou d'un fichier
        /// si le chemin commence par "." le chemin est relatif à l'application, sinon le chemin est considérer comme absolu.
        /// par défaut retourne une chaine vide.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <returns>Valeur du paramètre.</returns>
        public string GetPathWithoutCreating(string section, string key)
        {
            return GetPathWithoutCreating(section, key, "");
        }

        /// <summary>
        /// Retourne un chemin d'accès d'un dossier ou d'un fichier
        /// si le chemin commence par "." le chemin est relatif à l'application, sinon le chemin est considérer comme absolu.
        /// </summary>
        /// <param name="section">Nom de la section.</param>
        /// <param name="key">Nom de la clé.</param>
        /// <param name="defaut">Valeur par défaut si la clé recherchée n'existe pas.</param>
        /// <returns>Valeur du paramètre.</returns>
        public string GetPathWithoutCreating(string section, string key, string defaut)
        {
            string path = GetValueWithoutCreating(section, key, defaut);

            try
            {
                if (path.StartsWith("."))
                {
                    path = Path.GetFullPath(Path.Combine(Application.StartupPath, path));
                }
                else
                {
                    path = Path.GetFullPath(path);
                }
            }
            catch
            {
                path = "";
            }


            return path;
        }

        /// <summary>
        ///  Indexeur des sections
        /// </summary>
        /// <param name="section">le nom de la section à récupéré</param>
        /// <returns>la section correspondante</returns>
        protected Section this[string section]
        {
            get
            {
                if (!Sections.ContainsKey(section))
                    AddSection(section);

                return (Section)Sections[section];
            }
            set
            {
                if (!Sections.ContainsKey(section))
                    AddSection(section);
                Sections[section] = value;
            }
        }

        /// <summary>
        /// Sauvegarde le fichier INI en cours
        /// </summary>
        /// <returns><c>true</c> si la sauvegarde a réussi <c>false</c> sinon.</returns>
        public virtual bool Save()
        {
            if (sFileName != "")
                return Save(sFileName);

            return false;
        }

        /// <summary>
        /// Sauvegarde le fichier INI sous un nom spécifique
        /// </summary>
        /// <param name="fileName">Nom de fichier</param>
        /// <returns><c>true</c> si la sauvegarde a réussi <c>false</c> sinon.</returns>
        public bool Save(string fileName)
        {
            if (this.readOnly)
            {
                MessageBox.Show("Erreur le fichier : \"" + fileName + "\" à été ouvert en lecture seule");
                return false;
            }
            else
            {
                IniFileSavingEventArgs tmpEventArg = new IniFileSavingEventArgs();

                // Génère l'évènement qui avertit que le fichier est sur le point d'être sauvé.
                OnIniFileSaving(tmpEventArg);

                // Si la sauvegarde est annulée.
                if (tmpEventArg.Cancel)
                    return false;


                Monitor.Enter(lockSave);

                try
                {
                    if (File.Exists(fileName))
                        File.Copy(fileName, fileName + "~.tmp");

                    File.WriteAllText(fileName, this.ToString(), Encoding.UTF8);

                    // Génère l'évènement qui informe que le fichier ini a été sauvé.
                    OnIniFileSaved(new EventArgs());

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Erreurs lors de la sauvegarde\n" + e.Message);
                    return false;
                }
                finally
                {
                    try
                    {
                        File.Delete(fileName + "~.tmp");
                    }
                    catch { }

                    Monitor.Exit(lockSave);
                }
            }
        }

        public override string ToString()
        {
            string fichier = fileHeaderCommentsOrEmptyLines;
            bool first = true;

            //
            // Sauver dans l'ordre spécifié par la liste ListSections
            //
            foreach (object okey in ListSections)
            {
                if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + okey.ToString() + "]") && !beforeCodeCommentsOrEmptyLines["[" + okey.ToString() + "]"].Replace(" ", "").Replace("\t", "").Equals(""))
                {
                    fichier += beforeCodeCommentsOrEmptyLines["[" + okey.ToString() + "]"];
                }
                else if (!first)
                {
                    fichier += newline;
                }

                first = false;

                if (linkedComments.ContainsKey("[" + okey.ToString() + "]"))
                {
                    fichier += "[" + okey.ToString() + "] ;" + linkedComments["[" + okey.ToString() + "]"] + newline;
                }
                else
                {
                    fichier += "[" + okey.ToString() + "]" + newline;
                }

                Section sct = (Section)Sections[okey.ToString()];

                foreach (string key in (sct.Keys))
                {
                    string value = sct[key];

                    if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + okey.ToString() + "]" + key))
                    {
                        fichier += beforeCodeCommentsOrEmptyLines["[" + okey.ToString() + "]" + key];
                    }

                    if (sct.IsQuoted[key])
                    {
                        value = "\"" + value + "\"";
                    }

                    if (linkedComments.ContainsKey("[" + okey.ToString() + "]" + key))
                    {
                        fichier += key + "=" + value + " ;" + linkedComments["[" + okey.ToString() + "]" + key] + newline;
                    }
                    else
                    {
                        fichier += key + "=" + value + newline;
                    }
                }
            }

            fichier += fileFooterCommentsOrEmptyLines;

            // Pour que la fin du fichier ne soit pas de plus en plus de retour à la ligne
            char[] removeList = { '\n', '\r', '\t', ' ' };

            fichier = fichier.TrimEnd(removeList);

            return fichier;
        }

        /// <summary>
        /// Récupère le commentaire dans le fichier ini avant la clé spécifiée.
        /// </summary>
        /// <param name="section">La section de la clé</param>
        /// <param name="key">La clé dont on veut connaitre le commentaire</param>
        /// <returns>Le commentaire correspondant</returns>
        public string GetCommentBeforeKey(string section, string key)
        {
            string result = "";

            if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + section + "]" + key))
            {
                result = beforeCodeCommentsOrEmptyLines["[" + section + "]" + key].Trim();
            }

            return result;
        }

        /// <summary>
        /// Récupère le commentaire dans le fichier ini avant la section spécifiée.
        /// </summary>
        /// <param name="section">La section dont on veut connaitre le commentaire</param>
        /// <returns>Le commentaire correspondant</returns>
        public string GetCommentBeforeSection(string section)
        {
            string result = "";

            if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + section + "]"))
            {
                result = beforeCodeCommentsOrEmptyLines["[" + section + "]"].Trim();
            }

            return result;
        }

        /// <summary>
        /// Récupère le commentaire dans le fichier ini après la clé spécifiée.
        /// </summary>
        /// <param name="section">La section de la clé</param>
        /// <param name="key">La clé dont on veut connaitre le commentaire</param>
        /// <returns>Le commentaire correspondant</returns>
        public string GetComment(string section, string key)
        {
            string result = "";

            if (linkedComments.ContainsKey("[" + section + "]" + key))
            {
                result = linkedComments["[" + section + "]" + key].Trim();
            }

            return result;
        }

        /// <summary>
        /// Récupère le commentaire dans le fichier ini après la section spécifiée.
        /// </summary>
        /// <param name="section">La section dont on veut connaitre le commentaire</param>
        /// <returns>Le commentaire correspondant</returns>
        public string GetComment(string section)
        {
            string result = "";

            if (linkedComments.ContainsKey("[" + section + "]"))
            {
                result = linkedComments["[" + section + "]"].Trim();
            }

            return result;
        }

        /// <summary>
        /// Attribue un commentaire dans le fichier ini avant la clé spécifiée.
        /// </summary>
        /// <param name="section">La section de la clé</param>
        /// <param name="key">La clé que l'on veut commenter</param>
        /// <param name="comment">Le commentaire</param>
        public void SetCommentBeforeKey(string section, string key, string comment)
        {
            string oldComment = GetComment(section, key);

            CommentChangingEventArgs tmpArgs = new CommentChangingEventArgs(section, key, oldComment, comment, true);

            OnCommentChanging(tmpArgs);

            if (!tmpArgs.Cancel)
            {
                linkedComments["[" + section + "]" + key] = comment;

                OnCommentChanged(new CommentChangedEventArgs(section, key, oldComment, comment, true));
            }
        }

        /// <summary>
        /// Attribue un commentaire dans le fichier ini avant la section spécifiée
        /// </summary>
        /// <param name="section">La section que l'on veut commenter</param>
        /// <param name="comment">Le commentaire</param>
        public void SetCommentBeforeSection(string section, string comment)
        {
            string oldComment = GetComment(section);

            CommentChangingEventArgs tmpArgs = new CommentChangingEventArgs(section, "", oldComment, comment, true);

            OnCommentChanging(tmpArgs);

            if (!tmpArgs.Cancel)
            {
                linkedComments["[" + section + "]"] = comment;

                OnCommentChanged(new CommentChangedEventArgs(section, "", oldComment, comment, true));
            }
        }

        /// <summary>
        /// Attribue un commentaire dans le fichier ini après la clé spécifiée.
        /// </summary>
        /// <param name="section">La section de la clé</param>
        /// <param name="key">La clé que l'on veut commenter</param>
        /// <param name="comment">Le commentaire</param>
        public void SetComment(string section, string key, string comment)
        {
            string oldComment = GetComment(section, key);

            CommentChangingEventArgs tmpArgs = new CommentChangingEventArgs(section, key, oldComment, comment, false);

            OnCommentChanging(tmpArgs);

            if (!tmpArgs.Cancel)
            {
                linkedComments["[" + section + "]" + key] = comment;

                OnCommentChanged(new CommentChangedEventArgs(section, key, oldComment, comment, false));
            }
        }

        /// <summary>
        /// Attribue un commentaire dans le fichier ini après la section spécifiée
        /// </summary>
        /// <param name="section">La section que l'on veut commenter</param>
        /// <param name="comment">Le commentaire</param>
        public void SetComment(string section, string comment)
        {
            string oldComment = GetComment(section);

            CommentChangingEventArgs tmpArgs = new CommentChangingEventArgs(section, "", oldComment, comment, false);

            OnCommentChanging(tmpArgs);

            if (!tmpArgs.Cancel)
            {
                linkedComments["[" + section + "]"] = comment;

                OnCommentChanged(new CommentChangedEventArgs(section, "", oldComment, comment, false));
            }
        }

        /// <summary>
        /// Attribue un commentaire dans le fichier ini après la clé spécifiée.
        /// Si aucun commentaire n'existe déjà pour cette clé
        /// </summary>
        /// <param name="section">La section de la clé</param>
        /// <param name="key">La clé que l'on veut commenter</param>
        /// <param name="comment">Le commentaire</param>
        public void SetCommentIfNotAlreadyExists(string section, string key, string comment)
        {
            string oldComment = GetComment(section, key);

            if (oldComment.Equals(""))
            {
                CommentChangingEventArgs tmpArgs = new CommentChangingEventArgs(section, key, oldComment, comment, false);

                OnCommentChanging(tmpArgs);

                if (!tmpArgs.Cancel)
                {
                    linkedComments["[" + section + "]" + key] = comment;

                    OnCommentChanged(new CommentChangedEventArgs(section, key, oldComment, comment, false));
                }
            }
        }

        /// <summary>
        /// Attribue un commentaire dans le fichier ini après la section spécifiée
        /// Si aucun commentaire n'existe déjà pour cette section
        /// </summary>
        /// <param name="section">La section que l'on veut commenter</param>
        /// <param name="comment">Le commentaire</param>
        public void SetCommentIfNotAlreadyExists(string section, string comment)
        {
            string oldComment = GetComment(section);

            if (oldComment.Equals(""))
            {
                CommentChangingEventArgs tmpArgs = new CommentChangingEventArgs(section, "", oldComment, comment, false);

                OnCommentChanging(tmpArgs);

                if (!tmpArgs.Cancel)
                {
                    linkedComments["[" + section + "]"] = comment;

                    OnCommentChanged(new CommentChangedEventArgs(section, "", oldComment, comment, false));
                }
            }
        }

        /// <summary>
        /// Envoie la clé spécifiée si elle existe au début de la section
        /// </summary>
        /// <param name="section">La section</param>
        /// <param name="key">La clé a bouger</param>
        public void MakeKeyFirstOfSection(string section, string key)
        {
            Sections[section].MakeKeyFirst(key);
        }

        /// <summary>
        /// Envoie la clé spécifiée si elle existe à la fin de la section
        /// </summary>
        /// <param name="section">La section</param>
        /// <param name="key">La clé a bouger</param>
        public void MakeKeyLastOfSection(string section, string key)
        {
            Sections[section].MakeKeyLast(key);
        }

        /// <summary>
        /// Enlève les espaces et tabulation au début et à la fin d'un text
        /// </summary>
        /// <param name="spacedText">texte à stripé</param>
        /// <returns>le texte stripé.</returns>
        protected string SpacesStrip(string spacedText)
        {
            string result = spacedText;

            while (result.EndsWith(" ") || result.EndsWith("\t"))
            {
                result = result.Substring(0, result.ToCharArray().Length - 1);
            }

            while (result.StartsWith(" ") || result.StartsWith("\t"))
            {
                result = result.Substring(1, result.ToCharArray().Length - 1);
            }

            return result;
        }

        /// <summary>
        /// Tente de renommer une clé dans une section
        /// </summary>
        /// <param name="section">la section de la clé à renommer</param>
        /// <param name="currentKeyName">le nom courrant de la clé à renommer</param>
        /// <param name="newKeyName">le nouveau nom à donné à la clé</param>
        /// <returns><c>true</c> si  le renommage s'est bien passé, <c>false</c> si le renommage n'est pas possible</returns>
        public bool RenameKey(string section, string currentKeyName, string newKeyName)
        {
            bool result = false;

            if (Sections.ContainsKey(section))
            {
                Section sectionOfKey = Sections[section];

                if (sectionOfKey.KeyExists(currentKeyName) && !sectionOfKey.KeyExists(newKeyName))
                {
                    //
                    // Renommage de la clé
                    //

                    sectionOfKey.SetKey(newKeyName, sectionOfKey[currentKeyName]);
                    sectionOfKey.DeleteKey(currentKeyName);

                    //
                    // Suivi des commentaires
                    //

                    if (linkedComments.ContainsKey("[" + section + "]" + currentKeyName))
                    {
                        string comment = linkedComments["[" + section + "]" + currentKeyName];

                        linkedComments.Remove("[" + section + "]" + currentKeyName);
                        linkedComments["[" + section + "]" + newKeyName] = comment;
                    }

                    if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + section + "]" + currentKeyName))
                    {
                        string comment = beforeCodeCommentsOrEmptyLines["[" + section + "]" + currentKeyName];

                        beforeCodeCommentsOrEmptyLines.Remove("[" + section + "]" + currentKeyName);
                        beforeCodeCommentsOrEmptyLines["[" + section + "]" + newKeyName] = comment;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Supprimer une Key dans une section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        public void DeleteKey(string section, string key)
        {
            Sections[section].DeleteKey(key);

            try
            {
                if (beforeCodeCommentsOrEmptyLines.ContainsKey("[" + section + "]" + key))
                    beforeCodeCommentsOrEmptyLines.Remove("[" + section + "]" + key);

                if (linkedComments.ContainsKey("[" + section + "]" + key))
                    linkedComments.Remove("[" + section + "]" + key);
            }
            catch { }
        }

        /// <summary>
        /// Recharge le fichier INI (en utf-8)
        /// </summary>
        public virtual void Reload()
        {
            // Efface tout
            this.RemoveAllSections();
            this.linkedComments.Clear();
            this.beforeCodeCommentsOrEmptyLines.Clear();
            this.fileHeaderCommentsOrEmptyLines = "";
            this.fileFooterCommentsOrEmptyLines = "";
            this.loaded = false;
            OnIniFileLoadedStateChanged();

            // Recharge 
            this.Load(this.FileName);
        }

        /// <summary>
        /// Charge un fichier INI (en utf-8)
        /// </summary>
        /// <param name="fileName">Nom du fichier à charger</param>
        public void Load(string fileName)
        {
            Load(fileName, Encoding.UTF8);
        }

        /// <summary>
        /// Libère les données du fichier d'ini et décharge le fichier courant.
        /// </summary>
        public void UnLoad()
        {
            // Efface tout
            this.RemoveAllSections();
            this.linkedComments.Clear();
            this.beforeCodeCommentsOrEmptyLines.Clear();
            this.fileHeaderCommentsOrEmptyLines = "";
            this.fileFooterCommentsOrEmptyLines = "";
            this.FileName = "";
            this.loaded = false;
            OnIniFileLoadedStateChanged();
        }

        /// <summary>
        /// Charge un fichier INI
        /// </summary>
        /// <param name="fileName">Nom du fichier à charger</param>
        /// <param name="encoding">L'encodage à utiliser pour charger le fichier</param>
        public void Load(string fileName, Encoding encoding)
        {
            // variable qui va prendre le nom de la section courante du fichier ini
            string currentSection = "";
            string currentKey = "";

            try
            {
                StreamReader str;

                if (this.readOnly)
                {
                    // Ouverture en lecture seule.
                    str = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), encoding);
                }
                else
                {
                    // Ouverture avec possibilité de resauvegarder les données.
                    str = new StreamReader(File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite), encoding);
                }

                string fichier = str.ReadToEnd();

                // split le texte du fichier en tableau de ligne
                string[] lignes = fichier.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                bool fileHeaderPossible = true;
                string currentCommentOrEmptyLines = "";

                fileHeaderCommentsOrEmptyLines = "";
                fileFooterCommentsOrEmptyLines = "";

                // Parcours des lignes du fichier
                for (int i = 0; i < lignes.Length; i++)
                {
                    // splitte la ligne en 2 Code effectif/Commentaire
                    string[] splittedCommentLine = SplitComments(lignes[i]);

                    //récupère la partie effective de la ligne
                    string ligne = SpacesStrip(splittedCommentLine[0]);

                    // si la ligne déclare une nouvelle section
                    if (ligne.StartsWith("[") && ligne.EndsWith("]"))
                    {
                        // Si la ligne possède un commentaire, on sauvegarde le commentaire pour la section.
                        if (splittedCommentLine.Length > 1)
                            linkedComments.Add(ligne, splittedCommentLine[1]);

                        // Sauvegarde les commentaires et ligne vides se trouvant avant cette section
                        beforeCodeCommentsOrEmptyLines.Add(ligne, currentCommentOrEmptyLines);

                        // Créé la section en mémoire avec son nom
                        currentSection = ligne.Substring(1, ligne.Length - 2);
                        AddSection(currentSection);

                        currentCommentOrEmptyLines = "";
                        fileHeaderPossible = false;
                    }
                    else if (ligne != "" && !currentSection.Equals(""))
                    {
                        // split la ligne en tableau clé/valeur
                        char[] ca = new char[1] { '=' };
                        string[] scts = ligne.Split(ca, 2);

                        currentKey = scts[0];

                        // Si la ligne possède un commentaire, on sauvegarde le commentaire pour la clé de la section courante.
                        if (splittedCommentLine.Length > 1)
                            linkedComments.Add("[" + currentSection + "]" + scts[0], splittedCommentLine[1]);

                        // Sauvegarde les commentaires et ligne vides se trouvant avant cette clé.
                        beforeCodeCommentsOrEmptyLines.Add("[" + currentSection + "]" + scts[0], currentCommentOrEmptyLines);

                        if (scts.Length == 2)
                        {
                            // Crée l'association clé/valeur en mémoire.
                            this[currentSection].SetKey(currentKey, scts[1]);
                        }

                        currentCommentOrEmptyLines = "";
                        fileHeaderPossible = false;
                    }
                    else
                    {
                        // Récupère les ligne vides ou de commentaire simple entre les clés et/ou les sections.
                        currentCommentOrEmptyLines += lignes[i] + newline;

                        if (fileHeaderPossible && lignes[i].Replace(" ", "").Replace("\t", "").Equals(""))
                        {
                            fileHeaderCommentsOrEmptyLines = currentCommentOrEmptyLines;
                            currentCommentOrEmptyLines = "";
                            fileHeaderPossible = false;
                        }
                    }
                }

                // Mémorise les éventuels commentaire en fin de fichier
                fileFooterCommentsOrEmptyLines = currentCommentOrEmptyLines;

                // mémorise le chemin d'accès au fichier
                this.sFileName = fileName;

                str.Close();
                str.Dispose();
                str = null;

                loaded = true;
                OnIniFileLoadedStateChanged();
            }
            catch (Exception e)
            {
                throw new Exception("Impossible de charger le fichier : \"" + fileName + "\"\nDes erreurs sont survenues.\n" + e.Message + "\nDernière section : " + currentSection + "\nDernière clé : " + currentKey, e);
            }
        }

        public void LoadFromString(string iniString)
        {
            string currentSection = "";
            string currentKey = "";

            try
            {
                string fichier = iniString;

                // split le texte du fichier en tableau de ligne
                string[] lignes = fichier.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                bool fileHeaderPossible = true;
                string currentCommentOrEmptyLines = "";

                fileHeaderCommentsOrEmptyLines = "";
                fileFooterCommentsOrEmptyLines = "";

                // Parcours des lignes du fichier
                for (int i = 0; i < lignes.Length; i++)
                {
                    // splitte la ligne en 2 Code effectif/Commentaire
                    string[] splittedCommentLine = SplitComments(lignes[i]);

                    //récupère la partie effective de la ligne
                    string ligne = SpacesStrip(splittedCommentLine[0]);

                    // si la ligne déclare une nouvelle section
                    if (ligne.StartsWith("[") && ligne.EndsWith("]"))
                    {
                        // Si la ligne possède un commentaire, on sauvegarde le commentaire pour la section.
                        if (splittedCommentLine.Length > 1)
                            linkedComments.Add(ligne, splittedCommentLine[1]);

                        // Sauvegarde les commentaires et ligne vides se trouvant avant cette section
                        beforeCodeCommentsOrEmptyLines.Add(ligne, currentCommentOrEmptyLines);

                        // Créé la section en mémoire avec son nom
                        currentSection = ligne.Substring(1, ligne.Length - 2);
                        AddSection(currentSection);

                        currentCommentOrEmptyLines = "";
                        fileHeaderPossible = false;
                    }
                    else if (ligne != "" && !currentSection.Equals(""))
                    {
                        // split la ligne en tableau clé/valeur
                        char[] ca = new char[1] { '=' };
                        string[] scts = ligne.Split(ca, 2);

                        currentKey = scts[0];

                        // Si la ligne possède un commentaire, on sauvegarde le commentaire pour la clé de la section courante.
                        if (splittedCommentLine.Length > 1)
                            linkedComments.Add("[" + currentSection + "]" + scts[0], splittedCommentLine[1]);

                        // Sauvegarde les commentaires et ligne vides se trouvant avant cette clé.
                        beforeCodeCommentsOrEmptyLines.Add("[" + currentSection + "]" + scts[0], currentCommentOrEmptyLines);

                        if (scts.Length == 2)
                        {
                            // Crée l'association clé/valeur en mémoire.
                            this[currentSection].SetKey(currentKey, scts[1]);
                        }

                        currentCommentOrEmptyLines = "";
                        fileHeaderPossible = false;
                    }
                    else
                    {
                        // Récupère les ligne vides ou de commentaire simple entre les clés et/ou les sections.
                        currentCommentOrEmptyLines += lignes[i] + newline;

                        if (fileHeaderPossible && lignes[i].Replace(" ", "").Replace("\t", "").Equals(""))
                        {
                            fileHeaderCommentsOrEmptyLines = currentCommentOrEmptyLines;
                            currentCommentOrEmptyLines = "";
                            fileHeaderPossible = false;
                        }
                    }
                }

                // Mémorise les éventuels commentaire en fin de fichier
                fileFooterCommentsOrEmptyLines = currentCommentOrEmptyLines;
            }
            catch (Exception e)
            {
                throw new Exception("Impossible de charger le chaine ini \nDes erreurs sont survenues.\n" + e.Message + "\nDernière section : " + currentSection + "\nDernière clé : " + currentKey, e);
            }
        }

        /// <summary>
        /// Sépare la partie commentaire de la partie code de fichier ini d'une ligne.
        /// </summary>
        /// <param name="textLine">le text à splitté</param>
        /// <returns>le tableau de text [code,commentaire]</returns>
        protected string[] SplitComments(string textLine)
        {
            bool inTextValue = false;
            int commentCaractPos = -1;

            for (int i = 0; i < textLine.Length; i++)
            {
                char currentChar = textLine.ToCharArray()[i];

                if (currentChar == '"')
                {
                    inTextValue = !inTextValue;
                }

                if (currentChar == ';' && !inTextValue)
                {
                    commentCaractPos = i;
                    break;
                }
            }

            if (commentCaractPos > -1)
            {
                return new string[] { textLine.Substring(0, commentCaractPos), textLine.Substring(commentCaractPos + 1, textLine.Length - (commentCaractPos + 1)) };
            }
            else
            {
                return new string[] { textLine };
            }
        }

        /// <summary>
        /// Récupère un dictionnaire clé valeur pour la section spécifiée.
        /// </summary>
        /// <param name="section">nom de la section dont on veut récupérer un dictionnaire</param>
        /// <returns>Le dictionnaire de toutes les paires clés valeurs de la section</returns>
        public Dictionary<string, string> GetDictOfSection(string section)
        {
            Dictionary<string, string> resultDict;
            if (!Sections.ContainsKey(section))
                resultDict = new Dictionary<string, string>();
            else
            {
                Section currentSection = Sections[section] as Section;
                resultDict = currentSection.GetKeys();
            }

            return resultDict;
        }

        /// <summary>
        /// Structure de donnée des sections
        /// </summary>
        protected class Section
        {
            /// <summary>
            /// Dictionnaire des paires clefs valeurs de la section
            /// </summary>
            private Dictionary<string, string> keys = new Dictionary<string, string>();

            /// <summary>
            ///Dictionnaire des valeur en string true si la valeur est une string false sinon.
            /// </summary>
            public Dictionary<string, bool> IsQuoted = new Dictionary<string, bool>();

            /// <summary>
            /// Retourne la référence au dictionnaire des clé de la section
            /// </summary>
            /// <returns>Une référence au dictionnaire de la section</returns>
            public Dictionary<string, string> GetKeys()
            {
                return keys;
            }

            /// <summary>
            /// Constructeur de la classe imbriquée Section
            /// </summary>
            public Section() { }

            /// <summary>
            /// Méthode interne pour enlever les " au début et à la fin des valeurs
            /// </summary>
            /// <param name="quotedText">Le texte potentiellement double quoté</param>
            /// <param name="key">La clé lié à la valeur quotedText</param>
            /// <returns>Le texte débarrassé de ses double quotes.</returns>
            private string DoubleQuotesStrip(string quotedText, string key)
            {
                string result = quotedText;

                if (result.Equals(" "))
                    IsQuoted[key] = true;
                else
                {
                    while (result.EndsWith(" ") || result.EndsWith("\t"))
                    {
                        result = result.Substring(0, result.ToCharArray().Length - 1);
                    }

                    while (result.StartsWith(" ") || result.StartsWith("\t"))
                    {
                        result = result.Substring(1, result.ToCharArray().Length - 1);
                    }
                }

                if (result.StartsWith("\"") && result.EndsWith("\"") && !result.Equals("\""))
                {
                    result = result.Substring(1, result.ToCharArray().Length - 2);

                    if (!IsQuoted.ContainsKey(key) || result.Contains(";"))
                        IsQuoted[key] = true;
                }
                else
                {
                    if (!IsQuoted.ContainsKey(key))
                    {
                        if (result.Contains(";"))
                            IsQuoted[key] = true;
                        else
                            IsQuoted[key] = false;
                    }
                    else if (result.Contains(";"))
                        IsQuoted[key] = true;
                }

                return result;
            }

            /// <summary>
            /// Affecte une valeur à une clef et la crée si elle n'existe pas
            /// </summary>
            /// <param name="key">Nom de la clef</param>
            /// <param name="value">Valeur de la clef</param>
            public void SetKey(string key, string value)
            {
                if (key.IndexOf("=") > 0)
                    throw new Exception("Caractère '=' interdit");

                if (keys.ContainsKey(key))
                    keys[key] = DoubleQuotesStrip(value, key);
                else
                    keys.Add(key, DoubleQuotesStrip(value, key));
            }

            /// <summary>
            /// Supprime une clefs
            /// </summary>
            /// <param name="key">Nom de la clef à supprimer</param>
            public void DeleteKey(string key)
            {
                if (keys.ContainsKey(key))
                    keys.Remove(key);
            }

            /// <summary>
            /// Les clefs contenues dans la section
            /// </summary>
            public ICollection Keys
            {
                get
                {
                    return keys.Keys;
                }
            }

            public void MakeKeyFirst(string key)
            {
                if (keys.ContainsKey(key))
                {
                    List<KeyValuePair<string, string>> list = keys.ToList();
                    KeyValuePair<string, string> keyValuePair = list.Find(e => e.Key.Equals(key));
                    list.Remove(keyValuePair);
                    list.Insert(0, keyValuePair);
                    keys = list.ToDictionary(p => p.Key, p => p.Value);
                }
            }

            public void MakeKeyLast(string key)
            {
                if (keys.ContainsKey(key))
                {
                    List<KeyValuePair<string, string>> list = keys.ToList();
                    KeyValuePair<string, string> keyValuePair = list.Find(e => e.Key.Equals(key));
                    list.Remove(keyValuePair);
                    list.Add(keyValuePair);
                    keys = list.ToDictionary(p => p.Key, p => p.Value);
                }
            }

            /// <summary>
            /// Vérifie si la clé spécifiée existe dans la section
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public bool KeyExists(string key)
            {
                return keys.ContainsKey(key);
            }

            /// <summary>
            /// Indexeur des clefs
            /// </summary>
            public string this[string key]
            {
                get
                {
                    if (keys.ContainsKey(key))
                        return keys[key];
                    else
                    {
                        SetKey(key, "");
                        return "";
                    }
                }
                set
                {
                    SetKey(key, value);
                }
            }
        }

        /// <summary>
        /// Classe d'argument pour l'évènement ValueChanged de la classe IniFile
        /// </summary>
        public class ValueChangedEventArgs : EventArgs
        {
            private string section;
            private string key;
            private string oldValue;
            private string newValue;

            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="section">Nom de la section où la valeur à changé</param>
            /// <param name="key">Nom de la clé où la valeur à changé</param>
            /// <param name="oldValue">La valeur avant le changement</param>
            /// <param name="newValue">La valeur après le changement</param>
            public ValueChangedEventArgs(string section, string key, string oldValue, string newValue)
            {
                this.section = section;
                this.key = key;
                this.oldValue = oldValue;
                this.newValue = newValue;
            }

            /// <summary>
            /// Nom de la section où la valeur à changé
            /// </summary>
            public string Section
            {
                get { return section; }
            }

            /// <summary>
            /// Nom de la clé où la valeur à changé
            /// </summary>
            public string Key
            {
                get { return key; }
            }

            /// <summary>
            /// La valeur avant le changement
            /// </summary>
            public string OldValue
            {
                get { return oldValue; }
            }

            /// <summary>
            /// La valeur après le changement
            /// </summary>
            public string NewValue
            {
                get { return newValue; }
            }
        }

        /// <summary>
        /// Classe d'argument pour l'évènement ValueChanging de la classe IniFile
        /// </summary>
        public class ValueChangingEventArgs : EventArgs
        {
            private string section;
            private string key;
            private string oldValue;
            private string newValue;

            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="section">Nom de la section où la valeur à changé</param>
            /// <param name="key">Nom de la clé où la valeur à changé</param>
            /// <param name="oldValue">La valeur avant le changement</param>
            /// <param name="newValue">La valeur après le changement</param>
            public ValueChangingEventArgs(string section, string key, string oldValue, string newValue)
            {
                this.section = section;
                this.key = key;
                this.oldValue = oldValue;
                this.newValue = newValue;
            }

            /// <summary>
            /// Nom de la section où la valeur à changé
            /// </summary>
            public string Section
            {
                get { return section; }
            }

            /// <summary>
            /// Nom de la clé où la valeur à changé
            /// </summary>
            public string Key
            {
                get { return key; }
            }

            /// <summary>
            /// La valeur avant le changement
            /// </summary>
            public string OldValue
            {
                get { return oldValue; }
            }

            /// <summary>
            /// La valeur après le changement
            /// </summary>
            public string NewValue
            {
                get { return newValue; }
            }

            /// <summary>
            /// Obtient ou définit une valeur boolean qui définit si le changement
            /// doit-être annuler(<c>true</c>) ou être maintenue (<c>false</c>).
            /// </summary>
            public bool Cancel { get; set; }
        }

        /// <summary>
        /// Classe d'argument pour l'évènement CommentChanged de la classe IniFile
        /// </summary>
        public class CommentChangedEventArgs : EventArgs
        {
            private string section;
            private string key;
            private string oldComment;
            private string newComment;
            private bool isCommentBeforeKeyOrSection;

            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="section">Nom de la section où le commentaire à changé</param>
            /// <param name="key">Nom de la clé où la valeur à changé</param>
            /// <param name="oldComment">Le commentaire avant le changement</param>
            /// <param name="newComment">Le commentaire après le changement</param>
            public CommentChangedEventArgs(string section, string key, string oldComment, string newComment, bool isCommentBeforeKeyOrSection)
            {
                this.section = section;
                this.key = key;
                this.oldComment = oldComment;
                this.newComment = newComment;
                this.isCommentBeforeKeyOrSection = isCommentBeforeKeyOrSection;
            }

            /// <summary>
            /// Nom de la section où la valeur à changé
            /// </summary>
            public string Section
            {
                get { return section; }
            }

            /// <summary>
            /// Nom de la clé où la valeur à changé
            /// </summary>
            public string Key
            {
                get { return key; }
            }

            /// <summary>
            /// Le commentaire avant le changement
            /// </summary>
            public string OldComment
            {
                get { return oldComment; }
            }

            /// <summary>
            /// Le commentaire après le changement
            /// </summary>
            public string NewComment
            {
                get { return newComment; }
            }

            /// <summary>
            /// Si <c>true</c> C'est un commentaire placé avant la clé ou la section spécifiée.
            /// Si <c>false</c> C'est un commentaire placé après sur la même ligne que la clé ou la section spécifiée.
            /// </summary>
            public bool IsCommentBeforeKeyOrSection
            {
                get
                {
                    return isCommentBeforeKeyOrSection;
                }
            }
        }

        /// <summary>
        /// Classe d'argument pour l'évènement CommentChanging de la classe IniFile
        /// </summary>
        public class CommentChangingEventArgs : EventArgs
        {
            private string section;
            private string key;
            private string oldComment;
            private string newComment;
            private bool isCommentBeforeKeyOrSection;


            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="section">Nom de la section où le commentaire à changé</param>
            /// <param name="key">Nom de la clé où la valeur à changé</param>
            /// <param name="oldComment">Le commentaire avant le changement</param>
            /// <param name="newComment">Le commentaire après le changement</param>
            public CommentChangingEventArgs(string section, string key, string oldComment, string newComment, bool isCommentBeforeKeyOrSection)
            {
                this.section = section;
                this.key = key;
                this.oldComment = oldComment;
                this.newComment = newComment;
                this.isCommentBeforeKeyOrSection = isCommentBeforeKeyOrSection;
            }

            /// <summary>
            /// Nom de la section où la valeur à changé
            /// </summary>
            public string Section
            {
                get { return section; }
            }

            /// <summary>
            /// Nom de la clé où la valeur à changé
            /// </summary>
            public string Key
            {
                get { return key; }
            }

            /// <summary>
            /// Le commentaire avant le changement
            /// </summary>
            public string OldComment
            {
                get { return oldComment; }
            }

            /// <summary>
            /// Le commentaire après le changement
            /// </summary>
            public string NewComment
            {
                get { return newComment; }
            }

            /// <summary>
            /// Obtient ou définit une valeur boolean qui définit si le changement
            /// doit-être annuler(<c>true</c>) ou être maintenue (<c>false</c>).
            /// </summary>
            public bool Cancel { get; set; } = false;

            /// <summary>
            /// Si <c>true</c> C'est un commentaire placé avant la clé ou la section spécifiée.
            /// Si <c>false</c> C'est un commentaire placé après sur la même ligne que la clé ou la section spécifiée.
            /// </summary>
            public bool IsCommentBeforeKeyOrSection
            {
                get
                {
                    return isCommentBeforeKeyOrSection;
                }
            }
        }

        /// <summary>
        /// Classe d'argument pour l'évènement IniFileSaving de la classe IniFile
        /// </summary>
        public class IniFileSavingEventArgs : EventArgs
        {
            /// <summary>
            /// Constructeur
            /// </summary>
            public IniFileSavingEventArgs()
            { }

            /// <summary>
            /// Obtient ou définit une valeur boolean qui définit si la sauvegarde
            /// doit-être annuler(<c>true</c>) ou être maintenue (<c>false</c>).
            /// </summary>
            public bool Cancel { get; set; } = false;
        }
    }
}
