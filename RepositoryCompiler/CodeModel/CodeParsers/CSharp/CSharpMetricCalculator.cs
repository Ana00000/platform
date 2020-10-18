﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using RepositoryCompiler.CodeModel.CaDETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace RepositoryCompiler.CodeModel.CodeParsers.CSharp
{
    public class CSharpMetricCalculator
    {
        //TODO: See how this class will change with new metrics and try to decouple it from CSharp (e.g., by moving to CaDETClassMetric constructor)
        //TODO: Currently we see feature envy for CaDETClass.
        public CaDETClassMetrics CalculateClassMetrics(CaDETClass parsedClass)
        {
            return new CaDETClassMetrics
            {
                LOC = GetLinesOfCode(parsedClass.SourceCode),
                NMD = GetNumberOfMethodsDeclared(parsedClass),
                NAD = GetNumberOfAttributesDefined(parsedClass),
                WMC = GetWeightedMethodPerClass(parsedClass),
                LCOM = GetLackOfCohesionOfMethods(parsedClass)
            };
        }

        private double? GetLackOfCohesionOfMethods(CaDETClass parsedClass)
        {
            //TODO: Will need to reexamine the way we look at accessors and fields
            double maxCohesion = (GetNumberOfAttributesDefined(parsedClass) + GetNumberOfSimpleAccessors(parsedClass)) * GetNumberOfMethodsDeclared(parsedClass);
            if (maxCohesion == 0) return null;

            double methodFieldAccess = 0;
            foreach (var method in parsedClass.Methods.Where(method => method.Type.Equals(CaDETMemberType.Method)))
            {
                methodFieldAccess += CountOwnFieldAndAccessorAccessed(parsedClass, method);
            }
            return Math.Round(1 - methodFieldAccess/maxCohesion, 3);
        }

        private int GetNumberOfSimpleAccessors(CaDETClass parsedClass)
        {
            return parsedClass.Methods.Count(method => method.IsSimpleAccessor());
        }

        private int CountOwnFieldAndAccessorAccessed(CaDETClass parsedClass, CaDETMember method)
        {
            int counter = 0;
            foreach (var fieldOrAccessor in method.AccessedFieldsAndAccessors)
            {
                if (Enumerable.Contains(parsedClass.Fields, fieldOrAccessor) || Enumerable.Contains(parsedClass.Methods, fieldOrAccessor))
                {
                    counter++;
                }
            }

            return counter;
        }
        
        private int GetWeightedMethodPerClass(CaDETClass parsedClass)
        {
            //Defined based on 10.1109/32.295895
            return parsedClass.Methods.Sum(method => method.Metrics.CYCLO);
        }
        
        private int GetNumberOfAttributesDefined(CaDETClass parsedClass)
        {
            //TODO: Probably should expand to include simple accessors that do not have a related field.
            //TODO: It is C# specific, but this is the CSSharpMetricCalculator
            return parsedClass.Fields.Count;
        }

        private int GetNumberOfMethodsDeclared(CaDETClass parsedClass)
        {
            return parsedClass.Methods.Count(method => method.Type.Equals(CaDETMemberType.Method));
        }

        public CaDETMemberMetrics CalculateMemberMetrics(MemberDeclarationSyntax member)
        {
            return new CaDETMemberMetrics
            {
                CYCLO = CalculateCyclomaticComplexity(member),
                LOC = GetLinesOfCode(member.ToString()),
                NOP = GetNumberOfParameters(member),
                NOLV = GetNumberOfLocalVariables(member)
            };
        }

        private int GetNumberOfLocalVariables(MemberDeclarationSyntax method)
        {
            // TODO: Change this also to real value
            return 10;
        }

        private int GetNumberOfParameters(MemberDeclarationSyntax method)
        {
            // First because of structure type of 'OfType<ParameterListSyntax>()'
            return method.DescendantNodes().OfType<ParameterListSyntax>().First().Parameters.Count;
        }

        private int GetLinesOfCode(string code)
        {
            return code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length;
        }

        private int CalculateCyclomaticComplexity(MemberDeclarationSyntax method)
        {
            //Defined based on https://www.ndepend.com/docs/code-metrics#CC
            int count = method.DescendantNodes().OfType<IfStatementSyntax>().Count();
            count += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
            count += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
            count += method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
            count += method.DescendantNodes().OfType<CaseSwitchLabelSyntax>().Count();
            count += method.DescendantNodes().OfType<DefaultSwitchLabelSyntax>().Count();
            count += method.DescendantNodes().OfType<ContinueStatementSyntax>().Count();
            count += method.DescendantNodes().OfType<GotoStatementSyntax>().Count();
            count += method.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();
            count += method.DescendantNodes().OfType<CatchClauseSyntax>().Count();

            count += CountLogicalOperators(method, "&&");
            count += CountLogicalOperators(method, "||");
            count += CountLogicalOperators(method, "??");
            
            return count + 1;
        }

        private int CountLogicalOperators(MemberDeclarationSyntax method, string pattern)
        {
            var comments = method.DescendantTrivia();
            int commentOperatorCount = comments.Sum(comment => CountOccurrences(comment.ToString(), pattern));
            return CountOccurrences(method.ToString(), pattern) - commentOperatorCount;
        }

        private int CountOccurrences(string text, string pattern)
        {
            var count = 0;
            var i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }
    }
}