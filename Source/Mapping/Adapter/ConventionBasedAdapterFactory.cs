using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Common;
using Junior.Map.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Creates an adapter that adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/> and creates custom mappings at runtime.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The source type.</typeparam>
	public abstract class ConventionBasedAdapterFactory<TSource, TTarget> : AdapterFactory<TSource, TTarget>
		where TSource : class
		where TTarget : class
	{
		private readonly AdapterConventionEligiblePropertyFinder _propertyFinder = new AdapterConventionEligiblePropertyFinder();
		private IEnumerable<IMappingConvention> _conventions = Enumerable.Empty<IMappingConvention>();

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified adapter factory configuration.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		protected override void ConfigureAdapter(AdapterFactoryConfiguration<TSource, TTarget> configuration)
		{
			configuration.ThrowIfNull("configuration");

			_conventions = GetConventions();
			ApplyConventions(configuration);
			ConfigureCustomMapping(configuration);
		}

		/// <summary>
		/// Retrieves mapping conventions to use when configuring mappings.
		/// </summary>
		/// <returns>The mapping conventions to use when configuring mappings.</returns>
		protected abstract IEnumerable<IMappingConvention> GetConventions();

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified adapter factory configuration.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		protected abstract void ConfigureCustomMapping(AdapterFactoryConfiguration<TSource, TTarget> configuration);

		private void ApplyConventions(IMappingConfiguration<TSource, TTarget> configuration)
		{
			foreach (IMappingConvention convention in _conventions)
			{
				convention.Apply(_propertyFinder, configuration);
			}
		}
	}
}