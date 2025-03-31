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

        // Regex patterns to identify template regions
        private static readonly Regex usingsRegex = new(@"#usings\r?\n(.*?)\r?\n#endusings", RegexOptions.Singleline);
        private static readonly Regex globalRegex = new(@"#global\r?\n(.*?)\r?\n#endglobal", RegexOptions.Singleline);
        private static readonly Regex beforeRegex = new(@"#before\r?\n(.*?)\r?\n#endbefore", RegexOptions.Singleline);
        private static readonly Regex afterRegex = new(@"#after\r?\n(.*?)\r?\n#endafter", RegexOptions.Singleline);
        private static readonly Regex removeAllRegionRegex = new(@"#(?<region>\w+)\r?\n(.*?)\r?\n#end\k<region>", RegexOptions.Singleline);

        // Pour le debugging
        private bool _isDebugMode = true;

        public RoslynService()
        {
            // Charger toutes les références nécessaires
            _references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(File).Assembly.Location),
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

        private string ExtractRegionContent(string editorContent, Regex regionRegex)
        {
            Match match = regionRegex.Match(editorContent);
            return match.Success ? match.Groups[1].Value : "";
        }

        public async Task<IEnumerable<Model.CompletionData>> GetCompletionItemsAsync(
            string editorContent, int position, string templateCode)
        {
            try
            {
                LogDebug($"GetCompletionItemsAsync called with position {position}");

                string codeToAnalyze;
                int adjustedPosition;
                int positionInBloc;

                // Déterminer la région actuelle
                var currentRegion = DetermineRegion(editorContent, position, out positionInBloc);
                LogDebug($"Current region: {currentRegion}");

                // Extraire les parties du code de l'utilisateur selon les régions
                string usingsCode = ExtractRegionContent(editorContent, usingsRegex);
                string globalCode = ExtractRegionContent(editorContent, globalRegex);
                string beforeCode = ExtractRegionContent(editorContent, beforeRegex);
                string afterCode = ExtractRegionContent(editorContent, afterRegex);

                // Le code principal est ce qui n'est pas dans une région spéciale
                string mainCode = removeAllRegionRegex.Replace(editorContent, "");

                // Injecter le code dans le template
                codeToAnalyze = templateCode
                    .Replace("//usings", usingsCode)
                    .Replace("//global", globalCode)
                    .Replace("//code", mainCode)
                    .Replace("//before", beforeCode)
                    .Replace("//after", afterCode);

                LogDebug("Code to analyze created");

                // Calculer la position ajustée
                adjustedPosition = CalculateAdjustedPosition(templateCode, currentRegion, positionInBloc,
                                                          usingsCode, globalCode, beforeCode, afterCode, mainCode);

                LogDebug($"Adjusted position: {adjustedPosition}");

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

        private enum CodeRegion
        {
            Usings,
            Global,
            ReplaceMethod,
            BeforeMethod,
            AfterMethod
        }

        private CodeRegion DetermineRegion(string editorContent, int position, out int positionInBloc)
        {
            // Vérifier si la position est dans une région spéciale
            var usingsMatch = usingsRegex.Match(editorContent);
            if (usingsMatch.Success && IsPositionInMatch(position, usingsMatch))
            {
                positionInBloc = position - usingsMatch.Groups[1].Index;
                return CodeRegion.Usings;
            }

            var globalMatch = globalRegex.Match(editorContent);
            if (globalMatch.Success && IsPositionInMatch(position, globalMatch))
            {
                positionInBloc = position - globalMatch.Groups[1].Index;
                return CodeRegion.Global;
            }

            var beforeMatch = beforeRegex.Match(editorContent);
            if (beforeMatch.Success && IsPositionInMatch(position, beforeMatch))
            {
                positionInBloc = position - beforeMatch.Groups[1].Index;
                return CodeRegion.BeforeMethod;
            }

            var afterMatch = afterRegex.Match(editorContent);
            if (afterMatch.Success && IsPositionInMatch(position, afterMatch))
            {
                positionInBloc = position - afterMatch.Groups[1].Index;
                return CodeRegion.AfterMethod;
            }

            positionInBloc = position - Math.Max(0,removeAllRegionRegex.Matches(editorContent).Cast<Match>().Max(m => m.Index + m.Length));
            // Par défaut, on est dans la méthode Replace
            return CodeRegion.ReplaceMethod;
        }

        private bool IsPositionInMatch(int position, Match match)
        {
            return position >= match.Groups[1].Index &&
                   position <= match.Groups[1].Index + match.Groups[1].Length;
        }

        private int CalculateAdjustedPosition(string template, CodeRegion region, int positionInBlock,
                                            string usingsCode, string globalCode,
                                            string beforeCode, string afterCode, string mainCode)
        {
            int adjustedPosition = 0;

            // Selon la région, trouver la position du placeholder dans le template
            switch (region)
            {
                case CodeRegion.Usings:
                    adjustedPosition = template.IndexOf("//usings") + positionInBlock;
                    break;
                case CodeRegion.Global:
                    adjustedPosition = template
                        .Replace("//usings", usingsCode)
                        .IndexOf("//global") + positionInBlock;
                    break;
                case CodeRegion.BeforeMethod:
                    adjustedPosition = template
                        .Replace("//usings", usingsCode)
                        .Replace("//global", globalCode)
                        .IndexOf("//before") + positionInBlock;
                    break;
                case CodeRegion.AfterMethod:
                    adjustedPosition = template
                        .Replace("//usings", usingsCode)
                        .Replace("//global", globalCode)
                        .Replace("//before", beforeCode)
                        .IndexOf("//after") + positionInBlock;
                    break;
                case CodeRegion.ReplaceMethod:
                    adjustedPosition = template
                        .Replace("//usings", usingsCode)
                        .Replace("//global", globalCode)
                        .Replace("//before", beforeCode)
                        .Replace("//after", afterCode)
                        .IndexOf("//code") + positionInBlock;
                    break;
            }

            return adjustedPosition;
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