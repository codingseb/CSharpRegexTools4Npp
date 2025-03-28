using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Microsoft.CodeAnalysis.Tags;

namespace RegexDialog.Services
{
    public class RoslynService
    {
        private readonly MetadataReference[] _references;
        private readonly CSharpCompilationOptions _compilationOptions;

        // Pour le debugging
        private bool _isDebugMode = true;

        public RoslynService()
        {
            // Charger toutes les références nécessaires
            _references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Regex).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Match).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StringBuilder).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ObservableCollection<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(CultureInfo).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
            };

            _compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug,
                allowUnsafe: true);

            LogDebug("RoslynService initialized");
        }

        public async Task<IEnumerable<Model.CompletionData>> GetCompletionItemsAsync(
            string editorContent, int position, string templateCode)
        {
            try
            {
                LogDebug($"GetCompletionItemsAsync called with position {position}");

                // Utiliser le template fourni et injecter le code de l'utilisateur
                string codeToAnalyze = templateCode.Replace("//code", editorContent);

                // Calculer la position ajustée
                int codeMarkerPosition = templateCode.IndexOf("//code");
                int adjustedPosition = codeMarkerPosition + position;

                LogDebug($"Code marker position: {codeMarkerPosition}");
                LogDebug($"Adjusted position: {adjustedPosition}");
                LogDebug($"Code to analyze: {codeToAnalyze}");

                // Créer le workspace pour l'analyse
                using var workspace = new AdhocWorkspace();
                var projectId = ProjectId.CreateNewId();
                var projectInfo = ProjectInfo.Create(
                    projectId, VersionStamp.Create(), "CompletionProject", "CompletionProject",
                    LanguageNames.CSharp, compilationOptions: _compilationOptions,
                    metadataReferences: _references);

                var project = workspace.AddProject(projectInfo);

                // Ajouter le document avec le code à analyser
                var sourceText = SourceText.From(codeToAnalyze);

                // Correction : utiliser le projet.Id au lieu de documentId
                var document = workspace.AddDocument(project.Id, "Completion.cs", sourceText);

                // Obtenir le service de complétion
                var completionService = CompletionService.GetService(document);
                if (completionService == null)
                {
                    LogDebug("CompletionService is null");
                    return Enumerable.Empty<Model.CompletionData>();
                }

                // Obtenir les suggestions de complétion
                var completionList = await completionService.GetCompletionsAsync(document, adjustedPosition);

                if (completionList == null)
                {
                    LogDebug("CompletionList is null");
                    return Enumerable.Empty<Model.CompletionData>();
                }

                // Correction : utiliser ItemsList au lieu de Items (obsolète)
                LogDebug($"Found {completionList.ItemsList.Count} completion items");

                // Convertir en notre modèle de données
                var result = new List<Model.CompletionData>();

                // Correction : utiliser ItemsList au lieu de Items
                foreach (var item in completionList.ItemsList)
                {
                    var description = await completionService.GetDescriptionAsync(document, item);
                    result.Add(new Model.CompletionData
                    {
                        Text = item.DisplayText,
                        Description = description?.Text,
                        Kind = GetCompletionItemKind(item.Tags)
                    });
                }

                LogDebug($"Returning {result.Count} completion items");
                return result;
            }
            catch (Exception ex)
            {
                LogDebug($"Error in GetCompletionItemsAsync: {ex.Message}");
                LogDebug(ex.StackTrace);
                return Enumerable.Empty<Model.CompletionData>();
            }
        }

        private Model.CompletionItemKind GetCompletionItemKind(ImmutableArray<string> tags)
        {
            if (tags.Contains(WellKnownTags.Class))
                return Model.CompletionItemKind.Class;
            if (tags.Contains(WellKnownTags.Method))
                return Model.CompletionItemKind.Method;
            if (tags.Contains(WellKnownTags.ExtensionMethod))
                return Model.CompletionItemKind.Extension;
            if (tags.Contains(WellKnownTags.Property))
                return Model.CompletionItemKind.Property;
            if (tags.Contains(WellKnownTags.Field))
                return Model.CompletionItemKind.Field;
            if (tags.Contains(WellKnownTags.Event))
                return Model.CompletionItemKind.Event;
            if (tags.Contains(WellKnownTags.Interface))
                return Model.CompletionItemKind.Interface;
            if (tags.Contains(WellKnownTags.Enum))
                return Model.CompletionItemKind.Enum;
            if (tags.Contains(WellKnownTags.EnumMember))
                return Model.CompletionItemKind.EnumMember;
            if (tags.Contains(WellKnownTags.Keyword))
                return Model.CompletionItemKind.Keyword;
            if (tags.Contains(WellKnownTags.Namespace))
                return Model.CompletionItemKind.Namespace;
            if (tags.Contains(WellKnownTags.Constant))
                return Model.CompletionItemKind.Namespace;
            if (tags.Contains(WellKnownTags.Structure))
                return Model.CompletionItemKind.Struct;
            if (tags.Contains(WellKnownTags.Local) || tags.Contains(WellKnownTags.Parameter))
                return Model.CompletionItemKind.Variable;
            
            return Model.CompletionItemKind.Other;
        }

        private void LogDebug(string message)
        {
            if (_isDebugMode)
            {
                Debug.WriteLine($"[RoslynService] {message}");
                Console.WriteLine($"[RoslynService] {message}");
            }
        }
    }
}