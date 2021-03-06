﻿using CodeModel.CaDETModel.CodeItems;
using CodeModel.CodeParsers.CSharp.Exceptions;
using CodeModel.Tests.DataFactories;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CodeModel.Tests.Unit
{
    public class CodeParserTests
    {
        private readonly CodeFactory _testDataFactory = new CodeFactory();

        //This test is a safety net for the C# SyntaxParser and serves to check the understanding of the API.
        [Fact]
        public void Compiles_CSharp_class_with_appropriate_fields_and_methods()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetDoctorClassText()).Classes;

            classes.ShouldHaveSingleItem();
            var doctorClass = classes.First();
            doctorClass.Members.ShouldContain(method =>
                method.Type.Equals(CaDETMemberType.Property) && method.Name.Equals("Email"));
            doctorClass.Members.ShouldContain(method => method.Type.Equals(CaDETMemberType.Constructor));
            doctorClass.Members.ShouldContain(method =>
                method.Type.Equals(CaDETMemberType.Method) && method.Name.Equals("IsAvailable"));
            doctorClass.Members.First().Parent.SourceCode.ShouldBe(doctorClass.SourceCode);
            doctorClass.FindMember("Email").Modifiers.First().Value.ShouldBe(CaDETModifierValue.Public);
            doctorClass.FindMember("IsAvailable").Modifiers.First().Value.ShouldBe(CaDETModifierValue.Internal);
        }

        [Fact]
        public void Checks_method_signature()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;

            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var service = classes.Find(c => c.Name.Equals("DoctorService"));
            var holidayDates = doctor.FindMember("HolidayDates");
            var overlapsWith = dateRange.FindMember("OverlapsWith");
            var findDoctors = service.FindMember("FindAvailableDoctor");
            holidayDates.Signature().Equals("HolidayDates");
            overlapsWith.Signature().Equals("OverlapsWith(DoctorApp.Model.Data.DateR.DateRange)");
            findDoctors.Signature().Equals("FindAvailableDoctor(DoctorApp.Model.Data.DateR.DateRange)");
        }

        [Fact]
        public void Calculates_invoked_methods()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;

            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var service = classes.Find(c => c.Name.Equals("DoctorService"));
            var overlapsWith = dateRange.FindMember("OverlapsWith");
            var logChecked = service.FindMember("LogChecked");
            var findDoctors = service.FindMember("FindAvailableDoctor");
            
            findDoctors.InvokedMethods.ShouldContain(overlapsWith);
            findDoctors.InvokedMethods.ShouldContain(logChecked);
            logChecked.InvokedMethods.ShouldContain(findDoctors);
            logChecked.InvokedMethods.ShouldContain(overlapsWith);
            logChecked.InvokedMethods.ShouldContain(doctor.FindMember("TestFunction"));
        }

        [Fact]
        public void Calculates_accessed_fields()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;
            
            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var service = classes.Find(c => c.Name.Equals("DoctorService"));
            var holidayDates = doctor.FindMember("HolidayDates");
            var findDoctors = service.FindMember("FindAvailableDoctor");
            var logChecked = service.FindMember("LogChecked");
            
            findDoctors.AccessedFields.ShouldContain(doctor.Fields.Find(f => f.Name.Equals("Test")));
            logChecked.AccessedFields.ShouldContain(service.FindField("_doctors"));
            logChecked.AccessedFields.ShouldContain(doctor.FindField("Test"));
            logChecked.AccessedFields.ShouldContain(doctor.FindField("TestDR"));
            logChecked.AccessedFields.ShouldContain(dateRange.FindField("NumOfDays"));
        }
        [Fact]
        public void Calculates_accessed_accessors()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;
            
            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var service = classes.Find(c => c.Name.Equals("DoctorService"));
            var holidayDates = doctor.FindMember("HolidayDates");
            var findDoctors = service.FindMember("FindAvailableDoctor");
            var logChecked = service.FindMember("LogChecked");
            
            findDoctors.AccessedAccessors.ShouldContain(holidayDates);
            logChecked.AccessedAccessors.ShouldContain(service.FindMember("TestDoc"));
            logChecked.AccessedAccessors.ShouldContain(doctor.FindMember("TestProp"));
            logChecked.AccessedAccessors.ShouldContain(doctor.FindMember("Name"));
            logChecked.AccessedAccessors.ShouldContain(dateRange.FindMember("From"));
            logChecked.AccessedAccessors.ShouldContain(dateRange.FindMember("To"));
        }
        [Fact (Skip = "Unsupported feature")]
        public void Calculates_array_element_accessed_accessor()
        {
            //CURRENTLY NOT SUPPORTED - ergo tests fail.
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;
            
            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var service = classes.Find(c => c.Name.Equals("DoctorService"));
            var holidayDates = doctor.FindMember("HolidayDates");
            var logChecked = service.FindMember("LogChecked");
            
            logChecked.AccessedAccessors.ShouldContain(holidayDates);
        }

        [Fact]
        public void Determines_if_is_data_class()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;

            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var service = classes.Find(c => c.Name.Equals("DoctorService"));
            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            dateRange.IsDataClass().ShouldBeFalse();
            doctor.IsDataClass().ShouldBeFalse();
            service.IsDataClass().ShouldBeFalse();
        }

        [Fact]
        public void Builds_member_parameters()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;

            var service = classes.Find(c => c.Name.Equals("DoctorService"));
            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var overlapTimeSpanParam = dateRange.FindMember("OverlapsWith").Params.First();
            overlapTimeSpanParam.Name.ShouldBe("timeSpan");
            overlapTimeSpanParam.Type.FullType.ShouldBe("DoctorApp.Model.Data.DateR.DateRange");
            var serviceTimeSpanParam = service.FindMember("FindAvailableDoctor").Params.First();
            serviceTimeSpanParam.Name.ShouldBe("timeSpan");
            serviceTimeSpanParam.Type.FullType.ShouldBe("DoctorApp.Model.Data.DateR.DateRange");
            var logParam = service.FindMember("LogChecked").Params.First();
            logParam.Name.ShouldBe("testData");
            logParam.Type.FullType.ShouldBe("int");
        }

        [Fact]
        public void Establishes_correct_class_hierarchy()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetClassesWithHierarchy()).Classes;

            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var employee = classes.Find(c => c.Name.Equals("Employee"));
            var entity = classes.Find(c => c.Name.Equals("Entity"));
            doctor.Parent.ShouldBe(employee);
            employee.Parent.ShouldBe(entity);
            entity.Parent.ShouldBeNull();
            doctor.FindMember("Doctor").AccessedAccessors.ShouldContain(employee.FindMember("Email"));
            employee.FindMember("Employee").AccessedAccessors.ShouldContain(entity.FindMember("Id"));
        }

        [Fact]
        public void Fails_to_build_code_with_nonunique_class_full_names()
        {
            CodeModelFactory factory = new CodeModelFactory();

            Should.Throw<NonUniqueFullNameException>(() => factory.CreateProject(_testDataFactory.GetTwoClassesWithSameFullName()));
        }

        [Fact]
        public void Ignores_partial_classes()
        {
            CodeModelFactory factory = new CodeModelFactory();

            var classes = factory.CreateProject(_testDataFactory.GetTwoPartialClassesWithSameFullName()).Classes;

            classes.Count.ShouldBe(0);
        }

        [Fact]
        public void Calculates_method_variables()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;

            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var service = classes.Find(c => c.Name.Equals("DoctorService"));
            var overlapsWith = dateRange.FindMember("OverlapsWith");
            var logChecked = service.FindMember("LogChecked");

            overlapsWith.Variables.Count().ShouldBe(0);
            logChecked.Variables.Count().ShouldBe(8);

            logChecked.Variables.Count(v => v.Name.Equals("test1")).ShouldBe(1);
            logChecked.Variables.Count(v => v.Name.Equals("test")).ShouldBe(1);
            logChecked.Variables.Count(v => v.Name.Equals("a")).ShouldBe(1);
            logChecked.Variables.Count(v => v.Name.Equals("b")).ShouldBe(1);
            logChecked.Variables.Count(v => v.Name.Equals("c")).ShouldBe(1);
            logChecked.Variables.Count(v => v.Name.Equals("temp1")).ShouldBe(1);
            logChecked.Variables.Count(v => v.Name.Equals("temp2")).ShouldBe(1);
            logChecked.Variables.Count(v => v.Name.Equals("temp2")).ShouldBe(1);
        }

        [Fact]
        public void Checks_field_linked_types()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;
            List<CaDETClass> otherClasses = factory.CreateProject(_testDataFactory.GetClassesFromDifferentNamespace()).Classes;

            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var doctorService = classes.Find(c => c.Name.Equals("DoctorService"));
            var otherDoctor = otherClasses.Find(c => c.Name.Equals("Doctor"));
            var otherDateRange = otherClasses.Find(c => c.Name.Equals("DateRange"));

            dateRange.FindField("testDictionary").GetLinkedTypes().ShouldContain(doctor);
            dateRange.FindField("testDictionary").GetLinkedTypes().ShouldNotContain(otherDoctor);
            dateRange.FindField("testDictionary").GetLinkedTypes().ShouldContain(doctorService);
            doctor.FindField("TestDR").GetLinkedTypes().ShouldContain(dateRange);
            doctor.FindField("TestDR").GetLinkedTypes().ShouldNotContain(otherDateRange);
            doctorService.FindField("_doctors").GetLinkedTypes().ShouldContain(doctor);
        }

        [Fact]
        public void Checks_method_linked_return_types()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;
            List<CaDETClass> otherClasses = factory.CreateProject(_testDataFactory.GetClassesFromDifferentNamespace()).Classes;

            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var doctorService = classes.Find(c => c.Name.Equals("DoctorService"));
            var otherDoctor = otherClasses.Find(c => c.Name.Equals("Doctor"));
            var otherDateRange = otherClasses.Find(c => c.Name.Equals("DateRange"));

            doctor.FindMember("TestFunction").GetLinkedReturnTypes().ShouldContain(dateRange);
            doctor.FindMember("TestFunction").GetLinkedReturnTypes().ShouldNotContain(otherDateRange);
            doctorService.FindMember("FindAvailableDoctor").GetLinkedReturnTypes().ShouldContain(doctor);
            doctorService.FindMember("FindAvailableDoctor").GetLinkedReturnTypes().ShouldNotContain(otherDoctor);
        }

        [Fact]
        public void Checks_variable_linked_types()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;
            List<CaDETClass> otherClasses = factory.CreateProject(_testDataFactory.GetClassesFromDifferentNamespace()).Classes;

            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var doctorService = classes.Find(c => c.Name.Equals("DoctorService"));
            var otherDoctor = otherClasses.Find(c => c.Name.Equals("Doctor"));
            var otherDateRange = otherClasses.Find(c => c.Name.Equals("DateRange"));

            var variables = doctor.FindMember("TestFunction").Variables.SelectMany(v => v.GetLinkedTypes()).ToList();

            variables.ShouldContain(doctor);
            variables.ShouldNotContain(otherDoctor);
            variables.ShouldContain(dateRange);
            variables.ShouldNotContain(otherDateRange);
            variables.ShouldNotContain(doctorService);
        }

        [Fact]
        public void Checks_property_linked_types()
        {
            CodeModelFactory factory = new CodeModelFactory();

            List<CaDETClass> classes = factory.CreateProject(_testDataFactory.GetMultipleClassTexts()).Classes;
            List<CaDETClass> otherClasses = factory.CreateProject(_testDataFactory.GetClassesFromDifferentNamespace()).Classes;

            var dateRange = classes.Find(c => c.Name.Equals("DateRange"));
            var doctor = classes.Find(c => c.Name.Equals("Doctor"));
            var otherDateRange = otherClasses.Find(c => c.Name.Equals("DateRange"));

            var testPropertyType = doctor.Members.Find(m => m.Name.Equals("TestProp")).GetLinkedReturnTypes();
            var holidayDatesType = doctor.Members.Find(m => m.Name.Equals("HolidayDates")).GetLinkedReturnTypes();

            testPropertyType.ShouldContain(dateRange);
            testPropertyType.ShouldNotContain(otherDateRange);
            holidayDatesType.ShouldContain(dateRange);
            holidayDatesType.ShouldNotContain(otherDateRange);
        }

        [Theory]
        [MemberData(nameof(CodeFactory.GetInvalidSyntaxClasses), MemberType = typeof(CodeFactory))]
        public void Checks_syntax_errors(string[] sourceCode, int syntaxErrorCount)
        {
            CodeModelFactory factory = new CodeModelFactory();
            
            var project = factory.CreateProject(sourceCode);

            project.SyntaxErrors.Count.ShouldBe(syntaxErrorCount);
        }
    }
}
