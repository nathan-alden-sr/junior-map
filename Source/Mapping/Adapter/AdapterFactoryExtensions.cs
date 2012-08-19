using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Extension methods for the <see cref="IAdapterFactory{TSource,TTarget}"/> type.
	/// </summary>
	public static class AdapterFactoryExtensions
	{
		/// <summary>
		/// Creates adapters that adapt <paramref name="sourceInstances"/> to instances of <typeparamref name="TTarget"/>.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <param name="adapterFactory">An adapter factory.</param>
		/// <param name="sourceInstances"><typeparamref name="TSource"/> instances for which to create adapters.</param>
		/// <returns>Instances of <typeparamref name="TTarget"/> that are created at runtime.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceInstances"/> is null.</exception>
		public static IEnumerable<TTarget> CreateMany<TSource, TTarget>(this IAdapterFactory<TSource, TTarget> adapterFactory, IEnumerable<TSource> sourceInstances)
			where TSource : class
			where TTarget : class
		{
			sourceInstances.ThrowIfNull("sourceInstances");

			return sourceInstances.Select(adapterFactory.Create);
		}
	}
}