using System;

using Junior.Mapping.Mapper;

namespace Junior.Mapping.Common
{
	/// <summary>
	/// Locates instances of mappers.
	/// </summary>
	public interface IMapperLocator
	{
		/// <summary>
		/// Locates a mapper that maps a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/>.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The source type.</typeparam>
		/// <returns>A mapper for the provided types.</returns>
		IMapper<TSource, TTarget> Locate<TSource, TTarget>()
			where TSource : class
			where TTarget : class;

		/// <summary>
		/// Locates a mapper that adapts a type to another type.
		/// </summary>
		/// <param name="sourceType">The source type.</param>
		/// <param name="targetType">The target type.</param>
		/// <returns>An adapter factory for the provided types.</returns>
		object Locate(Type sourceType, Type targetType);
	}
}