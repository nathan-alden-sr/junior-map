using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Common;

namespace Junior.Map.Common
{
	/// <summary>
	/// Maps a <typeparamref name="TSource"/> enum value to an equivalent <typeparamref name="TTarget"/> enum value
	/// if <typeparamref name="TTarget"/> has all fields in <typeparamref name="TSource"/>.
	/// <see cref="EnumerationMapper{TSource,TTarget}"/> automatically configures mappings and optionally allows customization.
	/// Supports enum types and their nullable equivalents. <typeparamref name="TSource"/> and <typeparamref name="TTarget"/> must be either both nullable or both non-nullable.
	/// </summary>
	/// <typeparam name="TSource">The source enum type.</typeparam>
	/// <typeparam name="TTarget">The target enum type.</typeparam>
	public abstract class EnumerationMapper<TSource, TTarget> : IMappingProvider
	{
		// ReSharper disable StaticFieldInGenericType
		private static readonly object _lockObject = new object();
		// ReSharper restore StaticFieldInGenericType
		private readonly EnumerationMapperConfiguration<TSource, TTarget> _configuration = new EnumerationMapperConfiguration<TSource, TTarget>();
		private readonly bool _isNullableMap;
		private readonly Type _sourceType;
		private readonly Type _targetType;
		private bool _isMapperConfigured;

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumerationMapper{TSource,TTarget}"/> class.
		/// </summary>
		/// <exception cref="InvalidGenericTypeArgumentException">Thrown when <typeparamref name="TSource"/> is nullable but <typeparamref name="TTarget"/> is non-nullable.</exception>
		/// <exception cref="InvalidGenericTypeArgumentException">Thrown when <typeparamref name="TSource"/> is nullable but <typeparamref name="TTarget"/> is non-nullable.</exception>
		/// <exception cref="InvalidGenericTypeArgumentException">Thrown when <typeparamref name="TSource"/> is not an enum.</exception>
		/// <exception cref="InvalidGenericTypeArgumentException">Thrown when <typeparamref name="TTarget"/> is not an enum.</exception>
		protected EnumerationMapper()
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

			return _configuration.GetMappedValue(value);
		}

		/// <summary>
		/// Automatically configures mappings from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
		/// </summary>
		/// <exception cref="Exception">Thrown when <typeparamref name="TTarget"/> contains unmapped values.</exception>
		private void ConfigureMapper()
		{
			if (_isMapperConfigured)
			{
				return;
			}

			lock (_lockObject)
			{
				if (_isMapperConfigured)
				{
					return;
				}

				IEnumerable<string> sourceNames = Enum.GetNames(_sourceType);
				IEnumerable<string> targetNames = Enum.GetNames(_targetType);

				foreach (string targetName in targetNames.Intersect(sourceNames))
				{
					_configuration.Map((TSource)Enum.Parse(_sourceType, targetName)).To((TTarget)Enum.Parse(_targetType, targetName));
				}
				ConfigureMapper(_configuration);

				ValidateConfiguration();
				_isMapperConfigured = true;
			}
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified enumeration mapper configuration.
		/// </summary>
		/// <param name="configuration">An enumeration mapper configuration.</param>
		protected virtual void ConfigureMapper(EnumerationMapperConfiguration<TSource, TTarget> configuration)
		{
		}

		private void ValidateConfiguration()
		{
			IEnumerable<string> unmappedSourceTypeValues = Enum.GetNames(_sourceType)
				.Except(_configuration.Mappings.Select(mapping => mapping.SourceValue.ToString()))
				.ToArray();

			if (!unmappedSourceTypeValues.Any())
			{
				return;
			}

			string errorMessage = String.Format("Source type '{0}' contains unmapped members: {1}", typeof(TSource).FullName, String.Join(", ", unmappedSourceTypeValues));

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