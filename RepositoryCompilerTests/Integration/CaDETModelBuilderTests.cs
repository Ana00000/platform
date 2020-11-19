﻿using RepositoryCompiler.CodeModel;
using RepositoryCompiler.CodeModel.CaDETModel.CodeItems;
using Shouldly;
using Xunit;

namespace RepositoryCompilerTests.Integration
{
    public class CaDETModelBuilderTests
    {
        [Fact]
        public void Build_code_model_from_repository()
        {
            CodeModelFactory factory = new CodeModelFactory(LanguageEnum.CSharp);

            var project = factory.ParseFiles("C:/repo");

            project.Classes.ShouldNotBeEmpty();
            CaDETClass conflict = project.Classes.Find(c => c.FullName.Equals("LibGit2Sharp.Conflict"));
            conflict.ShouldNotBeNull();
            conflict.Fields.ShouldContain(f => f.Name.Equals("ancestor"));
            conflict.Fields.ShouldContain(f => f.Name.Equals("ours"));
            conflict.Fields.ShouldContain(f => f.Name.Equals("theirs"));
            conflict.Members.ShouldContain(m => m.Name.Equals("Conflict") && m.Type.Equals(CaDETMemberType.Constructor) && m.AccessedFields.Count == 3);
            conflict.Members.ShouldContain(m => m.Name.Equals("Equals") && m.Type.Equals(CaDETMemberType.Method) && m.AccessedFields.Count == 1);
            conflict.Metrics.LOC.ShouldBe(108);
            conflict.Metrics.LCOM.ShouldBe(0.833);
            conflict.Metrics.NAD.ShouldBe(4);
            conflict.Metrics.NMD.ShouldBe(3);
            conflict.Metrics.WMC.ShouldBe(8);
            CaDETClass certificate = project.Classes.Find(c => c.FullName.Equals("LibGit2Sharp.Certificate"));
            certificate.ShouldNotBeNull();
            certificate.Members.ShouldBeEmpty();
            certificate.Fields.ShouldBeEmpty();
            certificate.Metrics.LCOM.ShouldBeNull();
            certificate.Metrics.LOC.ShouldBe(3);
            certificate.Metrics.NAD.ShouldBe(0);
            certificate.Metrics.NMD.ShouldBe(0);
            certificate.Metrics.WMC.ShouldBe(0);
            CaDETClass handles = project.Classes.Find(c => c.FullName.Equals("LibGit2Sharp.Core.Handles.Libgit2Object"));
            handles.ShouldNotBeNull();
            handles.Fields.ShouldContain(f => f.Name.Equals("ptr"));
            handles.Members.ShouldContain(m => m.Name.Equals("Handle") && m.Type.Equals(CaDETMemberType.Property));
            handles.Members.ShouldContain(m => m.Name.Equals("Dispose") && m.Type.Equals(CaDETMemberType.Method)
                                                                        && m.InvokedMethods.Count == 1 && m.AccessedFields.Count == 0 && m.AccessedAccessors.Count == 0);
            handles.Metrics.LCOM.ShouldBe(0.667);
            handles.Metrics.LOC.ShouldBe(100);
            handles.Metrics.NAD.ShouldBe(3);
            handles.Metrics.NMD.ShouldBe(4);
            handles.Metrics.WMC.ShouldBe(10);
        }
    }
}
