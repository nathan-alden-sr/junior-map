using System;

using Junior.Common;
using Junior.Mapping.Adapter;

using NUnit.Framework;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Junior.Mapping.UnitTests.Adapter
{
	public static class AdapterFactoryTester
	{
		#region Test types

		public class SimpleRefType
		{
			public Guid Id
			{
				get;
				set;
			}
		}

		#endregion

		[TestFixture]
		public class When_adapting_from_one_type_to_another_using_default_convention_based_factory
		{
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

				public int C
				{
					get;
					set;
				}

				public SimpleRefType RefType
				{
					get;
					set;
				}
			}

			public interface IBar
			{
				string A
				{
					get;
				}

				string B
				{
					get;
				}

				int C
				{
					get;
				}

				SimpleRefType RefType
				{
					get;
				}
			}

			#endregion

			[Test]
			public void Must_adapt_properties_in_target_type()
			{
				var systemUnderTest = new DefaultAdapterFactory<Foo, IBar>(new DefaultAdapterFactoryLocator());
				var sourceFoo = new Foo
				                	{
				                		A = "SourceA",
				                		B = "SourceB",
				                		C = 1,
				                		RefType = new SimpleRefType
				                		          	{
				                		          		Id = Guid.NewGuid()
				                		          	}
				                	};
				IBar bar = systemUnderTest.Create(sourceFoo);

				Assert.That(bar.A, Is.EqualTo(sourceFoo.A));
				Assert.That(bar.B, Is.EqualTo(sourceFoo.B));
				Assert.That(bar.C, Is.EqualTo(sourceFoo.C));
				Assert.That(bar.RefType, Is.Not.Null);
				Assert.That(bar.RefType.Id, Is.Not.Null);
				Assert.That(bar.RefType.Id, Is.EqualTo(sourceFoo.RefType.Id));
			}

			[Test]
			[Explicit("Interactive performance test")]
			public void Must_perform_acceptably()
			{
				var sourceFoo = new Foo
				                	{
				                		A = "SourceA",
				                		B = "SourceB",
				                		C = 1,
				                		RefType = new SimpleRefType
				                		          	{
				                		          		Id = Guid.NewGuid()
				                		          	}
				                	};
				const int numberOfMappings = 10000000;
				var systemUnderTest = new DefaultAdapterFactory<Foo, IBar>(new DefaultAdapterFactoryLocator());
				var milliseconds = (long)StopwatchContext.Timed(() =>
				                                                	{
				                                                		for (int i = 0; i < numberOfMappings; i++)
				                                                		{
				                                                			IBar bar = systemUnderTest.Create(sourceFoo);
				                                                			// ReSharper disable UnusedVariable
				                                                			string a = bar.A;
				                                                			string b = bar.B;
				                                                			int c = bar.C;
				                                                			SimpleRefType refType = bar.RefType;
				                                                			// ReSharper restore UnusedVariable
				                                                		}
				                                                	}).TotalMilliseconds;

				Console.WriteLine(String.Format("{0:0.000000000000000}", milliseconds / numberOfMappings));
			}
		}

		[TestFixture]
		public class When_adapting_from_one_type_to_another_using_derived_convention_based_factory
		{
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

				public int C
				{
					get;
					set;
				}

				public SimpleRefType RefType
				{
					get;
					set;
				}

				public string SomeString
				{
					get;
					set;
				}
			}

			public interface IBar
			{
				string likeA
				{
					get;
				}

				string likeB
				{
					get;
				}

				int likeC
				{
					get;
				}

				SimpleRefType likeRefType
				{
					get;
				}

				string SomeString
				{
					get;
				}
			}

			private class FooBarAdapterFactory : DefaultAdapterFactory<Foo, IBar>
			{
				public FooBarAdapterFactory()
					: base(new DefaultAdapterFactoryLocator())
				{
				}

				protected override void ConfigureCustomMapping(AdapterFactoryConfiguration<Foo, IBar> configuration)
				{
					configuration.Map(target => target.likeA).From(foo => foo.A);
					configuration.Map(target => target.likeB).From(foo => foo.B);
					configuration.Map(target => target.likeC).From(foo => foo.C);
					configuration.Map(target => target.likeRefType).From(foo => foo.RefType);
				}
			}

			#endregion

			[Test]
			public void Must_assign_all_configured_properties_and_run_custom_actions()
			{
				var sourceFoo = new Foo
				                	{
				                		A = "SourceA",
				                		B = "SourceB",
				                		C = 1,
				                		RefType = new SimpleRefType
				                		          	{
				                		          		Id = Guid.NewGuid()
				                		          	},
				                		SomeString = "FooSomeString"
				                	};
				var systemUnderTest = new FooBarAdapterFactory();
				IBar bar = systemUnderTest.Create(sourceFoo);

				Assert.That(bar.likeA, Is.EqualTo(sourceFoo.A));
				Assert.That(bar.likeB, Is.EqualTo(sourceFoo.B));
				Assert.That(bar.likeC, Is.EqualTo(sourceFoo.C));
				Assert.That(bar.likeRefType, Is.Not.Null);
				Assert.That(bar.SomeString, Is.EqualTo(sourceFoo.SomeString));
				Assert.That(bar.likeRefType.Id, Is.Not.Null);
				Assert.That(bar.likeRefType.Id, Is.EqualTo(sourceFoo.RefType.Id));
			}
		}

		[TestFixture]
		public class When_adapting_to_a_type_with_nested_types_using_default_convention_based_factory
		{
			#region Test types

			public class Person
			{
				public Employer Employer
				{
					get;
					set;
				}

				public string Name
				{
					get;
					set;
				}
			}

			public class Employer
			{
				public string Name
				{
					get;
					set;
				}

				public Address Address
				{
					get;
					set;
				}
			}

			public class Address
			{
				public string Street
				{
					get;
					set;
				}

				public string City
				{
					get;
					set;
				}
			}

			public interface IPerson
			{
				string Name
				{
					get;
				}
				IEmployer Employer
				{
					get;
				}
			}

			public interface IEmployer
			{
				string Name
				{
					get;
				}
				IAddress Address
				{
					get;
				}
			}

			public interface IAddress
			{
				string Street
				{
					get;
				}
				string City
				{
					get;
				}
			}

			#endregion

			[Test]
			public void Must_attempt_to_adapt_nested_types()
			{
				var person = new Person
				             	{
				             		Name = "Joe",
				             		Employer = new Employer
				             		           	{
				             		           		Name = "Microsoft",
				             		           		Address = new Address
				             		           		          	{
				             		           		          		Street = "1 Microsoft Way",
				             		           		          		City = "Redmond"
				             		           		          	}
				             		           	}
				             	};
				var systemUnderTest = new DefaultAdapterFactory<Person, IPerson>(new DefaultAdapterFactoryLocator());

				systemUnderTest.Validate();

				IPerson personInterface = systemUnderTest.Create(person);

				Assert.That(personInterface.Employer.Name, Is.EqualTo(person.Employer.Name));
				Assert.That(personInterface.Employer.Address.Street, Is.EqualTo(person.Employer.Address.Street));
				Assert.That(personInterface.Employer.Address.City, Is.EqualTo(person.Employer.Address.City));
			}
		}
	}
}