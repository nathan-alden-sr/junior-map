using Junior.Common;
using Junior.Map.Mapper;
using Junior.Map.Mapper.Conventions;

using NUnit.Framework;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Junior.Map.UnitTests.Adapter.Conventions
{
	public static class NamesMatchAndTypeIsEnumMappingConventionTester
	{
		[TestFixture]
		public class When_applied_to_type_with_matching_property_names_and_similar_enums
		{
			#region Test types

			public enum Color
			{
				Red,
				Green,
				Blue
			}

			public enum ColorType
			{
				Red,
				Green,
				Blue
			}

			public class Foo
			{
				public Color Color
				{
					get;
					set;
				}
			}

			public class FooNullable
			{
				public Color? NullableColor
				{
					get;
					set;
				}
			}

			public class Bar
			{
				public ColorType Color
				{
					get;
					set;
				}
			}

			public class BarNullable
			{
				public ColorType? NullableColor
				{
					get;
					set;
				}
			}

			#endregion

			[Test]
			public void Must_configure_mapping_that_delegates_to_DefaultEnumerationMapper_for_matching_non_nullable_properties()
			{
				var configuration = new MapperConfiguration<Foo, Bar>();
				var systemUnderTest = new NamesMatchAndTypeIsEnumMappingConvention();

				systemUnderTest.Apply(new MapperConventionEligiblePropertyFinder(), configuration);

				Assert.That(configuration.Mappings.CountEqual(1), Is.True);
			}

			[Test]
			public void Must_configure_mapping_that_delegates_to_DefaultEnumerationMapper_for_matching_nullable_properties()
			{
				var configuration = new MapperConfiguration<FooNullable, BarNullable>();
				var systemUnderTest = new NamesMatchAndTypeIsEnumMappingConvention();

				systemUnderTest.Apply(new MapperConventionEligiblePropertyFinder(), configuration);

				Assert.That(configuration.Mappings.CountEqual(1), Is.True);
			}
		}
	}
}