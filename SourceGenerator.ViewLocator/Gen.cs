using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AvaloniaSourceGeneratedViewLocation.SourceGenerator.ViewLocator
{
// Creates the Dictionary with the Initializers for the Views, and a method `PopulateViewModelViewMappings`
// which can add it to the `Locator` dictionary [in the ViewLocator] which gets consumed by the `ViewLocator` 

// Usage: 3 steps: A,B,C

// A) Modify the method that adds the usings to the generated class (Methodname: `AddNamesspaceUsings()`)
// B) Modify the name of the namespace in the field `s_Setting_Namespace` that adds the usings to the generated class (Methodname: `AddNamesspaceUsings()`)
// C) In `App.axaml.cs` -> at the beginning of  `OnFrameworkInitialized()` call `ViewLocator.PopulateViewModelViewMappings()`;
// D) Don't forget to make ViewLocator-class partial.

    [Generator]
    public class ViewLocatorSourceGenerator : ISourceGenerator
    {
        private static bool                                            s_enableDiagnosticOutput   = false;
        public const   string                                          s_Setting_Namespace        = "AvaloniaControlsTest";
        private static Dictionary<INamedTypeSymbol, INamedTypeSymbol?> UniquenessGuaranteeingMap { get; } = new();
        private        List<ViewModelViewPair>                         s_toRemove = new();


        /// <summary>
        /// Executes the Generator
        /// </summary>
        /// <param name="context"></param>
        public void Execute(GeneratorExecutionContext context)
        {
            var classSymbols = GetAllClassSymbols(context).ToArray();
            var viewModelViewPairs = classSymbols
                                    .Distinct()
                                    .Where(s => s.Name.EndsWith("ViewModel"))
                                    .Where(viewModel => UniquenessGuaranteeingMap.TryGetValue(viewModel, out _) == false)
                                    .Select(viewModel =>
                                            {
                                                var pair = new ViewModelViewPair {
                                                                                     ViewModel = viewModel,
                                                                                     View      = FindViewClass(viewModel, classSymbols)!,
                                                                                 };

                                                bool failed = pair.View == null;
                                                if (failed) s_toRemove.Add(pair);
                                                else
                                                {
                                                    UniquenessGuaranteeingMap.Add(viewModel, pair.View);
                                                }

                                                return pair;
                                            })
                                    .Where(pair => pair.View != null)
                                    .ToList()
                                    .Except(s_toRemove)
                                    .ToArray();

            var registrationMethodsCode = GenerateRegistrationMethodsCode(viewModelViewPairs);
            context.AddSource("ViewLocatorRegistrations.cs", registrationMethodsCode);

            if (s_enableDiagnosticOutput)
            {
                var diagnostic = Diagnostic.Create(MyDiagnostic, Location.None, viewModelViewPairs.Length);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static readonly DiagnosticDescriptor MyDiagnostic = new DiagnosticDescriptor(id: "SG001",
                                                                                             title: "Source Generator Diagnostic",
                                                                                             messageFormat:
                                                                                             "Source Generator processed {0} ViewModel-View pairs",
                                                                                             category: "SourceGenerator",
                                                                                             DiagnosticSeverity.Warning,
                                                                                             isEnabledByDefault: true);
        

        /// <summary>
        /// Modify A) and B)
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        private string GenerateRegistrationMethodsCode(IEnumerable<ViewModelViewPair> pairs)
        {
            var pairsArr = pairs.ToArray();
            var sb = new StringBuilder();

            // A) modify method for your own usings
            AddNamesspaceUsings(sb);

            // B) modify value of the namespace-variable V
            sb.AppendLine($"namespace {s_Setting_Namespace}");
            sb.AppendLine("{");
            sb.AppendLine(" public partial class ViewLocator");
            sb.AppendLine(" {");

            sb.AppendLine("     public void PopulateViewModelViewMappings(Dictionary<Type, Func<Control>>? locator = null)");
            sb.AppendLine("     {");
            sb.AppendLine("             if (locator == null)");
            sb.AppendLine("             { locator = Locator; }");
            foreach (var pair in pairsArr)
            {
                sb.AppendLine($"        Locator.Add(typeof({pair.ViewModel.Name}), () => new {pair.View.Name}());");
            }
            sb.AppendLine("     }");
            sb.AppendLine(" }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private INamedTypeSymbol? FindViewClass(INamedTypeSymbol viewModelClass, IEnumerable<INamedTypeSymbol> classSymbols)
        {
            var viewClassName = viewModelClass.Name.Replace("ViewModel", "View");
            return classSymbols.FirstOrDefault(c => c.Name == viewClassName);
        }

        private IEnumerable<INamedTypeSymbol> GetAllClassSymbols(GeneratorExecutionContext context)
        {
            var                    compilation  = context.Compilation;
            List<INamedTypeSymbol> classSymbols = new List<INamedTypeSymbol>();

            foreach (var tree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(tree);
                var classNodes    = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classNode in classNodes)
                {
                    if (semanticModel.GetDeclaredSymbol(classNode) is INamedTypeSymbol classSymbol)
                    {
                        classSymbols.Add(classSymbol);
                    }
                }
            }

            return classSymbols;
        }

        // A)
        public virtual void AddNamesspaceUsings(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Collections.ObjectModel;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using AvaloniaSourceGeneratedViewLocation.Views;");
            sb.AppendLine("using AvaloniaSourceGeneratedViewLocation.ViewModels;");
            sb.AppendLine("using Avalonia.Controls;");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }

    public class ViewModelViewPair
    {
        public INamedTypeSymbol ViewModel { get; set; } = null!;
        public INamedTypeSymbol View      { get; set; } = null!;
    }
}

