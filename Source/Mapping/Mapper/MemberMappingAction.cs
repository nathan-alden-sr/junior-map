using System;

namespace Junior.Map.Mapper
{
	/// <summary>
	/// Provides a delegate used to retrieve the associated member's value.
	/// </summary>
	/// <typeparamref name="TTarget">The type of the target member.</typeparamref>
	/// <typeparamref name="TSource">The type of the source member.</typeparamref>
	public class MemberMappingAction<TTarget, TSource>
	{
		private readonly Action<TTarget, TSource> _mapDelegate;
		private readonly string _memberName;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberMappingAction{TTarget,TSource}"/> type.
		/// </summary>
		/// <param name="memberName">A member name.</param>
		/// <param name="mapDelegate">An Action to execute for mapping the member.</param>
		public MemberMappingAction(string memberName, Action<TTarget, TSource> mapDelegate)
		{
			_memberName = memberName;
			_mapDelegate = mapDelegate;
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
		/// Gets a delegate that maps a source member to a target member.
		/// </summary>
		public Action<TTarget, TSource> MapDelegate
		{
			get
			{
				return _mapDelegate;
			}
		}
	}
}