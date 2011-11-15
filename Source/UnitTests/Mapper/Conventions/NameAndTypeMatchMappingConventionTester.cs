using System.Linq;

using Junior.Common;
using Junior.Map.Mapper;
using Junior.Map.Mapper.Conventions;

using NUnit.Framework;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Junior.Map.UnitTests.Mapper.Conventions
{
	public static class NameAndTypeMatchMappingConventionTester
	{
		[TestFixture]
		public class When_applying_NameAndTypeMatchMappingConvention
		{
			[SetUp]
			public void Setup()
			{
				_systemUnderTest = new NameAndTypeMatchMappingConvention();
				_configuration = new MapperConfiguration<Foo, Bar>();
			}

			private NameAndTypeMatchMappingConvention _systemUnderTest;
			private MapperConfiguration<Foo, Bar> _configuration;

			#region Test types

			public enum Sex
			{
				Male,
				Female
			}

			public class Generic<T>
			{
				public T Value
				{
					get;
					set;
				}
			}

			public class NonGeneric
			{
				public string Attribute
				{
					get;
					set;
				}
			}

			public class FooBase
			{
				public string Name
				{
					get;
					set;
				}

				public string Employer
				{
					get;
					set;
				}
			}

			public class Foo : FooBase
			{
				public int Age
				{
					get;
					set;
				}

				public Sex Sex
				{
					get;
					set;
				}

				public Generic<int> Generic
				{
					get;
					set;
				}

				public NonGeneric NonGeneric
				{
					get;
					set;
				}

				public string DiffTypes
				{
					get;
					set;
				}

				public int DesiredTypeNotSettable
				{
					get;
					set;
				}
			}

			public class Bar
			{
				public string Name
				{
					get;
					set;
				}

				public string Employer2
				{
					get;
					set;
				}

				public int Age
				{
					get;
					set;
				}

				public Sex Sex
				{
					get;
					set;
				}

				public Generic<int> Generic
				{
					get;
					set;
				}

				public NonGeneric NonGeneric
				{
					get;
					set;
				}

				public int DiffTypes
				{
					get;
					set;
				}

				public int DesiredTypeNotSettable
				{
					get;
					// ReSharper disable UnusedAutoPropertyAccessor.Local
					private set;
					// ReSharper restore UnusedAutoPropertyAccessor.Local
				}
			}

			public interface IBarBaseBase
			{
				string Name
				{
					get;
					set;
				}

				int Age
				{
					get;
					set;
				}
			}

			public interface IBarBase : IBarBaseBase
			{
				Sex Sex
				{
					get;
					set;
				}
			}

			public interface IBar : IBarBase
			{
				Generic<int> Generic
				{
					get;
					set;
				}

				NonGeneric NonGeneric
				{
					get;
					set;
				}

				string DiffTypes
				{
					get;
					set;
				}

				int DesiredTypeNotSettable
				{
					get;
				}
			}

			#endregion

			[Test]
			public void Must_configure_mappings_for_properties_that_can_be_sourced_from_implemented_interfaces()
			{
				var fromInterfaceConfiguration = new MapperConfiguration<IBar, Bar>();
				_systemUnderTest.Apply(new MapperConventionEligiblePropertyFinder(), fromInterfaceConfiguration);

				Assert.That(fromInterfaceConfiguration.Mappings.CountEqual(5), Is.True);
				Assert.That(fromInterfaceConfiguration.Mappings.Where(arg => arg.MemberName == "Name").CountEqual(1), Is.True);
				Assert.That(fromInterfaceConfiguration.Mappings.Where(arg => arg.MemberName == "Age").CountEqual(1), Is.True);
				Assert.That(fromInterfaceConfiguration.Mappings.Where(arg => arg.MemberName == "Sex").CountEqual(1), Is.True);
				Assert.That(fromInterfaceConfiguration.Mappings.Where(arg => arg.MemberName == "Generic").CountEqual(1), Is.True);
				Assert.That(fromInterfaceConfiguration.Mappings.Where(arg => arg.MemberName == "NonGeneric").CountEqual(1), Is.True);
			}

			[Test]
			public void Must_configure_mappings_for_properties_with_same_name_and_type()
			{
				_systemUnderTest.Apply(new MapperConventionEligiblePropertyFinder(), _configuration);

				Assert.That(_configuration.Mappings.CountEqual(5), Is.True);
				Assert.That(_configuration.Mappings.Where(arg => arg.MemberName == "Name").CountEqual(1), Is.True);
				Assert.That(_configuration.Mappings.Where(arg => arg.MemberName == "Age").CountEqual(1), Is.True);
				Assert.That(_configuration.Mappings.Where(arg => arg.MemberName == "Sex").CountEqual(1), Is.True);
				Assert.That(_configuration.Mappings.Where(arg => arg.MemberName == "Generic").CountEqual(1), Is.True);
				Assert.That(_configuration.Mappings.Where(arg => arg.MemberName == "NonGeneric").CountEqual(1), Is.True);
			}

			[Test]
			public void Must_not_configure_mappings_for_properties_that_are_not_settable()
			{
				_systemUnderTest.Apply(new MapperConventionEligiblePropertyFinder(), _configuration);

				Assert.That(_configuration.Mappings.Any(arg => arg.MemberName == "DesiredTypeNotSettable"), Is.False);
			}

			[Test]
			public void Must_not_configure_mappings_for_properties_with_different_names()
			{
				_systemUnderTest.Apply(new MapperConventionEligiblePropertyFinder(), _configuration);

				Assert.That(_configuration.Mappings.Any(arg => arg.MemberName == "Employer"), Is.False);
				Assert.That(_configuration.Mappings.Any(arg => arg.MemberName == "Employer2"), Is.False);
			}

			[Test]
			public void Must_not_configure_mappings_for_properties_with_same_name_but_different_type()
			{
				_systemUnderTest.Apply(new MapperConventionEligiblePropertyFinder(), _configuration);

				Assert.That(_configuration.Mappings.Any(arg => arg.MemberName == "DiffTypes"), Is.False);
			}
		}
	}
}