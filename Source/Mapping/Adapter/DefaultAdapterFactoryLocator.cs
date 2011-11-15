using System;

using Junior.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Locates instances of adapter factories.
	/// </summary>
	public class DefaultAdapterFactoryLocator : IAdapterFactoryLocator
	{
		/// <summary>
		/// Locates an adapter factory that adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/>.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <returns>An adapter factory for the provided types.</returns>
		public IAdapterFactory<TSource, TTarget> Locate<TSource, TTarget>()
			where TSource : class
			where TTarget : class
		{
			return new DefaultAdapterFactory<TSource, TTarget>(this);
		}

		/// <summary>
		/// Locates an adapter factory that adapts a type to another type.
		/// </summary>
		/// <param name="sourceType">The source type.</param>
		/// <param name="targetType">The target type.</param>
		/// <returns>An adapter factory for the provided types.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceType"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType"/> is null.</exception>
		public object Locate(Type sourceType, Type targetType)
		{
			sourceType.ThrowIfNull("sourceType");
			targetType.ThrowIfNull("targetType");

			Type adapterFactoryType = typeof(DefaultAdapterFactory<,>);
			Type combinedType = adapterFactoryType.MakeGenericType(sourceType, targetType);

			return Activator.CreateInstance(combinedType, this);
		}
	}
}