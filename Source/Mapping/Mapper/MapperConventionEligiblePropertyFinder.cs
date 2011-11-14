using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Junior.Common;
using Junior.Mapping.Common;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Finds properties that are eligible for being mapped by convention when configuring mappers.
	/// </summary>
	public class MapperConventionEligiblePropertyFinder : IConventionEligiblePropertyFinder
	{
		/// <summary>
		/// Retrieves properties eligible for being mapped by convention.
		/// </summary>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <returns>Eligible properties.</returns>
		public IEnumerable<PropertyInfo> GetEligibleProperties<TTarget>()
		{
			return typeof(TTarget).GetAllPublicInstanceProperties()
				.Where(arg =>
				       	{
				       		MethodInfo setMethod = arg.GetSetMethod();

				       		return setMethod != null && !setMethod.IsStatic;
				       	});
		}
	}
}