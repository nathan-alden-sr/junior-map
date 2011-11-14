using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Common;

namespace Junior.Mapping.Common
{
	/// <summary>
	/// Maps a <typeparamref name="TSource"/> enum value to an equivalent <typeparamref name="TTarget"/> enum value
	/// if <typeparamref name="TSource"/> and <typeparamref name="TTarget"/> have the same fields.
	/// <see cref="DefaultEnumerationMapper{TSource,TTarget}"/> automatically configures mappings and does not allow customization.
	/// Supports enum types and their nullable equivalents. <typeparamref name="TSource"/> and <typeparamref name="TTarget"/> must be either both nullable or both non-nullable.
	/// </summary>
	/// <typeparam name="TSource">The source enum type.</typeparam>
	/// <typeparam name="TTarget">The target enum type.</typeparam>
	public class DefaultEnumerationMapper<TSource, TTarget> : IMappingProvider
	{
		private readonly bool _isNullableMap;
		private readonly PairMapper<TSource, TTarget> _pairMapper = new PairMapper<TSource, TTarget>();
		private readonly Type _sourceType;
		private readonly Type _targetType;
		private bool _isMapperConfigured;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultEnumerationMapper{TSource,TTarget}"/> class.
		/// </summary>
		/// <exception cref="InvalidGenericTypeArgumentException">Thrown when <typeparamref name="TSource"/> is nullable but <typeparamref name="TTarget"/> is non-nullable.</exception>
		/// <exception cref="InvalidGenericTypeArgumentException">Thrown when <typeparamref name="TSource"/> is nullable but <typeparamref name="TTarget"/> is non-nullable.</exception>
		/// <exception cref="InvalidGenericTypeArgumentException">Thrown when <typeparamref name="TSource"/> is not an enum.</exception>
		/// <exception cref="InvalidGenericTypeArgumentException">Thrown when <typeparamref name="TTarget"/> is not an enum.</exception>
		public DefaultEnumerationMapper()
		{
			ValidateGenericTypeArgument(typeof(TSource), "TSource");
			ValidateGenericTypeArgument(typeof(TTarget), "TTarget");

			Type underlyingSourceType = Nullable.GetUnderlyingType(typeof(TSource));
			Type underlyingTargetType = Nullable.GetUnderlyingType(typeof(TTarget));

			if (underlyingSourceType != null && underlyingTargetType == null)
			{
				throw new InvalidGenericTypeArgumentException("Type must be nullable because TSource is nullable.", "TTarget");
			}
			if (underlyingSourceType == null && underlyingTargetType != null)
			{
				throw new InvalidGenericTypeArgumentException("Type must be non-nullable because TSource is non-nullable.", "TTarget");
			}

			_isNullableMap = underlyingSourceType != null;
			_sourceType = underlyingSourceType ?? typeof(TSource);
			_targetType = underlyingTargetType ?? typeof(TTarget);
		}

		/// <summary>
		/// Ensures that mappings are valid.
		/// </summary>
		public void Validate()
		{
			ConfigureMapper();
		}

		/// <summary>
		/// Returns a target value equivalent to the specified source value.
		/// </summary>
		/// <param name="value">The value of <typeparamref name="TSource"/> to map.</param>
		/// <returns>A <typeparamref name="TTarget"/> value equivalent to <paramref name="value"/>.</returns>
		public TTarget GetMappedValue(TSource value)
		{
			ConfigureMapper();

			// _isNullableMap being true guarantees that comparing value with null is a valid comparison.
			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (_isNullableMap && value == null)
				// ReSharper restore CompareNonConstrainedGenericWithNull
			{
				// default() returns null because TTarget is a nullable enum.
				return default(TTarget);
			}

			return _pairMapper.Map(value);
		}

		/// <summary>
		/// Automatically configures mappings from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
		/// </summary>
		/// <exception cref="Exception">Thrown when <typeparamref name="TTarget"/> contains unmapped values.</exception>
		protected void ConfigureMapper()
		{
			if (_isMapperConfigured)
			{
				return;
			}

			IEnumerable<string> sourceNames = Enum.GetNames(_sourceType);
			IEnumerable<string> targetNames = Enum.GetNames(_targetType);

			foreach (string targetName in targetNames.Intersect(sourceNames))
			{
				_pairMapper.Add((TSource)Enum.Parse(_sourceType, targetName), (TTarget)Enum.Parse(_targetType, targetName));
			}

			ValidateConfiguration();
			_isMapperConfigured = true;
		}

		private void ValidateConfiguration()
		{
			IEnumerable<string> unmappedValuesInTargetType = Enum.GetNames(_targetType)
				.Except(_pairMapper.Select(arg => arg.Second.ToString()))
				.ToArray();

			if (!unmappedValuesInTargetType.Any())
			{
				return;
			}

			string errorMessage = String.Format("Type '{0}' contains unmapped members.{1}", typeof(TTarget).FullName, String.Join(", ", unmappedValuesInTargetType));

			throw new Exception(errorMessage);
		}

		private static void ValidateGenericTypeArgument(Type type, string name)
		{
			Type underlyingType = Nullable.GetUnderlyingType(type);

			if (underlyingType != null && !underlyingType.IsEnum)
			{
				throw new InvalidGenericTypeArgumentException("Underlying type must be an enum.", name);
			}
			if (underlyingType == null && !type.IsEnum)
			{
				throw new InvalidGenericTypeArgumentException("Type must be an enum.", name);
			}
		}
	}
}