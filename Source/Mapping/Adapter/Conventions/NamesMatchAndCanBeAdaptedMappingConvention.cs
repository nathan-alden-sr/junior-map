using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Junior.Common;
using Junior.Map.Common;

namespace Junior.Map.Adapter.Conventions
{
	/// <summary>
	/// Maps properties with the same name if the types can be adapted by another adapter factory.
	/// </summary>
	public class NamesMatchAndCanBeAdaptedMappingConvention : IMappingConvention
	{
		private readonly AdapterFactoryLocator _locator;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamesMatchAndCanBeAdaptedMappingConvention"/> class.
		/// </summary>
		/// <param name="locator">An adapter factory locator to use when locating adapter factories.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="locator"/> is null.</exception>
		public NamesMatchAndCanBeAdaptedMappingConvention(AdapterFactoryLocator locator)
		{
			locator.ThrowIfNull("locator");

			_locator = locator;
		}

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
				PropertyInfo sourcePropertyInfo = sourceProperties.FindMatchingProperty(eligibleProperty.Name);

				if (sourcePropertyInfo == null || eligibleProperty.PropertyType.IsAssignableFrom(sourcePropertyInfo.PropertyType) || eligibleProperty.PropertyType.IsValueType)
				{
					continue;
				}

				object adapterFactory = _locator.Locate(sourcePropertyInfo.PropertyType, eligibleProperty.PropertyType);
				var mappingProvider = adapterFactory as IMappingProvider;

				if (mappingProvider != null)
				{
					try
					{
						mappingProvider.Validate();
					}
					catch
					{
						continue;
					}
				}

				MethodInfo factoryCreateMethod = adapterFactory.GetType().GetMethod("Create");
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "sourceInstance");
				UnaryExpression methodCallExpression =
					Expression.Convert(
						Expression.Call(
							Expression.Constant(adapterFactory),
							factoryCreateMethod,
							Expression.Convert(parameterExpression, sourcePropertyInfo.PropertyType)),
						typeof(object));

				Func<object, object> createAdapterDelegate = Expression.Lambda<Func<object, object>>(methodCallExpression, parameterExpression).Compile();
				// ReSharper disable ConvertClosureToMethodGroup
				// Converting the lambda to a method group causes a runtime error
				Func<TSource, object> sourceGetterDelegate = MappingConventionHelper.Instance.GetNullSafePropertyEvaluation<TSource>(sourcePropertyInfo, source => createAdapterDelegate(source));
				// ReSharper restore ConvertClosureToMethodGroup

				configuration.Map(eligibleProperty.Name).From(sourceGetterDelegate);
			}
		}
	}
}