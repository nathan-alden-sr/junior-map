using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Common.Net35;
using Junior.Map.Common;

namespace Junior.Map.Mapper
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
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mappingConventions"/> is null.</exception>
		public static IEnumerable<IMapperConvention> AsMapperConventions(this IEnumerable<IMappingConvention> mappingConventions)
		{
			mappingConventions.ThrowIfNull("mappingConventions");

			return mappingConventions.Select(arg => (IMapperConvention)new MappingConventionAdapter(arg));
		}
	}
}