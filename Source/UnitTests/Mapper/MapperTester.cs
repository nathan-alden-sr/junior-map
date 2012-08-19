using System;
using System.Collections.Generic;

using Junior.Common;
using Junior.Map.Mapper;

using NUnit.Framework;

using Rhino.Mocks;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Junior.Map.UnitTests.Mapper
{
	public static class MapperTester
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

		private class DefaultMapper<T1, T2> : Mapper<T1, T2>
		{
		}

		[TestFixture]
		public class When_mapping_a_type_with_nested_types_using_default_mapper
		{
			#region Test types

			public interface IPerson
			{
				IEmployer Employer
				{
					get;
				}

				string Name
				{
					get;
					set;
				}

				IAddress Address
				{
					get;
				}
			}

			public interface IPersonAddress : IAddress
			{
				IPerson Person
				{
					get;
				}
			}

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

				public PersonAddress Address
				{
					get;
					set;
				}
			}

			public interface IEmployer
			{
				string Name
				{
					get;
					set;
				}

				IAddress Address
				{
					get;
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

			public interface IAddress
			{
				string Street
				{
					get;
					set;
				}

				string City
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

			public class PersonAddress : Address
			{
				private readonly Person person;

				public PersonAddress(Person person)
				{
					this.person = person;
				}

				public Person Person
				{
					get
					{
						return person;
					}
				}
			}

			#endregion

			[Test]
			public void Must_map_the_nested_type_too_by_straight_assignment_when_settable_and_types_are_the_same()
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

				person.Address = new PersonAddress(person)
					{
						Street = "123 Main",
						City = "Dallas"
					};

				var systemUnderTest = new DefaultMapper<Person, Person>();

				systemUnderTest.Validate();

				var person2 = new Person();

				systemUnderTest.Map(person, person2);

				Assert.That(person2.Name, Is.EqualTo(person.Name));
				Assert.That(person2.Employer, Is.SameAs(person.Employer));
				Assert.That(person2.Employer.Name, Is.EqualTo(person.Employer.Name));
				Assert.That(person2.Employer.Address, Is.SameAs(person.Employer.Address));
				Assert.That(person2.Employer.Address.Street, Is.EqualTo(person.Employer.Address.Street));
				Assert.That(person2.Employer.Address.City, Is.EqualTo(person.Employer.Address.City));
				Assert.That(person2.Address, Is.SameAs(person.Address));
				Assert.That(person2.Address.Street, Is.EqualTo(person.Address.Street));
				Assert.That(person2.Address.City, Is.EqualTo(person.Address.City));
			}

			[Test]
			public void Must_not_map_through_nested_objects_by_convention_if_types_differ()
			{
				var systemUnderTest = new DefaultMapper<Person, IPerson>();

				Assert.Throws<Exception>(systemUnderTest.Validate, "A mapping was not provided for target property 'IPerson.Employer' from source 'Person'.");
			}
		}

		[TestFixture]
		public class When_mapping_from_one_type_to_another_using_derived_bidirectional_mapper
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

			public class Bar
			{
				public string likeA
				{
					get;
					set;
				}

				public string likeB
				{
					get;
					set;
				}

				public int likeC
				{
					get;
					set;
				}

				public SimpleRefType likeRefType
				{
					get;
					set;
				}
			}

			private class FooBarMapper : BidirectionalMapper<Foo, Bar>
			{
				protected override void ConfigureCustomMapping(MapperConfiguration<Foo, Bar> configuration)
				{
					configuration.Map(target => target.likeA).From(foo => foo.A);
					configuration.Map(target => target.likeB).From(foo => foo.B);
					configuration.Map(target => target.likeC).From(foo => foo.C);
					configuration.Map(target => target.likeRefType).From(foo => foo.RefType);
				}

				protected override void ConfigureCustomMapping(MapperConfiguration<Bar, Foo> configuration)
				{
					configuration.Map(target => target.A).From(bar => bar.likeA);
					configuration.Map(target => target.B).From(bar => bar.likeB);
					configuration.Map(target => target.C).From(bar => bar.likeC);
					configuration.Map(target => target.RefType).From(bar => bar.likeRefType);
				}
			}

			#endregion

			[Test]
			public void Must_assign_all_settable_properties()
			{
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.A = "SourceA";
				sourceFoo.B = "SourceB";
				sourceFoo.C = 1;
				sourceFoo.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};

				targetBar.likeA = "WrongA";
				targetBar.likeB = "WrongB";
				targetBar.likeC = 0;
				targetBar.likeRefType = null;

				var systemUnderTest = new FooBarMapper();

				systemUnderTest.Map(sourceFoo, targetBar);

				Assert.That(targetBar.likeA, Is.EqualTo(sourceFoo.A));
				Assert.That(targetBar.likeB, Is.EqualTo(sourceFoo.B));
				Assert.That(targetBar.likeC, Is.EqualTo(sourceFoo.C));
				Assert.That(targetBar.likeRefType, Is.Not.Null);
				Assert.That(targetBar.likeRefType.Id, Is.Not.Null);
				Assert.That(targetBar.likeRefType.Id, Is.EqualTo(sourceFoo.RefType.Id));
			}

			[Test]
			public void Must_assign_all_settable_properties_picked_up_by_convention_in_reverse_case_too()
			{
				var targetFoo = new Foo();
				var sourceBar = new Bar();

				targetFoo.A = "FooA";
				targetFoo.B = "FooB";
				targetFoo.C = 1;
				targetFoo.RefType = null;

				sourceBar.likeA = "BarA";
				sourceBar.likeB = "BarB";
				sourceBar.likeC = 0;
				sourceBar.likeRefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};

				var systemUnderTest = new FooBarMapper();

				systemUnderTest.Map(sourceBar, targetFoo);

				Assert.That(targetFoo.A, Is.EqualTo(sourceBar.likeA));
				Assert.That(targetFoo.B, Is.EqualTo(sourceBar.likeB));
				Assert.That(targetFoo.C, Is.EqualTo(sourceBar.likeC));
				Assert.That(targetFoo.RefType, Is.Not.Null);
				Assert.That(targetFoo.RefType.Id, Is.Not.Null);
				Assert.That(targetFoo.RefType.Id, Is.EqualTo(sourceBar.likeRefType.Id));
			}

			[Test]
			[Explicit("Interactive performance test")]
			public void Must_perform_acceptably()
			{
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.A = "SourceA";
				sourceFoo.B = "SourceB";
				sourceFoo.C = 1;
				sourceFoo.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};

				targetBar.likeA = "WrongA";
				targetBar.likeB = "WrongB";
				targetBar.likeC = 0;
				targetBar.likeRefType = null;

				var systemUnderTest = new FooBarMapper();
				var milliseconds = (long)StopwatchContext.Timed(() =>
					{
						for (int i = 0; i < 10000000; i++)
						{
							systemUnderTest.Map(sourceFoo, targetBar);
						}
					}).TotalMilliseconds;

				Console.WriteLine(milliseconds);
			}
		}

		[TestFixture]
		public class When_mapping_from_one_type_to_another_using_derived_mapper
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

			public class Bar
			{
				private bool _dirty;
				private int _x;
				private int _y;
				private int _z;

				public string likeA
				{
					get;
					set;
				}

				public string likeB
				{
					get;
					set;
				}

				public int likeC
				{
					get;
					set;
				}

				public SimpleRefType likeRefType
				{
					get;
					set;
				}

				public string SomeString
				{
					get;
					private set;
				}

				public DateTime CurrentDateTime
				{
					get
					{
						return DateTime.Now;
					}
				}

				public int CoordinateX
				{
					get
					{
						return _x;
					}
				}

				public int CoordinateY
				{
					get
					{
						return _y;
					}
				}

				public int CoordinateZ
				{
					get
					{
						return _z;
					}
				}

				public bool? ToIgnoreChain1
				{
					get;
					set;
				}

				public bool? ToIgnoreChain2
				{
					get;
					set;
				}

				public bool? ToIgnoreChain3
				{
					get;
					set;
				}

				public bool GetDirty()
				{
					return _dirty;
				}

				public void SetSomeString(string whatString)
				{
					SomeString = whatString;
				}

				public void SetDirty()
				{
					_dirty = true;
				}

				public void SetCoordinates(int x, int y, int z)
				{
					_x = x;
					_y = y;
					_z = z;
				}
			}

			private class FooBarMapper : DefaultMapper<Foo, Bar>
			{
				protected override void ConfigureCustomMapping(MapperConfiguration<Foo, Bar> configuration)
				{
					configuration.Map(target => target.likeA).From(foo => foo.A);
					configuration.Map(target => target.likeB).From(foo => foo.B);
					configuration.Map(target => target.likeC).From(foo => foo.C);
					configuration.Map(target => target.likeRefType).From(foo => foo.RefType);
					configuration.Map(target => target.CurrentDateTime).ByIgnoring();
					configuration.Map(target => target.SomeString).ByInvoking((target, source) => target.SetSomeString(source.SomeString));
					configuration.AddCustomActionDelegate((bar, foo) => bar.SetDirty());
					configuration.Map(target => target.CoordinateX)
						.And(target => target.CoordinateY)
						.And(target => target.CoordinateZ)
						.ByInvoking((bar1, foo1) => bar1.SetCoordinates(9, 8, 7));
					configuration.Map(target => target.ToIgnoreChain1)
						.And(target => target.ToIgnoreChain2)
						.And(target => target.ToIgnoreChain3)
						.ByIgnoring();
					configuration.Map(target => target.likeRefType).From(foo2 => foo2.RefType);
				}
			}

			private class FooBarMapperWithInjectedRefTypeMapper : DefaultMapper<Foo, Bar>
			{
				private readonly IMapper<SimpleRefType, SimpleRefType> _refTypeMapper;

				public FooBarMapperWithInjectedRefTypeMapper(IMapper<SimpleRefType, SimpleRefType> refTypeMapper)
				{
					_refTypeMapper = refTypeMapper;
				}

				protected override void ConfigureCustomMapping(MapperConfiguration<Foo, Bar> configuration)
				{
					configuration.Map(target => target.likeA).From(foo => foo.A);
					configuration.Map(target => target.likeB).From(foo => foo.B);
					configuration.Map(target => target.likeC).From(foo => foo.C);
					configuration.Map(target => target.CurrentDateTime).ByIgnoring();
					configuration.Map(target => target.SomeString).ByInvoking((target, source) => target.SetSomeString(source.SomeString));
					configuration.AddCustomActionDelegate((bar, foo) => bar.SetDirty());
					configuration.Map(target => target.CoordinateX)
						.And(target => target.CoordinateY)
						.And(target => target.CoordinateZ)
						.ByInvoking((bar1, foo1) => bar1.SetCoordinates(9, 8, 7));
					configuration.Map(target => target.ToIgnoreChain1)
						.And(target => target.ToIgnoreChain2)
						.And(target => target.ToIgnoreChain3)
						.ByIgnoring();
					configuration.Map(target => target.likeRefType).ByDelegatingTo(foo2 => foo2.RefType, _refTypeMapper);
				}
			}

			#endregion

			[Test]
			public void Must_assign_all_configured_properties_and_invoke_action_delegates()
			{
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.A = "SourceA";
				sourceFoo.B = "SourceB";
				sourceFoo.C = 1;
				sourceFoo.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};
				sourceFoo.SomeString = "FooSomeString";

				targetBar.likeA = "WrongA";
				targetBar.likeB = "WrongB";
				targetBar.likeC = 0;
				targetBar.likeRefType = null;

				var systemUnderTest = new FooBarMapper();

				systemUnderTest.Map(sourceFoo, targetBar);

				Assert.That(targetBar.likeA, Is.EqualTo(sourceFoo.A));
				Assert.That(targetBar.likeB, Is.EqualTo(sourceFoo.B));
				Assert.That(targetBar.likeC, Is.EqualTo(sourceFoo.C));
				Assert.That(targetBar.likeRefType, Is.Not.Null);
				Assert.That(targetBar.SomeString, Is.EqualTo(sourceFoo.SomeString));
				Assert.That(targetBar.likeRefType.Id, Is.Not.Null);
				Assert.That(targetBar.likeRefType.Id, Is.EqualTo(sourceFoo.RefType.Id));
				Assert.That(targetBar.GetDirty(), Is.True);
				Assert.That(targetBar.CoordinateX, Is.EqualTo(9));
				Assert.That(targetBar.CoordinateY, Is.EqualTo(8));
				Assert.That(targetBar.CoordinateZ, Is.EqualTo(7));
				Assert.That(targetBar.ToIgnoreChain1, Is.Null);
				Assert.That(targetBar.ToIgnoreChain2, Is.Null);
				Assert.That(targetBar.ToIgnoreChain3, Is.Null);
			}

			[Test]
			public void Must_delegate_to_provided_mapper_when_source_is_not_null()
			{
				var simpleRefTypeMapper = MockRepository.GenerateMock<IMapper<SimpleRefType, SimpleRefType>>();
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};
				targetBar.likeRefType = null;

				var systemUnderTest = new FooBarMapperWithInjectedRefTypeMapper(simpleRefTypeMapper);

				systemUnderTest.Map(sourceFoo, targetBar);

				simpleRefTypeMapper.AssertWasCalled(arg => arg.Map(Arg<SimpleRefType>.Is.Same(sourceFoo.RefType), Arg<SimpleRefType>.Is.NotNull));

				Assert.That(targetBar.likeRefType, Is.Not.Null);
			}

			[Test]
			public void Must_not_delegate_to_injected_mapper_when_source_is_null()
			{
				var simpleRefTypeMapper = MockRepository.GenerateMock<IMapper<SimpleRefType, SimpleRefType>>();
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.RefType = null;
				targetBar.likeRefType = null;

				var systemUnderTest = new FooBarMapperWithInjectedRefTypeMapper(simpleRefTypeMapper);

				systemUnderTest.Map(sourceFoo, targetBar);

				simpleRefTypeMapper.AssertWasNotCalled(arg => arg.Map(Arg<SimpleRefType>.Is.Anything, Arg<SimpleRefType>.Is.Anything));

				Assert.That(targetBar.likeRefType, Is.Null);
			}

			[Test]
			[Explicit("Interactive performance test")]
			public void Must_perform_acceptably()
			{
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.A = "SourceA";
				sourceFoo.B = "SourceB";
				sourceFoo.C = 1;
				sourceFoo.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};

				targetBar.likeA = "WrongA";
				targetBar.likeB = "WrongB";
				targetBar.likeC = 0;
				targetBar.likeRefType = null;

				var systemUnderTest = new FooBarMapper();
				var milliseconds = (long)StopwatchContext.Timed(() =>
					{
						for (int i = 0; i < 10000000; i++)
						{
							systemUnderTest.Map(sourceFoo, targetBar);
						}
					}).TotalMilliseconds;

				Console.WriteLine(milliseconds);
			}
		}

		[TestFixture]
		public class When_mapping_from_one_type_to_another_using_standard_bidirectional_mapper
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

			private class DefaultBidirectionalMapper<T1, T2> : BidirectionalMapper<T1, T2>
			{
			}

			#endregion

			[Test]
			public void Must_assign_all_settable_properties_picked_up_by_convention()
			{
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.A = "SourceA";
				sourceFoo.B = "SourceB";
				sourceFoo.C = 1;
				sourceFoo.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};

				targetBar.A = "WrongA";
				targetBar.B = "WrongB";
				targetBar.C = 0;
				targetBar.RefType = null;

				var systemUnderTest = new DefaultBidirectionalMapper<Foo, Bar>();

				systemUnderTest.Map(sourceFoo, targetBar);

				Assert.That(targetBar.A, Is.EqualTo(sourceFoo.A));
				Assert.That(targetBar.B, Is.EqualTo(sourceFoo.B));
				Assert.That(targetBar.C, Is.EqualTo(sourceFoo.C));
				Assert.That(targetBar.RefType, Is.Not.Null);
				Assert.That(targetBar.RefType.Id, Is.Not.Null);
				Assert.That(targetBar.RefType.Id, Is.EqualTo(sourceFoo.RefType.Id));
			}

			[Test]
			public void Must_assign_all_settable_properties_picked_up_by_convention_in_reverse_case_too()
			{
				var targetFoo = new Foo();
				var sourceBar = new Bar();

				targetFoo.A = "FooA";
				targetFoo.B = "FooB";
				targetFoo.C = 1;
				targetFoo.RefType = null;

				sourceBar.A = "BarA";
				sourceBar.B = "BarB";
				sourceBar.C = 0;
				sourceBar.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};

				var systemUnderTest = new DefaultBidirectionalMapper<Foo, Bar>();

				systemUnderTest.Map(sourceBar, targetFoo);

				Assert.That(targetFoo.A, Is.EqualTo(sourceBar.A));
				Assert.That(targetFoo.B, Is.EqualTo(sourceBar.B));
				Assert.That(targetFoo.C, Is.EqualTo(sourceBar.C));
				Assert.That(targetFoo.RefType, Is.Not.Null);
				Assert.That(targetFoo.RefType.Id, Is.Not.Null);
				Assert.That(targetFoo.RefType.Id, Is.EqualTo(sourceBar.RefType.Id));
			}
		}

		[TestFixture]
		public class When_mapping_from_one_type_to_another_using_standard_one_way_mapper
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
				Blue,
				Red,
				Green
			}

			public class Foo
			{
				public Foo()
				{
					Integers = new List<int>();
				}

				public Color? NullableColor
				{
					get;
					set;
				}

				public Color Color
				{
					get;
					set;
				}

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

				public List<int> Integers
				{
					get;
					set;
				}
			}

			public class Bar
			{
				public ColorType? NullableColor
				{
					get;
					set;
				}

				public ColorType Color
				{
					get;
					set;
				}

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

			public class NonAutoMappableBar
			{
				public NonAutoMappableBar()
				{
					Integers = new List<int>();
				}

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

				public List<int> Integers
				{
					get;
					private set;
				}
			}

			public class BarWithExtraProperty : Bar
			{
				public decimal Amount
				{
					get;
					set;
				}
			}

			#endregion

			[Test]
			public void Must_assign_all_settable_properties_picked_up_by_convention()
			{
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.A = "SourceA";
				sourceFoo.B = "SourceB";
				sourceFoo.C = 1;
				sourceFoo.Color = Color.Red;
				sourceFoo.NullableColor = null;
				sourceFoo.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};

				targetBar.A = "WrongA";
				targetBar.B = "WrongB";
				targetBar.C = 0;
				targetBar.Color = ColorType.Blue;
				targetBar.NullableColor = ColorType.Green;
				targetBar.RefType = null;

				var systemUnderTest = new DefaultMapper<Foo, Bar>();

				systemUnderTest.Map(sourceFoo, targetBar);

				Assert.That(targetBar.A, Is.EqualTo(sourceFoo.A));
				Assert.That(targetBar.B, Is.EqualTo(sourceFoo.B));
				Assert.That(targetBar.C, Is.EqualTo(sourceFoo.C));
				Assert.That(targetBar.Color, Is.EqualTo(ColorType.Red));
				Assert.That(targetBar.NullableColor, Is.EqualTo(null));
				Assert.That(targetBar.RefType, Is.Not.Null);
				Assert.That(targetBar.RefType.Id, Is.Not.Null);
				Assert.That(targetBar.RefType.Id, Is.EqualTo(sourceFoo.RefType.Id));
			}

			[Test]
			[Explicit("Interactive performance test")]
			public void Must_perform_acceptably()
			{
				var sourceFoo = new Foo();
				var targetBar = new Bar();

				sourceFoo.A = "SourceA";
				sourceFoo.B = "SourceB";
				sourceFoo.C = 1;
				sourceFoo.RefType = new SimpleRefType
					{
						Id = Guid.NewGuid()
					};

				targetBar.A = "WrongA";
				targetBar.B = "WrongB";
				targetBar.C = 0;
				targetBar.RefType = null;

				const int numberOfMappings = 10000000;
				var systemUnderTest = new DefaultMapper<Foo, Bar>();
				var milliseconds = (long)StopwatchContext.Timed(() =>
					{
						for (int i = 0; i < numberOfMappings; i++)
						{
							systemUnderTest.Map(sourceFoo, targetBar);
						}
					}).TotalMilliseconds;

				Console.WriteLine("{0:0.000000000000000}", milliseconds / numberOfMappings);
			}

			[Test]
			public void Must_throw_exception_when_properties_on_TTarget_are_not_accounted_for()
			{
				var systemUnderTest = new DefaultMapper<Foo, NonAutoMappableBar>();

				Assert.Throws<Exception>(systemUnderTest.Validate, "A mapping was not provided for target property 'Bar.Integers' from source 'Foo'.");
			}
		}
	}
}