using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Junior.Common;

namespace Junior.Map.Common.Conventions
{
	/// <summary>
	/// Maps properties with the same name and with types that are convertible between each other.
	/// </summary>
	public class OnlyNameMatchesButTypeIsConvertibleMappingConvention : IMappingConvention
	{
		/// <summary>
		/// Applies mappings to the specified mapping configuration.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <param name="propertyFinder">An <see cref="IConventionEligiblePropertyFinder"/> with which to find eligible properties.</param>
		/// <param name="configuration">A mapping configuration to which mappings will be applied.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyFinder"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		public void Apply<TSource, TTarget>(IConventionEligiblePropertyFinder propertyFinder, IMappingConfiguration<TSource, TTarget> configuration)
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

				if (sourcePropertyInfo == null)
				{
					continue;
				}

				TypeConverter targetConverter = TypeDescriptor.GetConverter(tempEligibleProperty.PropertyType);
				TypeConverter sourceConverter = TypeDescriptor.GetConverter(sourcePropertyInfo.PropertyType);

				// ReSharper disable ConditionIsAlwaysTrueOrFalse
				if (targetConverter != null &&
				    // ReSharper restore ConditionIsAlwaysTrueOrFalse
				    !tempEligibleProperty.PropertyType.IsAssignableFrom(sourcePropertyInfo.PropertyType) &&
				    targetConverter.CanConvertFrom(sourcePropertyInfo.PropertyType))
				{
					Func<TSource, object> @delegate = MappingConventionHelper.Instance.GetPropertyGetter<TSource>(sourcePropertyInfo).Compile();

					configuration.Map(tempEligibleProperty.Name).From(source => targetConverter.ConvertFrom(@delegate(source)));
				}
					// ReSharper disable ConditionIsAlwaysTrueOrFalse
				else if (sourceConverter != null &&
				         // ReSharper restore ConditionIsAlwaysTrueOrFalse
				         !tempEligibleProperty.PropertyType.IsAssignableFrom(sourcePropertyInfo.PropertyType) &&
				         sourceConverter.CanConvertTo(tempEligibleProperty.PropertyType))
				{
					Func<TSource, object> @delegate = MappingConventionHelper.Instance.GetPropertyGetter<TSource>(sourcePropertyInfo).Compile();

					configuration.Map(tempEligibleProperty.Name).From(source => sourceConverter.ConvertTo(@delegate(source), tempEligibleProperty.PropertyType));
				}
			}
		}
	}
}