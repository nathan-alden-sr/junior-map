using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Junior.Mapping.Common;

namespace Junior.Mapping.UnitTests
{
	public class MappingConfiguration<TSource, TTarget> : IMappingConfiguration<TSource, TTarget>
	{
		private readonly MappingConfigurationHelper<TSource, TTarget> _mappingConfigurationHelper = new MappingConfigurationHelper<TSource, TTarget>();

		public IEnumerable<MemberMapping<TSource>> Mappings
		{
			get
			{
				return _mappingConfigurationHelper.Mappings;
			}
		}

		public IMapping<TSource, TMember> Map<TMember>(Expression<Func<TTarget, TMember>> expression)
		{
			return _mappingConfigurationHelper.Map(expression);
		}

		public IMapping<TSource, object> Map(string memberName)
		{
			return _mappingConfigurationHelper.Map(memberName);
		}
	}
}