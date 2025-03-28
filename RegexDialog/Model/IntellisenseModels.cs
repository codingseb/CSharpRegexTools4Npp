using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RegexDialog.Model
{
    public class CompletionData : ICompletionData
    {
        public string Text { get; set; }
        public string Description { get; set; }
        public CompletionItemKind Kind { get; set; }

        public object Content => Text;
        public double Priority => 0;

        object ICompletionData.Description => Description;

        private ImageSource _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = GetImageForKind(Kind);
                }
                return _image;
            }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        private ImageSource GetImageForKind(CompletionItemKind kind)
        {
            try
            {
                // Construire le chemin de l'image en fonction du type
                string imageName = kind.ToString();
                string resourcePath = $"/RegexDialog;component/img/{imageName}.png";

                // Charger l'image depuis les ressources
                var uri = new Uri(resourcePath, UriKind.Relative);
                var resourceStream = Application.GetResourceStream(uri);
                
                if (resourceStream != null)
                {
                    // Créer une BitmapImage à partir du flux
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = resourceStream.Stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Charger en mémoire et fermer le flux
                    bitmap.EndInit();
                    bitmap.Freeze(); // Pour une utilisation thread-safe

                    return bitmap;
                }

                // Si l'image spécifique n'est pas trouvée, essayer de charger une image par défaut
                if (kind != CompletionItemKind.Other)
                {
                    return GetImageForKind(CompletionItemKind.Other);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image for {kind}: {ex.Message}");
                return null;
            }
        }
    }

    public enum CompletionItemKind
    {
        Class,
        Method,
        Extension,
        Property,
        Field,
        Event,
        Interface,
        Enum,
        EnumMember,
        Keyword,
        Namespace,
        Variable,
        Constant,
        Struct,
        Other
    }
}