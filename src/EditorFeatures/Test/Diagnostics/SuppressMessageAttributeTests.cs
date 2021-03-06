﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Test.Utilities;

namespace Microsoft.CodeAnalysis.UnitTests.Diagnostics
{
    [UseExportProvider]
    public class SuppressMessageAttributeWorkspaceTests : SuppressMessageAttributeTests
    {
        protected override async Task VerifyAsync(string source, string language, DiagnosticAnalyzer[] analyzers, DiagnosticDescription[] expectedDiagnostics, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = null, bool logAnalyzerExceptionAsDiagnostics = true, string rootNamespace = null)
        {
            using var workspace = CreateWorkspaceFromFile(source, language, rootNamespace);
            var documentId = workspace.Documents[0].Id;
            var document = workspace.CurrentSolution.GetDocument(documentId);
            var span = (await document.GetSyntaxRootAsync()).FullSpan;

            var actualDiagnostics = new List<Diagnostic>();
            foreach (var analyzer in analyzers)
            {
                actualDiagnostics.AddRange(
                    await DiagnosticProviderTestUtilities.GetAllDiagnosticsAsync(analyzer, document, span, onAnalyzerException));
            }

            actualDiagnostics.Verify(expectedDiagnostics);
        }

        private static TestWorkspace CreateWorkspaceFromFile(string source, string language, string rootNamespace)
        {
            if (language == LanguageNames.CSharp)
            {
                return TestWorkspace.CreateCSharp(source);
            }
            else
            {
                return TestWorkspace.CreateVisualBasic(
                    source,
                    compilationOptions: new VisualBasic.VisualBasicCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary, rootNamespace: rootNamespace));
            }
        }

        protected override bool ConsiderArgumentsForComparingDiagnostics
        {
            get
            {
                // Round tripping diagnostics from DiagnosticData causes the Arguments info stored within compiler DiagnosticWithInfo to be lost, so don't compare Arguments in IDE.
                // NOTE: We will still compare squiggled text for the diagnostics, which is also a sufficient test.
                return false;
            }
        }
    }
}
