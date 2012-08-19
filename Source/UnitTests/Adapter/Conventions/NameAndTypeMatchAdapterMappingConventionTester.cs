using System.Linq;

using Junior.Common;
using Junior.Map.Adapter.Conventions;
using Junior.Map.Mapper;

using NUnit.Framework;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Junior.Map.UnitTests.Adapter.Conventions
{
	public static class NameAndTypeMatchAdapterMappingConventionTester
	{
		[TestFixture]
		public class When_applying_NameAndTypeMatchMappingConvention
		{
			[SetUp]
			public void Setup()
			{
				_systemUnderTest = new NameAndTypeMatchAdapterMappingConvention();
				_configuration = new MappingConfiguration<Foo, Bar>();
			}

			private NameAndTypeMatchAdapterMappingConvention _systemUnderTest;
			private MappingConfiguration<Foo, Bar> _configuration;

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

			public class Foo
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
			}

			#endregion

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