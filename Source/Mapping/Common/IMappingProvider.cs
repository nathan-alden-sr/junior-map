namespace Junior.Mapping.Common
{
	/// <summary>
	/// Represents a mapping provider that configures mappings between types at runtime.
	/// </summary>
	public interface IMappingProvider
	{
		/// <summary>
		/// Ensures that mappings are valid.
		/// </summary>
		void Validate();
	}
}