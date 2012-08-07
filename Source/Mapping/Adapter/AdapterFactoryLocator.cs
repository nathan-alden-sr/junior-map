using System;
using System.Collections.Generic;

using Junior.Common;

namespace Junior.Map.Adapter
{
    /// <summary>
    /// Locates instances of adapter factories.
    /// </summary>
    public class AdapterFactoryLocator
    {
        private static readonly Dictionary<Tuple<Type, Type>, object> _instancedAdapterFactories = new Dictionary<Tuple<Type, Type>, object>();
        private static readonly object _instancedAdapterFactoriesLock = new object();
        private readonly IAdapterFactoryLocator _adapterFactoryLocator;
        private readonly Dictionary<Tuple<Type, Type>, Func<object>> _registeredAdapterFactories = new Dictionary<Tuple<Type, Type>, Func<object>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterFactoryLocator"/> class with default behaviors.
        /// </summary>
        public AdapterFactoryLocator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterFactoryLocator"/> class with default behaviors and the given adapter factory locator.
        /// </summary>
        /// <param name="adapterFactoryLocator">An adapter factory locator to use for instances not directly registered with this locator.</param>
        public AdapterFactoryLocator(IAdapterFactoryLocator adapterFactoryLocator)
        {
            _adapterFactoryLocator = adapterFactoryLocator;
        }

        /// <summary>
        /// Locates an adapter factory that adapts a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <returns>An adapter factory for the provided types.</returns>
        public IAdapterFactory<TSource, TTarget> Locate<TSource, TTarget>()
            where TSource : class
            where TTarget : class
        {
            return (IAdapterFactory<TSource, TTarget>)Locate(typeof(TSource), typeof(TTarget));
        }

        /// <summary>
        /// Locates an adapter factory that adapts a type to another type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <returns>An adapter factory for the provided types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceType"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType"/> is null.</exception>
        public object Locate(Type sourceType, Type targetType)
        {
            sourceType.ThrowIfNull("sourceType");
            targetType.ThrowIfNull("targetType");

            var key = new Tuple<Type, Type>(sourceType, targetType);

            object instancedAdapterFactory;
            if (_instancedAdapterFactories.TryGetValue(key, out instancedAdapterFactory))
            {
                return instancedAdapterFactory;
            }

            Func<object> registeredAdapterFactoryCreateFunc;
            if (_registeredAdapterFactories.TryGetValue(key, out registeredAdapterFactoryCreateFunc))
            {
                object adapterFactory = registeredAdapterFactoryCreateFunc();
                lock (_instancedAdapterFactoriesLock)
                {
                    _instancedAdapterFactories.Add(key, adapterFactory);
                }
                return adapterFactory;
            }

            if (_adapterFactoryLocator != null)
            {
                return _adapterFactoryLocator.Locate(sourceType, targetType);
            }

            Type adapterFactoryType = typeof(DefaultAdapterFactory<,>);
            Type combinedType = adapterFactoryType.MakeGenericType(sourceType, targetType);

            return Activator.CreateInstance(combinedType);
        }

        /// <summary>
        /// Registers an adapter factory type that should be used when <typeparamref name="TTarget"/> has a <typeparamref name="TTarget"/> that should be adapted from a <typeparamref name="TSource"/>.
        /// </summary>
        /// <param name="adapterFactoryCreateFunc">A func that will create an instance of the registered adapter factory.</param>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="adapterFactoryCreateFunc"/> is null.</exception>
        public void RegisterAdapterFactory<TSource, TTarget>(Func<IAdapterFactory<TSource, TTarget>> adapterFactoryCreateFunc)
            where TTarget : class
            where TSource : class
        {
            adapterFactoryCreateFunc.ThrowIfNull("adapterFactoryCreateFunc");

            _registeredAdapterFactories[new Tuple<Type, Type>(typeof(TSource), typeof(TTarget))] = adapterFactoryCreateFunc;
        }

        /// <summary>
        /// Registers an adapter factory type that should be used when <typeparamref name="TTarget"/> has a <typeparamref name="TTarget"/> that should be adapted from a <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <typeparam name="TAdapterType">The adapter factory type to use.  It must have a default constructor.</typeparam>
        public void RegisterAdapterFactory<TSource, TTarget, TAdapterType>()
            where TAdapterType : class, IAdapterFactory<TSource, TTarget>, new()
            where TTarget : class
            where TSource : class
        {
            _registeredAdapterFactories[new Tuple<Type, Type>(typeof(TSource), typeof(TTarget))] = () => new TAdapterType();
        }

        private class DefaultAdapterFactory<TSource, TTarget> : AdapterFactory<TSource, TTarget>
            where TSource : class
            where TTarget : class
        {
            protected override void ConfigureCustomMapping(AdapterFactoryConfiguration<TSource, TTarget> configuration)
            {
            }
        }
    }
}