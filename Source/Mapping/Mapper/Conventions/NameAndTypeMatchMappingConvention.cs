using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Junior.Common;
using Junior.Map.Common;

namespace Junior.Map.Mapper.Conventions
{
	/// <summary>
	/// Maps properties with the same name and type.
	/// </summary>
	public class NameAndTypeMatchMappingConvention : IMapperConvention
	{
		/// <summary>
		/// Applies mappings to the specified mapping configuration.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <param name="propertyFinder">An <see cref="IConventionEligiblePropertyFinder"/> with which to find eligible properties.</param>
		/// <param name="configuration">A mapper configuration to which mappings will be applied.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyFinder"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		public void Apply<TSource, TTarget>(IConventionEligiblePropertyFinder propertyFinder, MapperConfiguration<TSource, TTarget> configuration)
		{
			propertyFinder.ThrowIfNull("propertyFinder");
			configuration.ThrowIfNull("configuration");

			IEnumerable<PropertyInfo> sourceProperties = typeof(TSource)
				.GetAllPublicInstanceProperties()
				.ToArray();
			IEnumerable<PropertyInfo> eligibleProperties = propertyFinder.GetEligibleProperties<TTarget>();

			foreach (PropertyInfo eligibleProperty in eligibleProperties)
			{
				PropertyInfo tempEligibleProperty = eligibleProperty;
				PropertyInfo sourcePropertyInfo = sourceProperties.FindMatchingProperty(tempEligibleProperty.Name);

				if (sourcePropertyInfo != null && tempEligibleProperty.GetSetMethod() != null && tempEligibleProperty.PropertyType.IsAssignableFrom(sourcePropertyInfo.PropertyType))
				{
					MappingConventionHelper.Instance.MapLikeProperties(configuration, sourcePropertyInfo, tempEligibleProperty);
				}
			}
		}
	}
}