using System.Collections.Generic;

using Junior.Mapping.Common;
using Junior.Mapping.Mapper.Conventions;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Maps a source object to a target object using common conventions, but does not allow creation of custom mappings at runtime.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public class DefaultMapper<TSource, TTarget> : ConventionBasedMapper<TSource, TTarget>
	{
		/// <summary>
		/// Retrieves mapper conventions to use when configuring mappings.
		/// </summary>
		/// <returns>The mapper conventions to use when configuring mappings.</returns>
		protected override IEnumerable<IMapperConvention> GetConventions()
		{
			foreach (IMapperConvention convention in DefaultMappingConventionsProvider.DefaultConventions.AsMapperConventions())
			{
				yield return convention;
			}
			yield return new NameAndTypeMatchMappingConvention();
			yield return new NamesMatchAndTypeIsEnumMappingConvention();
		}

		/// <summary>
		/// Allows configuration of custom maps at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected override void ConfigureCustomMapping(MapperConfiguration<TSource, TTarget> configuration)
		{
		}
	}
}