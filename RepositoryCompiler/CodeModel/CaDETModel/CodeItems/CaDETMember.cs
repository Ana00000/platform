﻿using System.Collections.Generic;
using System.Linq;
using RepositoryCompiler.CodeModel.CaDETModel.Metrics;

namespace RepositoryCompiler.CodeModel.CaDETModel.CodeItems
{
    public class CaDETMember
    {
        public string Name { get; internal set; }
        public CaDETMemberType Type { get; internal set; }
        public string SourceCode { get; internal set; }
        public CaDETClass Parent { get; internal set; }
        public List<string> Params { get; internal set; }
        public List<CaDETModifier> Modifiers { get; internal set; }
        public ISet<CaDETMember> InvokedMethods { get; internal set; }
        public ISet<CaDETMember> AccessedAccessors { get; internal set; }
        public ISet<CaDETField> AccessedFields { get; internal set; }
        public CaDETMemberMetrics Metrics { get; internal set; }

        public override bool Equals(object other)
        {
            if (!(other is CaDETMember otherMember)) return false;
            if (Parent == null) return Name.Equals(otherMember.Name);
            return Name.Equals(otherMember.Name)
                   && Parent.Equals(otherMember.Parent)
                   && !Params.Except(otherMember.Params).Any();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsSimpleAccessor()
        {
            //TODO: This is a workaround that should be reworked https://stackoverflow.com/questions/64009302/roslyn-c-how-to-get-all-fields-and-properties-and-their-belonging-class-acce
            //TODO: It is specific to C# properties. Should move this to CSharpCodeParser so that each language can define its rule for calculating simple accessors.
            return Type.Equals(CaDETMemberType.Property)
                   && (InvokedMethods.Count == 0)
                   && (AccessedAccessors.Count == 0 && AccessedFields.Count < 2)
                   && !SourceCode.Contains("return ") && !SourceCode.Contains("=");
        }
    }
}