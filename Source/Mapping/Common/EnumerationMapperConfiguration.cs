using System;
using System.Collections.Generic;

namespace Junior.Map.Common
{
	/// <summary>
	/// Stores mappings used to map members of enumeration type <typeparamref name="TSource"/> to enumeration type <typeparamref name="TTarget"/>.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public class EnumerationMapperConfiguration<TSource, TTarget>
	{
		private readonly Dictionary<TSource, EnumerationValueMapping<TSource, TTarget>> _mappings = new Dictionary<TSource, EnumerationValueMapping<TSource, TTarget>>();

		/// <summary>
		/// Gets configured value mappings.
		/// </summary>
		public IEnumerable<EnumerationValueMapping<TSource, TTarget>> Mappings
		{
			get
			{
				return _mappings.Values;
			}
		}

		/// <summary>
		/// Returns a target value equivalent to the specified source value.
		/// </summary>
		/// <param name="sourceValue">The value of <typeparamref name="TSource"/> to map.</param>
		/// <returns>A <typeparamref name="TTarget"/> value equivalent to <paramref name="sourceValue"/>.</returns>
		public TTarget GetMappedValue(TSource sourceValue)
		{
			return _mappings[sourceValue].GetMappedValue();
		}

		/// <summary>
		/// Creates a mapping for the provided member of <typeparamref name="TSource"/>.
		/// </summary>
		/// <param name="sourceMember">The source value.</param>
		/// <returns>A configurable mapping.</returns>
		public IEnumerationValueMapper<TTarget> Map(TSource sourceMember)
		{
			return new EnumerationValueMapper(sourceMember, this);
		}

		private class EnumerationValueMapper : IEnumerationValueMapper<TTarget>
		{
			private readonly EnumerationMapperConfiguration<TSource, TTarget> _enumerationMapperConfiguration;
			private readonly TSource _sourceValue;

			public EnumerationValueMapper(TSource sourceValue, EnumerationMapperConfiguration<TSource, TTarget> enumerationMapperConfiguration)
			{
				_sourceValue = sourceValue;
				_enumerationMapperConfiguration = enumerationMapperConfiguration;
			}

			public void To(TTarget targetValue)
			{
				_enumerationMapperConfiguration._mappings[_sourceValue] = EnumerationValueMapping<TSource, TTarget>.CreateMemberMappingWithTargetMember(_sourceValue, targetValue);
			}

			public void Throw<TException>()
				where TException : Exception, new()
			{
				_enumerationMapperConfiguration._mappings[_sourceValue] = EnumerationValueMapping<TSource, TTarget>.CreateMemberMappingToException(_sourceValue, () => { throw new TException(); });
			}

			public void Throw(string message = null)
			{
				_enumerationMapperConfiguration._mappings[_sourceValue] = EnumerationValueMapping<TSource, TTarget>.CreateMemberMappingToException(_sourceValue, () => { throw new Exception(message); });
			}
		}
	}
}