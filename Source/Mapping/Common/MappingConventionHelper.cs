using System;
using System.Linq.Expressions;
using System.Reflection;

using Junior.Common;

namespace Junior.Mapping.Common
{
	/// <summary>
	/// Provides helper methods to map convention classes.
	/// </summary>
	public class MappingConventionHelper
	{
		/// <summary>
		/// A singleton instance of <see cref="MappingConventionHelper"/>.
		/// </summary>
		public static readonly MappingConventionHelper Instance = new MappingConventionHelper();

		private MappingConventionHelper()
		{
		}

		/// <summary>
		/// Maps a property that appears to represent the same value in <typeparamref name="TSource"/> as <typeparamref name="TTarget"/>.
		/// </summary>
		/// <param name="configuration">A mapping configuration.</param>
		/// <param name="sourcePropertyInfo">A property of <typeparamref name="TSource"/>.</param>
		/// <param name="targetPropertyInfo">A property of <typeparamref name="TTarget"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sourcePropertyInfo"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="targetPropertyInfo"/> is null.</exception>
		public void MapLikeProperties<TSource, TTarget>(IMappingConfiguration<TSource, TTarget> configuration, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
		{
			configuration.ThrowIfNull("configuration");
			sourcePropertyInfo.ThrowIfNull("sourcePropertyInfo");
			targetPropertyInfo.ThrowIfNull("targetPropertyInfo");

			Func<TSource, object> valueDelegate = GetPropertyGetter<TSource>(sourcePropertyInfo).Compile();

			configuration.Map(targetPropertyInfo.Name).From(valueDelegate);
		}

		/// <summary>
		/// Retrieves a LINQ expression representing the specified property's getter.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TProperty">The type of the property's value.</typeparam>
		/// <param name="sourcePropertyInfo">A property of <typeparamref name="TSource"/>.</param>
		/// <returns>A LINQ expression representing the getter of <paramref name="sourcePropertyInfo"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sourcePropertyInfo"/> is null.</exception>
		public Expression<Func<TSource, TProperty>> GetPropertyGetter<TSource, TProperty>(PropertyInfo sourcePropertyInfo)
		{
			sourcePropertyInfo.ThrowIfNull("sourcePropertyInfo");

			ParameterExpression parameter = Expression.Parameter(typeof(TSource), "sourceInstance");
			MemberExpression memberExpression = Expression.Property(parameter, sourcePropertyInfo);

			return Expression.Lambda<Func<TSource, TProperty>>(
				Expression.Convert(memberExpression, typeof(TProperty)),
				parameter);
		}

		/// <summary>
		/// Retrieves a LINQ expression representing the specified property's getter.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <param name="sourcePropertyInfo">A property of <typeparamref name="TSource"/>.</param>
		/// <returns>A LINQ expression representing the getter of <paramref name="sourcePropertyInfo"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sourcePropertyInfo"/> is null.</exception>
		public Expression<Func<TSource, object>> GetPropertyGetter<TSource>(PropertyInfo sourcePropertyInfo)
		{
			sourcePropertyInfo.ThrowIfNull("sourcePropertyInfo");

			return GetPropertyGetter<TSource, object>(sourcePropertyInfo);
		}

		/// <summary>
		/// Gets a null-safe LINQ expression representing null-safe evaluation of a property value.
		/// If the source property has a null value, the expression will return null; otherwise, the result of a delegate will be returned.
		/// </summary>
		/// <param name="sourcePropertyInfo">A <see cref="PropertyInfo"/> instance representing a property of <typeparamref name="TSource"/>.</param>
		/// <param name="notNullSafeDelegate">A <see cref="Func{T,TResult}"/> to execute when the property value is not null.</param>
		/// <returns>null if the source property has a null value; otherwise, the result of <paramref name="notNullSafeDelegate"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sourcePropertyInfo"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when the declaring type of <paramref name="sourcePropertyInfo"/> is not <typeparamref name="TSource"/>.</exception>
		public Func<TSource, object> GetNullSafePropertyEvaluation<TSource>(PropertyInfo sourcePropertyInfo, Func<object, object> notNullSafeDelegate)
		{
			sourcePropertyInfo.ThrowIfNull("sourcePropertyInfo");

			Type sourceType = typeof(TSource);

			if (sourcePropertyInfo.DeclaringType != sourceType)
			{
				throw new ArgumentException("The property's declaring type must be the source type.", "sourcePropertyInfo");
			}

			// Generates the following delegate:
			// return sourceInstance.Property == null ? null : notNullSafeDelegate(sourceInstance.Property);

			ParameterExpression sourceInstance = Expression.Parameter(sourceType, "sourceInstance");
			Expression nullSafePropertyEvaluation =
				Expression.Condition(
					Expression.Equal(
						Expression.Property(sourceInstance, sourcePropertyInfo),
						Expression.Constant(null)),
					Expression.Constant(null),
					Expression.Call(
						Expression.Constant(notNullSafeDelegate.Target),
						notNullSafeDelegate.Method,
						Expression.Property(sourceInstance, sourcePropertyInfo)));

			return Expression.Lambda<Func<TSource, object>>(nullSafePropertyEvaluation, sourceInstance).Compile();
		}
	}
}