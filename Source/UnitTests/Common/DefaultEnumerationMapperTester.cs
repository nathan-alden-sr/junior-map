using System;

using Junior.Common.Net35;
using Junior.Map.Common;

using NUnit.Framework;

namespace Junior.Map.UnitTests.Common
{
	public static class DefaultEnumerationMapperTester
	{
		#region Test types

		public enum BarEnum
		{
			One,
			Two,
			Three,
			Four
		}

		public enum FooEnum
		{
			One,
			Two,
			Three,
			Four
		}

		#endregion

		public class When_instantiated_for_two_different_enumerated_types
		{
			#region Test types

			public enum AlmostSimilarEnum
			{
				One,
				Two,
				C,
				Four
			}

			public enum DifferentEnum
			{
				A,
				B,
				C,
				D
			}

			public enum FourValues
			{
				One,
				Two,
				Three,
				Four
			}

			public enum ThreeValues
			{
				One,
				Two,
				Three
			}

			#endregion

			[Test]
			public void Must_throw_exception_for_valid_mapping_when_nothing_matches()
			{
				var systemUnderTest = new FooEnumEnumerationMapper();

				Assert.Throws<Exception>(
					systemUnderTest.Validate,
					String.Format("Type '{0}' contains unmapped members.{1}A, B, C, D.", typeof(DifferentEnum).FullName, Environment.NewLine));
			}

			[Test]
			public void Must_throw_exception_test_for_valid_mapping_when_it_partially_matches()
			{
				var defaultEnumerationDynamicMapper = new AlmostSimilarEnumEnumerationMapper();

				Assert.Throws<Exception>(
					defaultEnumerationDynamicMapper.Validate,
					String.Format("Type '{0}' contains unmapped members.{1}Three.", typeof(FooEnum).FullName, Environment.NewLine));
			}

			[Test]
			public void Must_throw_exception_if_source_contains_members_not_in_target()
			{
				var defaultEnumerationDynamicMapper = new FourValuesEnumEnumerationMapper();

				Assert.Throws<Exception>(
					defaultEnumerationDynamicMapper.Validate,
					String.Format("Type '{0}' contains unmapped members.{1}Four.", typeof(FooEnum).FullName, Environment.NewLine));
			}

			[Test]
			public void Should_not_throw_if_target_has_all_elements_in_source()
			{
				Assert.DoesNotThrow(() =>
				{
					var mapper = new ThreeValuesEnumerationMapper();
					mapper.Validate();
				});
			}

			[Test]
			public void Should_not_throw_if_target_has_less_elements_than_source_but_custom_mapping_covers_it()
			{
				Assert.DoesNotThrow(() =>
				{
					var mapper = new ThreeToFourCustomValuesEnumerationMapper();
					mapper.Validate();
				});
			}

			private class AlmostSimilarEnumEnumerationMapper : EnumerationMapper<AlmostSimilarEnum, FooEnum>
			{
			}

			private class FooEnumEnumerationMapper : EnumerationMapper<FooEnum, DifferentEnum>
			{
			}

			private class FourValuesEnumEnumerationMapper : EnumerationMapper<FourValues, ThreeValues>
			{
			}

			private class ThreeToFourCustomValuesEnumerationMapper : EnumerationMapper<FourValues, ThreeValues>
			{
				protected override void ConfigureMapper(EnumerationMapperConfiguration<FourValues, ThreeValues> configuration)
				{
					base.ConfigureMapper(configuration);
					configuration.Map(FourValues.Four).To(ThreeValues.One);
				}
			}

			private class ThreeValuesEnumerationMapper : EnumerationMapper<ThreeValues, FourValues>
			{
			}
		}

		public class When_instantiated_for_two_similar_enumerated_types
		{
			[Test]
			public void Must_have_a_valid_mapping()
			{
				var systemUnderTest = new FooEnumEnumerationMapper();

				systemUnderTest.Validate();
			}

			[Test]
			public void Must_map_to_corresponding_enum_value()
			{
				var systemUnderTest = new FooEnumEnumerationMapper();

				Assert.That(systemUnderTest.GetMappedValue(FooEnum.Three), Is.EqualTo(BarEnum.Three));
				Assert.That(systemUnderTest.GetMappedValue(FooEnum.Four), Is.EqualTo(BarEnum.Four));
			}

			private class FooEnumEnumerationMapper : EnumerationMapper<FooEnum, BarEnum>
			{
			}
		}

		public class When_instantiated_for_two_similar_enumerated_types_with_mixed_nullability
		{
			[Test]
			public void Must_throw_exception_in_constructor()
			{
				Assert.Throws<InvalidGenericTypeArgumentException>(() => new FooNullBarEnumerationMapper());
				Assert.Throws<InvalidGenericTypeArgumentException>(() => new FooBarNullEnumerationMapper());
			}

			private class FooBarNullEnumerationMapper : EnumerationMapper<FooEnum, BarEnum?>
			{
			}

			private class FooNullBarEnumerationMapper : EnumerationMapper<FooEnum?, BarEnum>
			{
			}
		}

		public class When_instantiated_for_two_similar_nullable_enumerated_types
		{
			[Test]
			public void Must_have_a_valid_mapping()
			{
				var systemUnderTest = new FooBarNullEnumerationMapper();

				systemUnderTest.Validate();
			}

			[Test]
			public void Must_map_to_corresponding_enum_value()
			{
				var systemUnderTest = new FooBarNullEnumerationMapper();

				BarEnum? mappedValue = systemUnderTest.GetMappedValue(null);

				Assert.That(mappedValue, Is.Null);
				Assert.That(systemUnderTest.GetMappedValue(FooEnum.Three), Is.EqualTo(BarEnum.Three));
				Assert.That(systemUnderTest.GetMappedValue(FooEnum.Four), Is.EqualTo(BarEnum.Four));
			}

			private class FooBarNullEnumerationMapper : EnumerationMapper<FooEnum?, BarEnum?>
			{
			}
		}
	}
}