using System;

namespace Junior.Map.Common
{
	/// <summary>
	/// Represents a way to configure a mapping to a <typeparamref name="TTarget"/>.
	/// </summary>
	/// <typeparamref name="TTarget">The target type.</typeparamref>
	public interface IEnumerationValueMapper<TTarget>
	{
		/// <summary>
		/// Provides a mapping that maps to the provided value.
		/// </summary>
		/// <param name="targetValue">The target value.</param>
		void To(TTarget targetValue);

		/// <summary>
		/// Provides a mapping that throws an exception of the provided type.
		/// </summary>
		/// <typeparam name="TException">The type of exception to throw.  It must have a default constructor.</typeparam>
		void Throw<TException>()
			where TException : Exception, new();

		/// <summary>
		/// Provides a mapping that throws an exception with the provided message.
		/// </summary>
		/// <param name="message">A message.</param>
		void Throw(string message = null);
	}
}