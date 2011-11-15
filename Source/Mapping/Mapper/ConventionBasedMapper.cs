using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Common;

namespace Junior.Map.Mapper
{
	/// <summary>
	/// Maps a source object to a target object and allows creation of custom mappings at runtime.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public abstract class ConventionBasedMapper<TSource, TTarget> : Mapper<TSource, TTarget>
	{
		private readonly MapperConventionEligiblePropertyFinder _propertyFinder = new MapperConventionEligiblePropertyFinder();
		private IEnumerable<IMapperConvention> _conventions = Enumerable.Empty<IMapperConvention>();

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		protected override void ConfigureMapper(MapperConfiguration<TSource, TTarget> configuration)
		{
			configuration.ThrowIfNull("configuration");

			_conventions = GetConventions();
			ApplyConventions(configuration);
			ConfigureCustomMapping(configuration);
		}

		/// <summary>
		/// Retrieves mapper conventions to use when configuring mappings.
		/// </summary>
		/// <returns>The mapper conventions to use when configuring mappings.</returns>
		protected abstract IEnumerable<IMapperConvention> GetConventions();

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected abstract void ConfigureCustomMapping(MapperConfiguration<TSource, TTarget> configuration);

		private void ApplyConventions(MapperConfiguration<TSource, TTarget> configuration)
		{
			foreach (IMapperConvention convention in _conventions)
			{
				convention.Apply(_propertyFinder, configuration);
			}
		}
	}
}