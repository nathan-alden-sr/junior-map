namespace Junior.Map.Mapper
{
	/// <summary>
	/// Represents a way to map a source object to a target object.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public interface IMapper<in TSource, in TTarget>
	{
		/// <summary>
		/// Maps a source object to a target object.
		/// </summary>
		/// <param name="source">The source object.</param>
		/// <param name="target">The target object.</param>
		void Map(TSource source, TTarget target);
	}

	/// <summary>
	/// Represents a way to map a source object to a target object.
	/// </summary>
	public interface IMapper
	{
		/// <summary>
		/// Maps a source object to a target object.
		/// </summary>
		/// <param name="source">The source object.</param>
		/// <param name="target">The target object.</param>
		void Map(object source, object target);
	}
}