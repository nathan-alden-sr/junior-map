using System;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Specifies flags that control how an adapter factory configures the mappings.
	/// </summary>
	[Flags]
	public enum AdapterFactoryFlags
	{
		/// <summary>
		/// Specifies no adapter factory flag.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Specifies that the adapter factory should apply the recursive mapping convention such that the adapter will attempt to configure mappings for complex types exposed by the source and/or target types.
		/// </summary>
		ApplyRecursiveMappingConvention = 0x1,

		/// <summary>
		/// Specifies that the adapter factory should apply the default mapping conventions included with the framework.
		/// </summary>
		ApplyDefaultMappingConventions = 0x2
	}
}