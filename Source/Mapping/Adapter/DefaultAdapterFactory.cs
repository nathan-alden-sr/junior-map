using System.Collections.Generic;

using Junior.Map.Adapter.Conventions;
using Junior.Map.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Creates an adapter that adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/> and creates custom mappings at runtime.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The source type.</typeparam>
	public class DefaultAdapterFactory<TSource, TTarget> : RecursiveConventionBasedAdapterFactory<TSource, TTarget>
		where TSource : class
		where TTarget : class
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultAdapterFactory{TSource,TTarget}"/> class.
		/// A <see cref="DefaultAdapterFactoryLocator"/> instance will be used to locate adapter factories.
		/// </summary>
		public DefaultAdapterFactory()
			: base(new DefaultAdapterFactoryLocator())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultAdapterFactory{TSource,TTarget}"/> class.
		/// </summary>
		/// <param name="locator">An adapter factory locator.</param>
		public DefaultAdapterFactory(IAdapterFactoryLocator locator)
			: base(locator)
		{
		}

		/// <summary>
		/// Retrieves mapper conventions to use when configuring mappings.
		/// </summary>
		/// <returns>The mapper conventions to use when configuring mappings.</returns>
		protected override IEnumerable<IMappingConvention> GetConventions()
		{
			foreach (IMappingConvention convention in base.GetConventions())
			{
				yield return convention;
			}
			yield return new NameAndTypeMatchAdapterMappingConvention();
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified adapter factory configuration.
		/// </summary>
		/// <param name="configuration">An adapter factory configuration.</param>
		protected override void ConfigureCustomMapping(AdapterFactoryConfiguration<TSource, TTarget> configuration)
		{
		}
	}
}