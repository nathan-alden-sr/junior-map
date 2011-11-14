using System;

namespace Junior.Mapping.Common
{
	/// <summary>
	/// Represents a way to configure a mapping for a property of <typeparamref name="TSource"/>.
	/// </summary>
	/// <typeparamref name="TMember">The type of the target member.</typeparamref>
	/// <typeparamref name="TSource">The source type.</typeparamref>
	public interface IMapping<out TSource, in TMember>
	{
		/// <summary>
		/// The name of the member being mapped.
		/// </summary>
		string MemberName
		{
			get;
		}

		/// <summary>
		/// Provides a delegate that retrieves the value of the member.
		/// </summary>
		/// <typeparamref name="TMember">The type of the target member.</typeparamref>
		/// <param name="valueDelegate">A <see cref="Func{TSource,TMember}"/> that retrieves the value of the member.</param>
		void From(Func<TSource, TMember> valueDelegate);
	}
}