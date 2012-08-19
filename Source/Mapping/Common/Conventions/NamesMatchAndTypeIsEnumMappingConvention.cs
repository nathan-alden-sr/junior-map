using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Junior.Common;

namespace Junior.Map.Common.Conventions
{
	/// <summary>
	/// Maps properties with the same name and type that are enumerations.
	/// </summary>
	public class NamesMatchAndTypeIsEnumMappingConvention : IMappingConvention
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

				if (configuration.Mappings.Any(arg => arg.MemberName == tempEligibleProperty.Name))
				{
					continue;
				}

				Type nullableTargetUnderlyingType = Nullable.GetUnderlyingType(tempEligibleProperty.PropertyType);

				if (nullableTargetUnderlyingType == null && !tempEligibleProperty.PropertyType.IsEnum)
				{
					continue;
				}
				if (nullableTargetUnderlyingType != null && !nullableTargetUnderlyingType.IsEnum)
				{
					continue;
				}

				PropertyInfo sourcePropertyInfo = sourceProperties.FindMatchingProperty(tempEligibleProperty.Name);
				Type nullableSourceUnderlyingType = sourcePropertyInfo != null ? Nullable.GetUnderlyingType(sourcePropertyInfo.PropertyType) : null;

				if (sourcePropertyInfo == null ||
				    (!sourcePropertyInfo.PropertyType.IsEnum && (nullableSourceUnderlyingType == null || !nullableSourceUnderlyingType.IsEnum)))
				{
					continue;
				}

				Type type = typeof(DefaultEnumerationMapper<,>);
				Type defaultEnumerationMapperType = type.MakeGenericType(sourcePropertyInfo.PropertyType, tempEligibleProperty.PropertyType);

				try
				{
					object defaultEnumerationMapper = Activator.CreateInstance(defaultEnumerationMapperType, null);
					var mappingProvider = defaultEnumerationMapper as IMappingProvider;

					if (mappingProvider != null)
					{
						mappingProvider.Validate();
					}

					MethodInfo getMappedValueMethod = defaultEnumerationMapper.GetType().GetMethod("GetMappedValue");
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "sourceValue");
					UnaryExpression methodCallExpression = Expression.Convert(Expression.Call(
						Expression.Constant(defaultEnumerationMapper),
						getMappedValueMethod,
						Expression.Convert(parameterExpression, sourcePropertyInfo.PropertyType)), typeof(object));
					Func<object, object> getMappedValueFunc = Expression.Lambda<Func<object, object>>(methodCallExpression, parameterExpression).Compile();
					Func<TSource, object> getterForSourceType = MappingConventionHelper.Instance.GetPropertyGetter<TSource>(sourcePropertyInfo).Compile();

					configuration.Map(tempEligibleProperty.Name).From(source => getMappedValueFunc(getterForSourceType(source)));
				}
				catch
				{
					// Eat the exception because the determination of whether the enumeration is mappable is delegated to EnumerationMapper.
					// ReSharper disable RedundantJumpStatement
					continue;
					// ReSharper restore RedundantJumpStatement
				}
			}
		}

		private class DefaultEnumerationMapper<TSource, TTarget> : EnumerationMapper<TSource, TTarget>
		{
		}
	}
}