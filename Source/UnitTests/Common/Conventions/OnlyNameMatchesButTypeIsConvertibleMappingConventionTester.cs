using System;
using System.ComponentModel;
using System.Linq;

using Junior.Map.Adapter;
using Junior.Map.Common.Conventions;

using NUnit.Framework;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Junior.Map.UnitTests.Common.Conventions
{
	public static class OnlyNameMatchesButTypeIsConvertibleMappingConventionTester
	{
		[TestFixture]
		public class When_applying_OnlyNameMatchesButTypeIsConvertibleMappingConvention
		{
			[SetUp]
			public void Setup()
			{
				_systemUnderTest = new OnlyNameMatchesButTypeIsConvertibleMappingConvention();
				_configuration = new MappingConfiguration<Foo, IFoo>();
				_systemUnderTest.Apply(new AdapterConventionEligiblePropertyFinder(), _configuration);
			}

			private OnlyNameMatchesButTypeIsConvertibleMappingConvention _systemUnderTest;
			private MappingConfiguration<Foo, IFoo> _configuration;

			#region Test types

			public class ConvertibleToDesiredType1Converter : TypeConverter
			{
				public override bool CanConvertTo(ITypeDescriptorContext context, Type sourceType)
				{
					return (sourceType == typeof(DesiredType1));
				}
			}

			[TypeConverter(typeof(ConvertibleToDesiredType1Converter))]
			public class ConvertibleToDesiredType1
			{
			}

			public class ConvertibleByDesiredType2
			{
			}

			public class NotConvertible
			{
			}

			public class DesiredType1
			{
			}

			public class DesiredType2Converter : TypeConverter
			{
				public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				{
					return (sourceType == typeof(ConvertibleByDesiredType2));
				}
			}

			[TypeConverter(typeof(DesiredType2Converter))]
			public class DesiredType2
			{
			}

			public class Foo
			{
				public ConvertibleToDesiredType1 DesiredType1
				{
					get;
					set;
				}

				public ConvertibleByDesiredType2 DesiredType2
				{
					get;
					set;
				}

				public NotConvertible NotConvertible
				{
					get;
					set;
				}

				public ConvertibleToDesiredType1 NonMatchingName
				{
					get;
					set;
				}
			}

			public interface IFoo
			{
				DesiredType1 DesiredType1
				{
					get;
				}

				DesiredType2 DesiredType2
				{
					get;
				}

				int NotConvertible
				{
					get;
				}

				DesiredType1 MismatchedName
				{
					get;
				}
			}

			#endregion

			[Test]
			public void Must_map_if_desired_type_can_convert_from_source_type()
			{
				Assert.That(_configuration.Mappings.Any(arg => arg.MemberName == "DesiredType2"), Is.True);
			}

			[Test]
			public void Must_map_if_source_type_can_be_converted_to_desired_type()
			{
				Assert.That(_configuration.Mappings.Any(arg => arg.MemberName == "DesiredType1"), Is.True);
			}

			[Test]
			public void Must_not_map_if_names_do_not_match()
			{
				Assert.That(_configuration.Mappings.Any(arg => arg.MemberName == "MismatchedName"), Is.False);
			}

			[Test]
			public void Must_not_map_if_types_are_not_convertible()
			{
				Assert.That(_configuration.Mappings.Any(arg => arg.MemberName == "NotConvertible"), Is.False);
			}
		}
	}
}