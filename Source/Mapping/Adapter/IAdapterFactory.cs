namespace Junior.Map.Adapter
{
	/// <summary>
	/// Represents a way to create an adapter that adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/>.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public interface IAdapterFactory<in TSource, out TTarget>
		where TSource : class
		where TTarget : class
	{
		/// <summary>
		/// Creates an adapter that adapts <paramref name="source"/> to an instance of <typeparamref name="TTarget"/>.
		/// </summary>
		/// <param name="source">An instance of <typeparamref name="TSource"/>.</param>
		/// <returns>An instance of <typeparamref name="TTarget"/> that is created at runtime.</returns>
		TTarget Create(TSource source);
	}

	/// <summary>
	/// Represents a way to create an adapter that adapts an instance of one type to another type.
	/// </summary>
	public interface IAdapterFactory
	{
		/// <summary>
		/// Creates an adapter that adapts an instance of one type to another type.
		/// </summary>
		/// <param name="source">A source object.</param>
		/// <returns>The object adapted to another type.</returns>
		object Create(object source);
	}
}