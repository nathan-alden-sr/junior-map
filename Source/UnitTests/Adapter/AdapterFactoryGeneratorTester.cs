using System;
using System.Threading;

using Junior.Common;
using Junior.Map.Adapter;

using NUnit.Framework;

namespace Junior.Map.UnitTests.Adapter
{
	public static class AdapterFactoryGeneratorTester
	{
		#region Test types

		public class FooFactor
		{
			public FooFactor(int factor)
			{
				Factor = factor;
			}

			public int Factor
			{
				get;
				private set;
			}
		}

		#endregion

		[TestFixture]
		public class When_generating_adapter_class_factory
		{
			#region Test types

			public class Bar
			{
				public Bar(string name, Guid id)
				{
					Name = name;
					Id = id;
				}

				public string Name
				{
					get;
					private set;
				}

				public Guid Id
				{
					get;
					private set;
				}
			}

			public class Foo
			{
				public Foo(string name, Guid id, int factor)
				{
					Name = name;
					Id = id;
					Factor = new FooFactor(factor);
				}

				public string Name
				{
					get;
					private set;
				}

				public Guid Id
				{
					get;
					private set;
				}

				public FooFactor Factor
				{
					get;
					private set;
				}
			}

			public class HardCodedFooAdapter : IFooView
			{
				private readonly Foo _foo;

				public HardCodedFooAdapter(Foo foo)
				{
					_foo = foo;
				}

				public Guid Id
				{
					get
					{
						return _foo.Id;
					}
				}

				public string Name
				{
					get
					{
						return _foo.Name;
					}
				}

				public FooFactor Factor
				{
					get
					{
						return _foo.Factor;
					}
				}
			}

			public interface IFooView
			{
				Guid Id
				{
					get;
				}

				string Name
				{
					get;
				}

				FooFactor Factor
				{
					get;
				}
			}

			#endregion

			[Test]
			public void Must_not_call_mapping_delegate_for_each_property_before_first_access()
			{
				var fooConfiguration = new MappingConfiguration<Foo, IFooView>();
				bool nameDelegateCalled = false;
				bool idDelegateCalled = false;
				bool factorDelegateCalled = false;

				fooConfiguration.Map(view => view.Name).From(foo =>
					{
						nameDelegateCalled = true;
						return foo.Name;
					});
				fooConfiguration.Map(view => view.Id).From(foo =>
					{
						idDelegateCalled = true;
						return foo.Id;
					});
				fooConfiguration.Map(view => view.Factor).From(foo =>
					{
						factorDelegateCalled = true;
						return foo.Factor;
					});

				var foo1 = new Foo("test", Guid.NewGuid(), 1);
				IAdapterFactory<Foo, IFooView> systemUnderTest = AdapterFactoryGenerator.Instance.Generate<Foo, IFooView>(fooConfiguration.Mappings);
				IFooView fooView = systemUnderTest.Create(foo1);

				Assert.That(nameDelegateCalled, Is.False);
				Assert.That(idDelegateCalled, Is.False);
				Assert.That(factorDelegateCalled, Is.False);

				// ReSharper disable UnusedVariable
				string name = fooView.Name;
				Guid id = fooView.Id;
				int factor = fooView.Factor.Factor;
				// ReSharper restore UnusedVariable

				Assert.That(nameDelegateCalled, Is.True);
				Assert.That(idDelegateCalled, Is.True);
				Assert.That(factorDelegateCalled, Is.True);
			}

			[Test]
			[Explicit("Interactive performance test")]
			public void Must_perform_acceptably()
			{
				var configuration = new MappingConfiguration<Foo, IFooView>();

				configuration.Map(view => view.Name).From(foo => foo.Name);
				configuration.Map(view => view.Id).From(foo => foo.Id);
				configuration.Map(view => view.Factor).From(foo => foo.Factor);

				IAdapterFactory<Foo, IFooView> systemUnderTest = AdapterFactoryGenerator.Instance.Generate<Foo, IFooView>(configuration.Mappings);

				// Delay to allow CPU and I/O to drop
				Thread.Sleep(TimeSpan.FromSeconds(2));

				var foo1 = new Foo("test foo", Guid.NewGuid(), 1);
				const int numberOfInstances = 2000000;
				var mappingServiceMs = (long)StopwatchContext.Timed(
					() =>
						{
							for (int i = 0; i < numberOfInstances; i++)
							{
								systemUnderTest.Create(foo1);
							}
						}).TotalMilliseconds;
				// ReSharper disable ImplicitlyCapturedClosure
				var hardCodedMs = (long)StopwatchContext.Timed(
					() =>
					// ReSharper restore ImplicitlyCapturedClosure
						{
							for (int i = 0; i < numberOfInstances; i++)
							{
								// ReSharper disable ObjectCreationAsStatement
								new HardCodedFooAdapter(foo1);
								// ReSharper restore ObjectCreationAsStatement
							}
						}).TotalMilliseconds;
				double mappingServicePerInstanceSeconds = (mappingServiceMs / 1000.0) / numberOfInstances;
				double hardCodedPerInstanceSeconds = (hardCodedMs / 1000.0) / numberOfInstances;
				double performanceDifference = mappingServiceMs / (double)hardCodedMs;

				Console.WriteLine("Generated Adapter:  {0:0.0000000000000}s per instance, {1:0.000}s total, {2} instances.", mappingServicePerInstanceSeconds, mappingServiceMs / 1000.0, numberOfInstances);
				Console.WriteLine("Hard-coded Adapter: {0:0.0000000000000}s per instance, {1:0.000}s total, {2} instances.", hardCodedPerInstanceSeconds, hardCodedMs / 1000.0, numberOfInstances);
				Console.WriteLine();
				Console.WriteLine("Relative time for generated version: {0:00.00}x slower", performanceDifference);
				Console.WriteLine("Cost per 100 instances as percentage of 50ms page load: {0:000.000000}%", ((mappingServicePerInstanceSeconds * 100) / 0.050) * 100.0);

				Assert.That(performanceDifference, Is.LessThan(30.0));
			}

			[Test]
			public void Must_reuse_the_same_assembly_for_all_generated_adapter_factories()
			{
				var fooConfiguration = new MappingConfiguration<Foo, IFooView>();

				fooConfiguration.Map(view => view.Name).From(foo => foo.Name);
				fooConfiguration.Map(view => view.Id).From(foo => foo.Id);
				fooConfiguration.Map(view => view.Factor).From(foo => foo.Factor);

				IAdapterFactory<Foo, IFooView> fooFactory = AdapterFactoryGenerator.Instance.Generate<Foo, IFooView>(fooConfiguration.Mappings);
				var foo1 = new Foo("test", Guid.NewGuid(), 1);
				IFooView fooView = fooFactory.Create(foo1);
				var barConfiguration = new MappingConfiguration<Bar, IFooView>();

				barConfiguration.Map(view => view.Name).From(bar => bar.Name);
				barConfiguration.Map(view => view.Id).From(bar => bar.Id);
				barConfiguration.Map(view => view.Factor).From(bar => null);

				IAdapterFactory<Bar, IFooView> barFactory = AdapterFactoryGenerator.Instance.Generate<Bar, IFooView>(barConfiguration.Mappings);
				var bar1 = new Bar("test", Guid.NewGuid());
				IFooView barView = barFactory.Create(bar1);
				Type fooViewType = fooView.GetType();
				Type barViewType = barView.GetType();

				Assert.That(fooViewType.Assembly, Is.SameAs(barViewType.Assembly));
			}

			[Test]
			public void Must_throw_exception_if_not_all_desired_type_properties_are_mapped()
			{
				var fooConfiguration = new MappingConfiguration<Foo, IFooView>();

				fooConfiguration.Map(view => view.Name).From(foo => foo.Name);

				Assert.Throws<Exception>(() => AdapterFactoryGenerator.Instance.Generate<Foo, IFooView>(fooConfiguration.Mappings));
			}

			[Test]
			public void Must_wire_up_mapping_delegate_for_each_property()
			{
				var fooConfiguration = new MappingConfiguration<Foo, IFooView>();
				bool nameDelegateCalled = false;
				bool idDelegateCalled = false;
				bool factorDelegateCalled = false;

				fooConfiguration.Map(view => view.Name).From(foo =>
					{
						nameDelegateCalled = true;
						return foo.Name;
					});
				fooConfiguration.Map(view => view.Id).From(foo =>
					{
						idDelegateCalled = true;
						return foo.Id;
					});
				fooConfiguration.Map(view => view.Factor).From(foo =>
					{
						factorDelegateCalled = true;
						return foo.Factor;
					});

				var foo1 = new Foo("test", Guid.NewGuid(), 1);
				IAdapterFactory<Foo, IFooView> systemUnderTest = AdapterFactoryGenerator.Instance.Generate<Foo, IFooView>(fooConfiguration.Mappings);
				IFooView fooView = systemUnderTest.Create(foo1);
				string name = fooView.Name;
				Guid id = fooView.Id;
				int factor = fooView.Factor.Factor;

				Assert.That(name, Is.EqualTo(foo1.Name));
				Assert.That(id, Is.EqualTo(foo1.Id));
				Assert.That(factor, Is.EqualTo(foo1.Factor.Factor));
				Assert.That(nameDelegateCalled, Is.True);
				Assert.That(idDelegateCalled, Is.True);
				Assert.That(factorDelegateCalled, Is.True);
			}
		}
	}
}