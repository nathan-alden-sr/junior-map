using System;

using Junior.Map.Mapper;

using NUnit.Framework;

namespace Junior.Map.UnitTests.Mapper
{
	public static class MappingMethodGeneratorTester
	{
		[TestFixture]
		public class When_generating_a_mapping_method
		{
			[SetUp]
			public void Setup()
			{
				_configuration = new MapperConfiguration<Foo, Bar>();
			}

			private MapperConfiguration<Foo, Bar> _configuration;

			#region Test types

			public class Foo
			{
				public string A
				{
					get;
					set;
				}

				public string B
				{
					get;
					set;
				}

				public string C
				{
					get;
					set;
				}
			}

			public class Bar
			{
				public string A
				{
					get;
					set;
				}

				public string B
				{
					get;
					set;
				}

				public string C
				{
					get;
					// ReSharper disable UnusedAutoPropertyAccessor.Local
					private set;
					// ReSharper restore UnusedAutoPropertyAccessor.Local
				}

				public static int StaticInt
				{
					get;
					set;
				}
			}

			public class BarSubClass : Bar
			{
				public bool SubClassProperty
				{
					get;
					set;
				}
			}

			#endregion

			[Test]
			public void Must_not_throw_if_read_only_properties_are_mapped_with_ByIgnoring()
			{
				_configuration.Map(bar => bar.A).From(foo => foo.A);
				_configuration.Map(bar => bar.B).From(foo => foo.B);
				_configuration.Map(bar => bar.C).ByIgnoring();

				var systemUnderTest = new MappingMethodGenerator();

				Assert.DoesNotThrow(() => systemUnderTest.GenerateMappingMethod(_configuration));
			}

			[Test]
			public void Must_not_throw_if_read_only_properties_are_mapped_with_ByInvoking()
			{
				_configuration.Map(bar => bar.A).From(foo => foo.A);
				_configuration.Map(bar => bar.B).From(foo => foo.B);
				_configuration.Map(bar => bar.C).ByInvoking((bar1, foo1) => { });

				var systemUnderTest = new MappingMethodGenerator();

				Assert.DoesNotThrow(() => systemUnderTest.GenerateMappingMethod(_configuration));
			}

			[Test]
			public void Must_not_throw_if_settable_properties_are_mapped_as_ByExecuting()
			{
				_configuration.Map(bar => bar.A).ByInvoking((bar1, foo1) => { });
				_configuration.Map(bar => bar.B).ByInvoking((bar1, foo1) => { });
				_configuration.Map(bar => bar.C).ByInvoking((bar1, foo1) => { });

				var systemUnderTest = new MappingMethodGenerator();

				Assert.DoesNotThrow(() => systemUnderTest.GenerateMappingMethod(_configuration));
			}

			[Test]
			public void Must_not_throw_if_settable_properties_are_mapped_as_ByIgnoring()
			{
				_configuration.Map(bar => bar.A).ByIgnoring();
				_configuration.Map(bar => bar.B).ByIgnoring();
				_configuration.Map(bar => bar.C).ByIgnoring();

				var systemUnderTest = new MappingMethodGenerator();

				Assert.DoesNotThrow(() => systemUnderTest.GenerateMappingMethod(_configuration));
			}

			[Test]
			public void Must_not_throw_if_static_properties_are_not_mapped()
			{
				_configuration.Map(bar => bar.A).From(foo => foo.A);
				_configuration.Map(bar => bar.B).From(foo => foo.B);
				_configuration.Map(bar => bar.C).ByIgnoring();

				var systemUnderTest = new MappingMethodGenerator();

				Assert.DoesNotThrow(() => systemUnderTest.GenerateMappingMethod(_configuration));
			}

			[Test]
			public void Must_not_throw_if_two_properties_are_mapped_together_as_ByExecuting()
			{
				_configuration.Map(bar => bar.A).And(bar => bar.B).ByInvoking((bar1, foo) => { });
				_configuration.Map(bar => bar.C).ByIgnoring();

				var systemUnderTest = new MappingMethodGenerator();

				Assert.DoesNotThrow(() => systemUnderTest.GenerateMappingMethod(_configuration));
			}

			[Test]
			public void Must_not_throw_if_two_properties_are_mapped_together_as_ByIgnoring()
			{
				_configuration.Map(bar => bar.A).And(bar => bar.B).ByIgnoring();
				_configuration.Map(bar => bar.C).ByIgnoring();

				var systemUnderTest = new MappingMethodGenerator();

				Assert.DoesNotThrow(() => systemUnderTest.GenerateMappingMethod(_configuration));
			}

			[Test]
			public void Must_throw_exception_if_base_class_properties_are_not_mapped()
			{
				var barSubClassConfiguration = new MapperConfiguration<Foo, BarSubClass>();

				barSubClassConfiguration.Map(target => target.SubClassProperty).From(source => false);

				var systemUnderTest = new MappingMethodGenerator();

				Assert.Throws<Exception>(
					() => systemUnderTest.GenerateMappingMethod(barSubClassConfiguration),
					String.Format(
						"Mapping configuration is invalid.{0}A mapping was not provided for target property 'BarSubClass.A' from source 'Foo'.{0}A mapping was not provided for target property 'BarSubClass.B' from source 'Foo'.{0}A mapping was not provided for read-only target property 'BarSubClass.C' from source 'Foo'.",
						Environment.NewLine));
			}

			[Test]
			public void Must_throw_exception_if_not_all_desired_type_instance_properties_are_mapped()
			{
				_configuration.Map(bar => bar.A).From(foo1 => foo1.A);

				var systemUnderTest = new MappingMethodGenerator();

				Assert.Throws<Exception>(
					() => systemUnderTest.GenerateMappingMethod(_configuration),
					String.Format(
						"Mapping configuration is invalid.{0}A mapping was not provided for target property 'Bar.B' from source 'Foo'.{0}A mapping was not provided for read-only target property 'Bar.C' from source 'Foo'.",
						Environment.NewLine));
			}

			[Test]
			public void Must_throw_if_read_only_properties_are_mapped_as_assignment()
			{
				_configuration.Map(bar => bar.A).From(foo => foo.A);
				_configuration.Map(bar => bar.B).From(foo => foo.B);
				_configuration.Map(bar => bar.C).From(foo => foo.C);

				var systemUnderTest = new MappingMethodGenerator();

				Assert.Throws<Exception>(
					() => systemUnderTest.GenerateMappingMethod(_configuration),
					String.Format(
						"Mapping configuration is invalid.{0}A mapping was provided as an assignment for target property 'Bar.C' but the target property has no public instance setter.",
						Environment.NewLine));
			}

			[Test]
			public void Must_throw_if_read_only_properties_are_not_mapped()
			{
				_configuration.Map(bar => bar.A).From(foo => foo.A);
				_configuration.Map(bar => bar.B).From(foo => foo.B);

				var systemUnderTest = new MappingMethodGenerator();

				Assert.Throws<Exception>(
					() => systemUnderTest.GenerateMappingMethod(_configuration),
					String.Format("Mapping configuration is invalid.{0}A mapping was not provided for read-only target property 'Bar.C' from source 'Foo'.", Environment.NewLine));
			}
		}
	}
}