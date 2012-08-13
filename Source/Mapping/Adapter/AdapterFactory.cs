using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Common;
using Junior.Map.Adapter.Conventions;
using Junior.Map.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Creates an adapter that recursively adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/> and creates custom mappings at runtime.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public abstract class AdapterFactory<TSource, TTarget> : IAdapterFactory<TSource, TTarget>, IAdapterFactory, IMappingProvider
		where TSource : class
		where TTarget : class
	{
		private static object _lockObject = new object();
		private readonly Lazy<IAdapterFactory<TSource, TTarget>> _adapterFactory;
		private readonly AdapterFactoryFlags _adapterFactoryFlags;
		private readonly AdapterFactoryConfiguration<TSource, TTarget> _configuration;
		private readonly AdapterFactoryLocator _locator;
		private readonly AdapterConventionEligiblePropertyFinder _propertyFinder = new AdapterConventionEligiblePropertyFinder();
		private IEnumerable<IMappingConvention> _conventions = Enumerable.Empty<IMappingConvention>();
		private bool _isMappingConfigured;

		/// <summary>
		/// Initializes a new instance of the <see cref="AdapterFactory{TSource,TTarget}"/> class with default behaviors.
		/// </summary>
		protected AdapterFactory()
			: this(null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AdapterFactory{TSource,TTarget}"/> class with default options.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		/// <param name="adapterFactoryLocator">An adapter factory locator.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		protected AdapterFactory(AdapterFactoryConfiguration<TSource, TTarget> configuration = null, IAdapterFactoryLocator adapterFactoryLocator = null)
			: this(AdapterFactoryFlags.ApplyDefaultMappingConventions | AdapterFactoryFlags.ApplyRecursiveMappingConvention, configuration, adapterFactoryLocator)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AdapterFactory{TSource,TTarget}"/> class.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		/// <param name="adapterFactoryLocator">An adapter factory locator.</param>
		/// <param name="adapterFactoryFlags">Adapter factory flags.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		protected AdapterFactory(AdapterFactoryFlags adapterFactoryFlags, AdapterFactoryConfiguration<TSource, TTarget> configuration = null, IAdapterFactoryLocator adapterFactoryLocator = null)
		{
			configuration = configuration ?? new AdapterFactoryConfiguration<TSource, TTarget>();
			_locator = adapterFactoryLocator == null ? new AdapterFactoryLocator() : new AdapterFactoryLocator(adapterFactoryLocator);
			configuration.ThrowIfNull("configuration");

			_configuration = configuration;
			_adapterFactoryFlags = adapterFactoryFlags;
			_adapterFactory = new Lazy<IAdapterFactory<TSource, TTarget>>(
				() =>
					{
						ConfigureMapping();
						return AdapterFactoryGenerator.Instance.Generate<TSource, TTarget>(_configuration.Mappings);
					});
		}

		/// <summary>
		/// Use generic overload instead.
		/// </summary>
		object IAdapterFactory.Create(object source)
		{
			source.ThrowIfNull("source");

			return _adapterFactory.Value.Create((TSource)source);
		}

		/// <summary>
		/// Creates an adapter that adapts <paramref name="source"/> to an instance of <typeparamref name="TTarget"/>.
		/// </summary>
		/// <param name="source">An instance of <typeparamref name="TSource"/>.</param>
		/// <returns>an instance of <typeparamref name="TTarget"/> that is created at runtime.</returns>
		/// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
		public virtual TTarget Create(TSource source)
		{
			source.ThrowIfNull("source");

			return _adapterFactory.Value.Create(source);
		}

		/// <summary>
		/// Ensures that custom mappings are valid.
		/// </summary>
		public virtual void Validate()
		{
			ConfigureMapping();
			AdapterFactoryGenerator.Instance.ValidateMapping<TSource, TTarget>(_configuration.Mappings);
		}

		/// <summary>
		/// Registers an adapter factory type that should be used when <typeparamref name="TTarget"/> has a <typeparamref name="TDependencyTarget"/> that should be adapted from a <typeparamref name="TDependencySource"/>.
		/// </summary>
		/// <param name="adapterFactoryCreateFunc">A func that will create an instance of the registered adapter factory.</param>
		/// <typeparam name="TDependencySource">The source type.</typeparam>
		/// <typeparam name="TDependencyTarget">The target type.</typeparam>
		public void RegisterAdapterFactory<TDependencySource, TDependencyTarget>(Func<IAdapterFactory<TDependencySource, TDependencyTarget>> adapterFactoryCreateFunc)
			where TDependencyTarget : class
			where TDependencySource : class
		{
			_locator.RegisterAdapterFactory(adapterFactoryCreateFunc);
		}

		/// <summary>
		/// Registers an adapter factory type that should be used when <typeparamref name="TTarget"/> has a <typeparamref name="TDependencyTarget"/> that should be adapted from a <typeparamref name="TDependencySource"/>.
		/// </summary>
		/// <typeparam name="TDependencySource">The source type.</typeparam>
		/// <typeparam name="TDependencyTarget">The target type.</typeparam>
		/// <typeparam name="TAdapterType">The adapter factory type to use.  It must have a default constructor.</typeparam>
		public void RegisterAdapterFactory<TDependencySource, TDependencyTarget, TAdapterType>()
			where TAdapterType : class, IAdapterFactory<TDependencySource, TDependencyTarget>, new()
			where TDependencyTarget : class
			where TDependencySource : class
		{
			_locator.RegisterAdapterFactory<TDependencySource, TDependencyTarget, TAdapterType>();
		}

		/// <summary>
		/// Retrieves mapping conventions to use when configuring mappings.
		/// </summary>
		/// <returns>The mapping conventions to use when configuring mappings.</returns>
		protected virtual IEnumerable<IMappingConvention> GetConventions()
		{
			yield return new NameAndTypeMatchAdapterMappingConvention();

			if ((_adapterFactoryFlags & AdapterFactoryFlags.ApplyDefaultMappingConventions) == AdapterFactoryFlags.ApplyDefaultMappingConventions)
			{
				foreach (IMappingConvention convention in DefaultMappingConventionsProvider.DefaultConventions)
				{
					yield return convention;
				}
			}

			if ((_adapterFactoryFlags & AdapterFactoryFlags.ApplyRecursiveMappingConvention) == AdapterFactoryFlags.ApplyRecursiveMappingConvention)
			{
				yield return new NamesMatchAndCanBeAdaptedMappingConvention(_locator);
			}
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified adapter factory configuration.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		private void ConfigureAdapter(AdapterFactoryConfiguration<TSource, TTarget> configuration)
		{
			configuration.ThrowIfNull("configuration");

			_conventions = GetConventions();
			ApplyConventions(configuration);
			ConfigureCustomMapping(configuration);
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified adapter factory configuration.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		protected virtual void ConfigureCustomMapping(AdapterFactoryConfiguration<TSource, TTarget> configuration)
		{
		}

		private void ApplyConventions(IMappingConfiguration<TSource, TTarget> configuration)
		{
			foreach (IMappingConvention convention in _conventions)
			{
				convention.Apply(_propertyFinder, configuration);
			}
		}

		private void ConfigureMapping()
		{
			if (_isMappingConfigured)
			{
				return;
			}

			lock (_lockObject)
			{
				if (_isMappingConfigured)
				{
					return;
				}

				ConfigureAdapter(_configuration);
				_isMappingConfigured = true;
			}
		}
	}
}