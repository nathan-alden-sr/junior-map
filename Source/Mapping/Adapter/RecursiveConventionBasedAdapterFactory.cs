using System;
using System.Collections.Generic;

using Junior.Common;
using Junior.Map.Adapter.Conventions;
using Junior.Map.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Creates an adapter that recursively adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/> and creates custom mappings at runtime.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The source type.</typeparam>
	public abstract class RecursiveConventionBasedAdapterFactory<TSource, TTarget> : ConventionBasedAdapterFactory<TSource, TTarget>
		where TSource : class
		where TTarget : class
	{
		private readonly IAdapterFactoryLocator _locator;

		/// <summary>
		/// Initializes a new instance of the <see cref="RecursiveConventionBasedAdapterFactory{TSource,TTarget}"/> class.
		/// </summary>
		/// <param name="locator">An adapter factory locator.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="locator"/> is null.</exception>
		protected RecursiveConventionBasedAdapterFactory(IAdapterFactoryLocator locator)
		{
			locator.ThrowIfNull("locator");

			_locator = locator;
		}

		/// <summary>
		/// Retrieves mapping conventions to use when configuring mappings.
		/// </summary>
		/// <returns>The mapping conventions to use when configuring mappings.</returns>
		protected override IEnumerable<IMappingConvention> GetConventions()
		{
			foreach (IMappingConvention convention in DefaultMappingConventionsProvider.DefaultConventions)
			{
				yield return convention;
			}
			yield return new NamesMatchAndCanBeAdaptedMappingConvention(_locator);
		}
	}
}