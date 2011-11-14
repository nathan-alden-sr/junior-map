using System.Collections.Generic;
using System.Linq;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Maps bi-directionally between two types and allows creation of custom mappings at runtime.
	/// </summary>
	public abstract class ConventionBasedBidirectionalMapper<T1, T2> : BidirectionalMapper<T1, T2>
	{
		private readonly MapperConventionEligiblePropertyFinder _propertyFinder = new MapperConventionEligiblePropertyFinder();
		private IEnumerable<IMapperConvention> _conventions = Enumerable.Empty<IMapperConvention>();

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected override void ConfigureMapper(MapperConfiguration<T1, T2> configuration)
		{
			_conventions = GetConventions();
			ApplyConventions(configuration);
			ConfigureCustomMapping(configuration);
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected override void ConfigureMapper(MapperConfiguration<T2, T1> configuration)
		{
			_conventions = GetConventions();
			ApplyReverseConventions(configuration);
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
		protected abstract void ConfigureCustomMapping(MapperConfiguration<T1, T2> configuration);

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected abstract void ConfigureCustomMapping(MapperConfiguration<T2, T1> configuration);

		private void ApplyConventions(MapperConfiguration<T1, T2> configuration)
		{
			foreach (IMapperConvention convention in _conventions)
			{
				convention.Apply(_propertyFinder, configuration);
			}
		}

		private void ApplyReverseConventions(MapperConfiguration<T2, T1> configuration)
		{
			foreach (IMapperConvention convention in _conventions)
			{
				convention.Apply(_propertyFinder, configuration);
			}
		}
	}
}