using System.Collections.Generic;

using Junior.Map.Common.Conventions;

namespace Junior.Map.Common
{
	/// <summary>
	/// Provides default conventions used by convention-based mappers and adapters.
	/// </summary>
	public static class DefaultMappingConventionsProvider
	{
		private static readonly IEnumerable<IMappingConvention> _defaultConventions =
			new IMappingConvention[]
				{
					new OnlyNameMatchesButTypeIsConvertibleMappingConvention()
				};

		/// <summary>
		/// Gets default conventions used by convention-based mappers and adapters.
		/// </summary>
		public static IEnumerable<IMappingConvention> DefaultConventions
		{
			get
			{
				return _defaultConventions;
			}
		}
	}
}