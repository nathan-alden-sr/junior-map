using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Junior.Common;

namespace Junior.Mapping.Common
{
	/// <summary>
	/// Extensions for the <see cref="IEnumerable{T}"/> type.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Finds a property that matches the specified property name. Case-sensitive matches are favored over case-insensitive matches.
		/// </summary>
		/// <param name="propertyInfos"><see cref="PropertyInfo"/> instances to search.</param>
		/// <param name="propertyName">The property name for which to search.</param>
		/// <returns>The best-matching property if exactly one was found; otherwise, null.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfos"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyName"/> is null.</exception>
		public static PropertyInfo FindMatchingProperty(this IEnumerable<PropertyInfo> propertyInfos, string propertyName)
		{
			propertyInfos.ThrowIfNull("propertyInfos");
			propertyName.ThrowIfNull("propertyName");

			IEnumerable<PropertyInfo> propertyInfoList = propertyInfos.ToArray();
			IEnumerable<PropertyInfo> caseSensitiveMatches = propertyInfoList
				.Where(arg => arg.Name.Equals(propertyName))
				.Take(2)
				.ToArray();

			if (caseSensitiveMatches.Count() > 1)
			{
				return null;
			}

			PropertyInfo caseSensitiveProperty = caseSensitiveMatches.SingleOrDefault();

			if (caseSensitiveProperty != null)
			{
				return caseSensitiveProperty;
			}

			IEnumerable<PropertyInfo> caseInsensitiveMatches = propertyInfoList
				.Where(arg => arg.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
				.Take(2)
				.ToArray();

			return caseInsensitiveMatches.Count() == 1 ? caseInsensitiveMatches.Single() : null;
		}
	}
}