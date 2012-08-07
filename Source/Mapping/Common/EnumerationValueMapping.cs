using System;

namespace Junior.Map.Common
{
    /// <summary>
    /// Represents a mapping from a source value to a target value.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    public class EnumerationValueMapping<TSource, TTarget>
    {
        private readonly Action _exceptionAction;
        private readonly TSource _sourceValue;
        private readonly TTarget _targetValue;

        private EnumerationValueMapping(TSource sourceValue, TTarget targetValue)
        {
            _sourceValue = sourceValue;
            _targetValue = targetValue;
        }

        private EnumerationValueMapping(TSource sourceValue, Action exceptionAction)
        {
            _sourceValue = sourceValue;
            _exceptionAction = exceptionAction;
        }

        /// <summary>
        /// The source value that will be mapped.
        /// </summary>
        public TSource SourceValue
        {
            get { return _sourceValue; }
        }

        /// <summary>
        /// Performs the configured mapping.  This will return the mapped value or throw an exception, depending on how the mapping was configured.
        /// </summary>
        /// <returns>The target value.</returns>
        public TTarget GetMappedValue()
        {
            if (_exceptionAction != null)
            {
                _exceptionAction();
            }

            return _targetValue;
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="EnumerationValueMapping{TSource,TTarget}"/> class that maps to the provided source value to the provided target value.
        /// </summary>
        /// <returns>The instance.</returns>
        public static EnumerationValueMapping<TSource, TTarget> CreateMemberMappingWithTargetMember(TSource sourceValue, TTarget targetValue)
        {
            return new EnumerationValueMapping<TSource, TTarget>(sourceValue, targetValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerationValueMapping{TSource,TTarget}"/> class that will throw an exception for the provided source value.
        /// </summary>
        /// <returns>The instance.</returns>
        public static EnumerationValueMapping<TSource, TTarget> CreateMemberMappingToException(TSource sourceValue, Action exceptionAction)
        {
            return new EnumerationValueMapping<TSource, TTarget>(sourceValue, exceptionAction);
        }
    }
}