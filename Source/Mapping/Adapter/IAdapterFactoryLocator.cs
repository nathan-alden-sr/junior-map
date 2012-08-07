using System;

namespace Junior.Map.Adapter
{
    /// <summary>
    /// Represents a way to locate an adapter factory.
    /// </summary>
    public interface IAdapterFactoryLocator
    {
        /// <summary>
        /// Locates an adapter factory that adapts a type to another type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <returns>>An adapter factory for the provided types.</returns>
        object Locate(Type sourceType, Type targetType);
    }
}