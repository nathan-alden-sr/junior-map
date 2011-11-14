using System;

namespace Junior.Mapping.Adapter
{
	/// <summary>
	/// Locates instances of adapter factories.
	/// </summary>
	public interface IAdapterFactoryLocator
	{
		/// <summary>
		/// Locates an adapter factory that adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/>.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The source type.</typeparam>
		/// <returns>An adapter factory for the provided types.</returns>
		IAdapterFactory<TSource, TTarget> Locate<TSource, TTarget>()
			where TSource : class
			where TTarget : class;

		/// <summary>
		/// Locates an adapter factory that adapts a type to another type.
		/// </summary>
		/// <param name="sourceType">The source type.</param>
		/// <param name="targetType">The target type.</param>
		/// <returns>An adapter factory for the provided types.</returns>
		object Locate(Type sourceType, Type targetType);
	}
}