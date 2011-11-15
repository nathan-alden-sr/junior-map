using Junior.Map.Mapper;

using NUnit.Framework;

namespace Junior.Map.UnitTests.Mapper
{
	public static class DefaultMapperLocatorTester
	{
		[TestFixture]
		public class When_used_using_Type_instances
		{
			#region Test types

			public class Foo
			{
			}

			public class Bar
			{
			}

			public class FooBar
			{
			}

			#endregion

			[Test]
			public void Must_return_new_instance_of_DefaultMapper_for_the_requested_types()
			{
				var systemUnderTest = new DefaultMapperLocator();
				object mapper = systemUnderTest.Locate(typeof(Foo), typeof(Bar));

				Assert.That(mapper, Is.InstanceOf<DefaultMapper<Foo, Bar>>());
			}

			[Test]
			public void Must_return_same_instance_on_second_request()
			{
				var systemUnderTest = new DefaultMapperLocator();
				object mapper = systemUnderTest.Locate(typeof(FooBar), typeof(Bar));
				object mapper2 = systemUnderTest.Locate(typeof(FooBar), typeof(Bar));

				Assert.That(mapper, Is.InstanceOf<DefaultMapper<FooBar, Bar>>());
				Assert.That(mapper2, Is.SameAs(mapper));
			}
		}

		[TestFixture]
		public class When_used_using_type_parameters
		{
			#region Test types

			public class Foo
			{
			}

			public class Bar
			{
			}

			public class FooBar
			{
			}

			#endregion

			[Test]
			public void Must_return_new_instance_of_DefaultMapper_for_the_requested_types()
			{
				var systemUnderTest = new DefaultMapperLocator();
				IMapper<Foo, Bar> mapper = systemUnderTest.Locate<Foo, Bar>();

				Assert.That(mapper, Is.InstanceOf<DefaultMapper<Foo, Bar>>());
			}

			[Test]
			public void Must_return_same_instance_on_second_request()
			{
				var systemUnderTest = new DefaultMapperLocator();
				IMapper<FooBar, Bar> mapper = systemUnderTest.Locate<FooBar, Bar>();
				IMapper<FooBar, Bar> mapper2 = systemUnderTest.Locate<FooBar, Bar>();

				Assert.That(mapper, Is.InstanceOf<DefaultMapper<FooBar, Bar>>());
				Assert.That(mapper2, Is.SameAs(mapper));
			}
		}
	}
}