using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Junior.Common;
using Junior.Map.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Stores mapping delegates used to map members of <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public class AdapterFactoryConfiguration<TSource, TTarget> : IMappingConfiguration<TSource, TTarget>
	{
		private readonly MappingConfigurationHelper<TSource, TTarget> _mappingConfigurationHelper = new MappingConfigurationHelper<TSource, TTarget>();

		/// <summary>
		/// Gets configured mappings.
		/// </summary>
		public IEnumerable<MemberMapping<TSource>> Mappings
		{
			get
			{
				return _mappingConfigurationHelper.Mappings;
			}
		}

		/// <summary>
		/// Creates a mapping for the member of <typeparamref name="TTarget"/> referenced by the specified LINQ expression.
		/// </summary>
		/// <typeparam name="TMember">The type of the target member.</typeparam>
		/// <param name="expression">A LINQ expression whose body is a <see cref="MemberExpression"/>.</param>
		/// <returns>A configurable mapping.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
		public IMapping<TSource, TMember> Map<TMember>(Expression<Func<TTarget, TMember>> expression)
		{
			expression.ThrowIfNull("expression");

			return _mappingConfigurationHelper.Map(expression);
		}

		/// <summary>
		/// Creates a mapping for a member of <typeparamref name="TTarget"/> with the specified name.
		/// </summary>
		/// <param name="memberName">The name of a member of <typeparamref name="TTarget"/>.</param>
		/// <returns>A configurable mapping.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="memberName"/> is null.</exception>
		public IMapping<TSource, object> Map(string memberName)
		{
			memberName.ThrowIfNull("memberName");

			return _mappingConfigurationHelper.Map(memberName);
		}

		/// <summary>
		/// Ensures a <typeparamref name="TSource"/> instance can be mapped to a <typeparamref name="TTarget"/> instance.
		/// </summary>
		public void Validate()
		{
			_mappingConfigurationHelper.Validate();
		}
	}
}