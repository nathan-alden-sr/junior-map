using System.Collections.Generic;
using System.Linq;

using Junior.Mapping.Common;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Extensions for the <see cref="IEnumerable{T}"/> type.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Adapts <see cref="IMappingConvention"/> instances to <see cref="IMapperConvention"/> instances.
		/// </summary>
		/// <param name="mappingConventions">The mapping conventions to adapt.</param>
		/// <returns>The adapted mapper conventions.</returns>
		public static IEnumerable<IMapperConvention> AsMapperConventions(this IEnumerable<IMappingConvention> mappingConventions)
		{
			return mappingConventions.Select(arg => (IMapperConvention)new MappingConventionAdapter(arg));
		}
	}
}