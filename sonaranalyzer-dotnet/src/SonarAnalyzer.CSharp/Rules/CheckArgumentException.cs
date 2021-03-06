/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class CheckArgumentException : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3928";
        private const string MessageFormat = "{0}";
        private const string ParameterLessConstructorMessage = "Use a constructor overloads that allows a more meaningful exception message to be provided.";
        private const string ConstructorParametersInverted = "ArgumentException constructor arguments have been inverted.";
        private const string InvalidParameterName = "The parameter name '{0}' is not declared in the argument list.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> ArgumentExceptionTypesToCheck = new HashSet<KnownType>
        {
            KnownType.System_ArgumentException,
            KnownType.System_ArgumentNullException,
            KnownType.System_ArgumentOutOfRangeException,
            KnownType.System_DuplicateWaitObjectException
        };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForIssue,
                SyntaxKind.ObjectCreationExpression);

        private static void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            var objectCreationSyntax = (ObjectCreationExpressionSyntax)analysisContext.Node;
            var methodSymbol = analysisContext.SemanticModel.GetSymbolInfo(objectCreationSyntax).Symbol
                as IMethodSymbol;
            if (methodSymbol?.ContainingType == null ||
                !methodSymbol.ContainingType.IsAny(ArgumentExceptionTypesToCheck))
            {
                return;
            }

            if (objectCreationSyntax.ArgumentList.Arguments.Count == 0)
            {
                analysisContext.ReportDiagnostic(Diagnostic.Create(rule, objectCreationSyntax.GetLocation(),
                    ParameterLessConstructorMessage));
                return;
            }

            Optional<object> parameterNameValue = new Optional<object>();
            Optional<object> messageValue = new Optional<object>();
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var argumentExpression = objectCreationSyntax.ArgumentList.Arguments[i].Expression;
                if (methodSymbol.Parameters[i].MetadataName == "paramName" ||
                    methodSymbol.Parameters[i].MetadataName == "parameterName")
                {
                    parameterNameValue = analysisContext.SemanticModel.GetConstantValue(argumentExpression);
                }
                else if (methodSymbol.Parameters[i].MetadataName == "message")
                {
                    messageValue = analysisContext.SemanticModel.GetConstantValue(argumentExpression);
                }
                else
                {
                    // do nothing
                }
            }

            if (!parameterNameValue.HasValue)
            {
                // can't check non-constant strings OR argument is not set
                return;
            }

            var methodArgumentNames = GetMethodArgumentNames(objectCreationSyntax);
            if (methodArgumentNames.Contains(TakeOnlyBeforeDot(parameterNameValue)))
            {
                // paramName exists
                return;
            }

            if (messageValue.HasValue &&
                methodArgumentNames.Contains(TakeOnlyBeforeDot(messageValue)))
            {
                analysisContext.ReportDiagnostic(Diagnostic.Create(rule,
                    objectCreationSyntax.GetLocation(), ConstructorParametersInverted));
                return;
            }

            analysisContext.ReportDiagnostic(Diagnostic.Create(rule, objectCreationSyntax.GetLocation(),
                    string.Format(InvalidParameterName, parameterNameValue.Value)));
        }

        private static ISet<string> GetMethodArgumentNames(ObjectCreationExpressionSyntax creationSyntax)
        {
            var creationContext = creationSyntax.AncestorsAndSelf().FirstOrDefault(ancestor =>
                ancestor is SimpleLambdaExpressionSyntax ||
                ancestor is ParenthesizedLambdaExpressionSyntax ||
                ancestor is AccessorDeclarationSyntax ||
                ancestor is BaseMethodDeclarationSyntax);

            return new HashSet<string>(GetArgumentNames(creationContext));
        }

        private static IEnumerable<string> GetArgumentNames(SyntaxNode node)
        {
            var simpleLambda = node as SimpleLambdaExpressionSyntax;
            if (simpleLambda != null)
            {
                return new[] { simpleLambda.Parameter.Identifier.ValueText };
            }

            var method = node as BaseMethodDeclarationSyntax;
            if (method != null)
            {
                return method.ParameterList.Parameters.Select(p => p.Identifier.ValueText);
            }

            var lambda = node as ParenthesizedLambdaExpressionSyntax;
            if (lambda != null)
            {
                return lambda.ParameterList.Parameters.Select(p => p.Identifier.ValueText);
            }
            var accessor = node as AccessorDeclarationSyntax;
            if (accessor != null)
            {
                var arguments = new List<string>();
                var indexer = node.FirstAncestorOrSelf<IndexerDeclarationSyntax>();
                if (indexer != null)
                {
                    arguments.AddRange(indexer.ParameterList.Parameters.Select(p => p.Identifier.ValueText));
                }
                if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
                {
                    arguments.Add("value");
                }

                return arguments;
            }

            return Enumerable.Empty<string>();
        }

        private static string TakeOnlyBeforeDot(Optional<object> value)
        {
            return ((string)value.Value).Split('.').FirstOrDefault();
        }
    }
}