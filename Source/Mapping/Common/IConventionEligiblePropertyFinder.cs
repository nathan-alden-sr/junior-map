using System.Collections.Generic;
using System.Reflection;

namespace Junior.Map.Common
{
	/// <summary>
	/// Finds properties that are eligible for being mapped by convention.
	/// </summary>
	public interface IConventionEligiblePropertyFinder
	{
		/// <summary>
		/// Retrieves properties eligible for being mapped by convention.
		/// </summary>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <returns>Eligible properties.</returns>
		IEnumerable<PropertyInfo> GetEligibleProperties<TTarget>();
	}
}