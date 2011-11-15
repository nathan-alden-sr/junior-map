using System.Collections.Generic;

using Junior.Map.Common;
using Junior.Map.Mapper.Conventions;

namespace Junior.Map.Mapper
{
	/// <summary>
	/// Maps bi-directionally between two types and allows creation of custom mappings at runtime.
	/// </summary>
	public class DefaultBidirectionalMapper<T1, T2> : ConventionBasedBidirectionalMapper<T1, T2>
	{
		/// <summary>
		/// Retrieves mapper conventions to use when configuring mappings.
		/// </summary>
		/// <returns>The mapper conventions to use when configuring mappings.</returns>
		protected override IEnumerable<IMapperConvention> GetConventions()
		{
			yield return new NameAndTypeMatchMappingConvention();
			foreach (IMapperConvention convention in DefaultMappingConventionsProvider.DefaultConventions.AsMapperConventions())
			{
				yield return convention;
			}
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified <see cref="MapperConfiguration{T1,T2}"/> instance.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected override void ConfigureCustomMapping(MapperConfiguration<T1, T2> configuration)
		{
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified <see cref="MapperConfiguration{T1,T2}"/> instance.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected override void ConfigureCustomMapping(MapperConfiguration<T2, T1> configuration)
		{
		}
	}
}