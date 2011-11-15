using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Junior.Map.Common
{
	/// <summary>
	/// Represents a way to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public interface IMappingConfiguration<TSource, TTarget>
	{
		/// <summary>
		/// Gets configured mappings.
		/// </summary>
		IEnumerable<MemberMapping<TSource>> Mappings
		{
			get;
		}

		/// <summary>
		/// Creates a mapping for the member of <typeparamref name="TTarget"/> referenced by the specified LINQ expression.
		/// </summary>
		/// <typeparam name="TMember">The type of the target member.</typeparam>
		/// <param name="expression">A LINQ expression whose body is a <see cref="MemberExpression"/>.</param>
		/// <returns>A configurable mapping.</returns>
		IMapping<TSource, TMember> Map<TMember>(Expression<Func<TTarget, TMember>> expression);

		/// <summary>
		/// Creates a mapping for a member of <typeparamref name="TTarget"/> with the specified name.
		/// </summary>
		/// <param name="memberName">The name of a member of <typeparamref name="TTarget"/>.</param>
		/// <returns>A configurable mapping.</returns>
		IMapping<TSource, object> Map(string memberName);
	}
}