﻿namespace ImmutableObjectGraph.CodeGeneration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.MSBuild;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.ImmutableObjectGraph_SFG;
    using Xunit;

    public class CodeGenTests
    {
        protected Solution solution;
        protected ProjectId projectId;
        protected DocumentId inputDocumentId;

        public CodeGenTests()
        {
            var workspace = new CustomWorkspace();
            var project = workspace.CurrentSolution.AddProject("test", "test", "C#")
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(string).Assembly))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(GenerateImmutableAttribute).Assembly))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(CodeGenerationAttribute).Assembly))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(Optional).Assembly))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(ImmutableArray).Assembly))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(Assembly.LoadWithPartialName("System.Runtime")));
            var inputDocument = project.AddDocument("input.cs", string.Empty);
            this.inputDocumentId = inputDocument.Id;
            project = inputDocument.Project;
            this.projectId = inputDocument.Project.Id;
            this.solution = project.Solution;
        }

        [Fact]
        public async Task NoFields_HasCreateMethod()
        {
            var result = await this.GenerateFromStreamAsync("NoFields");
            Assert.True(result.DeclaredMethods.Any(m => m.Name == "Create" && m.Parameters.Length == 0 && m.IsStatic));
        }

        [Fact]
        public async Task FamilyPersonWatch()
        {
            await this.GenerateFromStreamAsync("FamilyPersonWatch");
        }

        [Fact]
        public async Task OneScalarField_HasWithMethod()
        {
            var result = await this.GenerateFromStreamAsync("OneScalarField");
            Assert.True(result.DeclaredMethods.Any(m => m.Name == "With" && m.Parameters.Single().Name == "seeds" && !m.IsStatic));
        }

        [Fact]
        public async Task OneScalarField_HasCreateMethod()
        {
            var result = await this.GenerateFromStreamAsync("OneScalarField");
            Assert.True(result.DeclaredMethods.Any(m => m.Name == "Create" && m.Parameters.Single().Name == "seeds"));
        }

        [Fact]
        public async Task OneScalarFieldWithBuilder_HasToBuilderMethod()
        {
            var result = await this.GenerateFromStreamAsync("OneScalarFieldWithBuilder");
            Assert.True(result.DeclaredMethods.Any(m => m.Name == "ToBuilder" && m.Parameters.Length == 0 && !m.IsStatic));
        }

        [Fact]
        public async Task OneScalarFieldWithBuilder_HasCreateBuilderMethod()
        {
            var result = await this.GenerateFromStreamAsync("OneScalarFieldWithBuilder");
            Assert.True(result.DeclaredMethods.Any(m => m.Name == "CreateBuilder" && m.Parameters.Length == 0 && m.IsStatic));
        }

        [Fact]
        public async Task OneScalarFieldWithBuilder_BuilderHasMutableProperties()
        {
            var result = await this.GenerateFromStreamAsync("OneScalarFieldWithBuilder");
            Assert.True(result.DeclaredProperties.Any(p => p.ContainingType?.Name == "Builder" && p.Name == "Seeds" && p.SetMethod != null && p.GetMethod != null));
        }

        [Fact]
        public async Task OneScalarFieldWithBuilder_BuilderHasToImmutableMethod()
        {
            var result = await this.GenerateFromStreamAsync("OneScalarFieldWithBuilder");
            Assert.True(result.DeclaredMethods.Any(m => m.ContainingType?.Name == "Builder" && m.Name == "ToImmutable" && m.Parameters.Length == 0 && !m.IsStatic));
        }

        protected async Task<GenerationResult> GenerateFromStreamAsync(string testName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType().Namespace + ".TestSources." + testName + ".cs"))
            {
                return await this.GenerateAsync(SourceText.From(stream));
            }
        }

        protected async Task<GenerationResult> GenerateAsync(SourceText inputSource)
        {
            var solution = this.solution.WithDocumentText(this.inputDocumentId, inputSource);
            var inputDocument = solution.GetDocument(this.inputDocumentId);
            var outputDocument = await DocumentTransform.TransformAsync(inputDocument, new MockProgress());

            // Make sure there are no compile errors.
            var compilation = await outputDocument.Project.GetCompilationAsync();
            var diagnostics = compilation.GetDiagnostics();
            var errors = from diagnostic in diagnostics
                         where diagnostic.Severity >= DiagnosticSeverity.Error
                         select diagnostic;

            Console.WriteLine(await outputDocument.GetTextAsync());

            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }

            Assert.Empty(errors);

            var semanticModel = await outputDocument.GetSemanticModelAsync();
            return new GenerationResult(outputDocument, semanticModel);
        }

        protected class GenerationResult
        {
            public GenerationResult(Document document, SemanticModel semanticModel)
            {
                this.Document = document;
                this.SemanticModel = semanticModel;
                this.Declarations = semanticModel.GetDeclarationsInSpan(TextSpan.FromBounds(0, semanticModel.SyntaxTree.Length), true, CancellationToken.None);
            }

            public Document Document { get; private set; }

            public SemanticModel SemanticModel { get; private set; }

            public ImmutableArray<DeclarationInfo> Declarations { get; private set; }

            public IEnumerable<ISymbol> DeclaredSymbols
            {
                get { return this.Declarations.Select(d => d.DeclaredSymbol); }
            }

            public IEnumerable<IMethodSymbol> DeclaredMethods
            {
                get { return this.DeclaredSymbols.OfType<IMethodSymbol>(); }
            }

            public IEnumerable<IPropertySymbol> DeclaredProperties
            {
                get { return this.DeclaredSymbols.OfType<IPropertySymbol>(); }
            }
        }

        private class MockProgress : IProgressAndErrors
        {
            public void Error(string message, uint line, uint column)
            {
            }

            public void Report(uint progress, uint total)
            {
            }

            public void Warning(string message, uint line, uint column)
            {
            }
        }
    }
}
