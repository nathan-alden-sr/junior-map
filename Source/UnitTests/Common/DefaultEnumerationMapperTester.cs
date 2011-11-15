using System;

using Junior.Common;
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

			#endregion

			[Test]
			public void Must_throw_exception_for_valid_mapping_when_nothing_matches()
			{
				var systemUnderTest = new DefaultEnumerationMapper<FooEnum, DifferentEnum>();

				Assert.Throws<Exception>(
					systemUnderTest.Validate,
					String.Format("Type '{0}' contains unmapped members.{1}A, B, C, D.", typeof(DifferentEnum).FullName, Environment.NewLine));
			}

			[Test]
			public void Must_throw_exception_test_for_valid_mapping_when_it_partially_matches()
			{
				var defaultEnumerationDynamicMapper = new DefaultEnumerationMapper<AlmostSimilarEnum, FooEnum>();

				Assert.Throws<Exception>(
					defaultEnumerationDynamicMapper.Validate,
					String.Format("Type '{0}' contains unmapped members.{1}Three.", typeof(FooEnum).FullName, Environment.NewLine));
			}
		}

		public class When_instantiated_for_two_similar_enumerated_types
		{
			[Test]
			public void Must_have_a_valid_mapping()
			{
				var systemUnderTest = new DefaultEnumerationMapper<FooEnum, BarEnum>();

				systemUnderTest.Validate();
			}

			[Test]
			public void Must_map_to_corresponding_enum_value()
			{
				var systemUnderTest = new DefaultEnumerationMapper<FooEnum, BarEnum>();

				Assert.That(systemUnderTest.GetMappedValue(FooEnum.Three), Is.EqualTo(BarEnum.Three));
				Assert.That(systemUnderTest.GetMappedValue(FooEnum.Four), Is.EqualTo(BarEnum.Four));
			}
		}

		public class When_instantiated_for_two_similar_enumerated_types_with_mixed_nullability
		{
			[Test]
			public void Must_throw_exception_in_constructor()
			{
				Assert.Throws<InvalidGenericTypeArgumentException>(() => new DefaultEnumerationMapper<FooEnum?, BarEnum>());
				Assert.Throws<InvalidGenericTypeArgumentException>(() => new DefaultEnumerationMapper<FooEnum, BarEnum?>());
			}
		}

		public class When_instantiated_for_two_similar_nullable_enumerated_types
		{
			[Test]
			public void Must_have_a_valid_mapping()
			{
				var systemUnderTest = new DefaultEnumerationMapper<FooEnum?, BarEnum?>();

				systemUnderTest.Validate();
			}

			[Test]
			public void Must_map_to_corresponding_enum_value()
			{
				var systemUnderTest = new DefaultEnumerationMapper<FooEnum?, BarEnum?>();

				BarEnum? mappedValue = systemUnderTest.GetMappedValue(null);

				Assert.That(mappedValue, Is.Null);
				Assert.That(systemUnderTest.GetMappedValue(FooEnum.Three), Is.EqualTo(BarEnum.Three));
				Assert.That(systemUnderTest.GetMappedValue(FooEnum.Four), Is.EqualTo(BarEnum.Four));
			}
		}
	}
}