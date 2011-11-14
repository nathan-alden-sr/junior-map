using Junior.Mapping.Common;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Adapts an instance of <see cref="IMappingConvention"/> to <see cref="IMapperConvention"/> by encapsulation.
	/// </summary>
	public class MappingConventionAdapter : IMapperConvention
	{
		private readonly IMappingConvention _mappingConvention;

		/// <summary>
		/// Initializes a new instance of the <see cref="MappingConventionAdapter"/> class.
		/// </summary>
		/// <param name="mappingConvention">The mapping convention to encapsulate.</param>
		public MappingConventionAdapter(IMappingConvention mappingConvention)
		{
			_mappingConvention = mappingConvention;
		}

		/// <summary>
		/// Applies mappings to the specified mapping configuration.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <param name="propertyFinder">An <see cref="IConventionEligiblePropertyFinder"/> with which to find eligible properties.</param>
		/// <param name="configuration">A mapper configuration to which mappings will be applied.</param>
		public void Apply<TSource, TTarget>(IConventionEligiblePropertyFinder propertyFinder, MapperConfiguration<TSource, TTarget> configuration)
		{
			_mappingConvention.Apply(propertyFinder, configuration);
		}
	}
}