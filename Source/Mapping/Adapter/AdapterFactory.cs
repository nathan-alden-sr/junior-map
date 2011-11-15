using System;

using Junior.Common;
using Junior.Map.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Creates an adapter that adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/> and creates custom mappings at runtime.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The source type.</typeparam>
	public abstract class AdapterFactory<TSource, TTarget> : IAdapterFactory<TSource, TTarget>, IAdapterFactory, IMappingProvider
		where TSource : class
		where TTarget : class
	{
		private readonly Lazy<IAdapterFactory<TSource, TTarget>> _adapterFactory;
		private readonly AdapterFactoryConfiguration<TSource, TTarget> _configuration;
		private bool _isMappingConfigured;

		/// <summary>
		/// Initializes a new instance of the <see cref="AdapterFactory{TSource,TTarget}"/> class.
		/// </summary>
		protected AdapterFactory()
			: this(new AdapterFactoryConfiguration<TSource, TTarget>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AdapterFactory{TSource,TTarget}"/> class.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		protected AdapterFactory(AdapterFactoryConfiguration<TSource, TTarget> configuration)
		{
			configuration.ThrowIfNull("configuration");

			_configuration = configuration;
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
		/// Allows configuration of custom mappings at runtime through the specified adapter factory configuration.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		protected abstract void ConfigureAdapter(AdapterFactoryConfiguration<TSource, TTarget> configuration);

		private void ConfigureMapping()
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