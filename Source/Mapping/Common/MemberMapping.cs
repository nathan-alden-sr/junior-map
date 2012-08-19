using System;

using Junior.Common;

namespace Junior.Map.Common
{
	/// <summary>
	/// Provides a delegate used to retrieve the associated member's value.
	/// </summary>
	/// <typeparamref name="TSource">The type of the source member.</typeparamref>
	public class MemberMapping<TSource>
	{
		private readonly string _memberName;
		private readonly Func<TSource, object> _valueDelegate;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberMapping{T}"/> class.
		/// </summary>
		/// <param name="memberName">A member name.</param>
		/// <param name="valueDelegate">A delegate that returns the value of the member.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="memberName"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="valueDelegate"/> is null.</exception>
		public MemberMapping(string memberName, Func<TSource, object> valueDelegate)
		{
			memberName.ThrowIfNull("memberName");
			valueDelegate.ThrowIfNull("valueDelegate");

			_memberName = memberName;
			_valueDelegate = valueDelegate;
		}

		/// <summary>
		/// Gets the member name.
		/// </summary>
		public string MemberName
		{
			get
			{
				return _memberName;
			}
		}

		/// <summary>
		/// Gets a delegate that returns the member's value.
		/// </summary>
		public Func<TSource, object> ValueDelegate
		{
			get
			{
				return _valueDelegate;
			}
		}
	}
}