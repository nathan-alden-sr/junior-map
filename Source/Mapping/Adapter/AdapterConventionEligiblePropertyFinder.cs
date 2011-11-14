using System.Collections.Generic;
using System.Reflection;

using Junior.Common;
using Junior.Mapping.Common;

namespace Junior.Mapping.Adapter
{
	/// <summary>
	/// Finds properties that are eligible for being mapped by convention when configuring adapters.
	/// </summary>
	public class AdapterConventionEligiblePropertyFinder : IConventionEligiblePropertyFinder
	{
		/// <summary>
		/// Retrieves properties eligible for being mapped by convention.
		/// </summary>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <returns>Eligible properties.</returns>
		public IEnumerable<PropertyInfo> GetEligibleProperties<TTarget>()
		{
			return typeof(TTarget).GetAllPublicInstanceProperties();
		}
	}
}