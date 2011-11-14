using System;
using System.Linq.Expressions;

using Junior.Mapping.Common;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Represents ways to configure mappings for a property of <typeparamref name="TSource"/> when using a mapper.
	/// </summary>
	/// <typeparamref name="TSource">The source type.</typeparamref>
	/// <typeparamref name="TTarget">The target type.</typeparamref>
	/// <typeparamref name="TMember">The member type to map.</typeparamref>
	public interface IMapperMapping<out TSource, TTarget, TMember> : IMapping<TSource, TMember>
	{
		/// <summary>
		/// Provides a delegate that maps the member.
		/// </summary>
		/// <param name="mapDelegate">An <see cref="Action{TTarget,TSource}"/> that maps the member.</param>
		void ByInvoking(Action<TTarget, TSource> mapDelegate);

		/// <summary>
		/// Ignores the member when mapping.
		/// </summary>
		void ByIgnoring();

		/// <summary>
		/// Maps the member by constructing a new instance of <typeparamref name="TMapperTarget"/> and delegating the actual mapping to the provided mapper.
		/// </summary>
		/// <param name="memberAccessDelegate">A <see cref="Func{TSource,TMapperSource}"/> that retrieves an instance of type <typeparamref name="TMapperSource"/> from the source instance.</param>
		/// <param name="mapper">The mapper accepting the delegated mapping.</param>
		/// <typeparam name="TMapperSource">The type of mapper accepting the delegated mapping.</typeparam>
		/// <typeparam name="TMapperTarget">The member type to map.</typeparam>
		void ByDelegatingTo<TMapperSource, TMapperTarget>(Func<TSource, TMapperSource> memberAccessDelegate, IMapper<TMapperSource, TMapperTarget> mapper)
			where TMapperTarget : TMember, new()
			where TMapperSource : class;

		/// <summary>
		/// Maps the member with another member.
		/// </summary>
		/// <param name="expression">A LINQ expression whose body is a <see cref="MemberExpression"/>.</param>
		/// <returns>A mapper that can configure the mapping.</returns>
		IMapperMapping<TSource, TTarget, TMember> And(Expression<Func<TTarget, TMember>> expression);
	}
}