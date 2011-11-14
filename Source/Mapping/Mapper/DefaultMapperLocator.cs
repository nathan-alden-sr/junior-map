using System;
using System.Collections.Generic;

using Junior.Common;
using Junior.Mapping.Common;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Locates instances of mappers.
	/// </summary>
	public class DefaultMapperLocator : IMapperLocator
	{
		private static readonly object _lockObject = new object();
		private static readonly Dictionary<Pair<Type, Type>, IMapper> _mapperCache = new Dictionary<Pair<Type, Type>, IMapper>();

		/// <summary>
		/// Locates a mapper that maps a <typeparamref name="TSource"/> instance to <typeparamref name="TTarget"/>.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <returns>A mapper for the provided types.</returns>
		public IMapper<TSource, TTarget> Locate<TSource, TTarget>()
			where TSource : class
			where TTarget : class
		{
			var cachePair = new Pair<Type, Type>(typeof(TSource), typeof(TTarget));
			IMapper mapper;

			lock (_lockObject)
			{
				if (!_mapperCache.TryGetValue(cachePair, out mapper))
				{
					mapper = new DefaultMapper<TSource, TTarget>();
					_mapperCache.Add(cachePair, mapper);
				}
			}

			return (Mapper<TSource, TTarget>)mapper;
		}

		/// <summary>
		/// Locates a mapper that adapts a type to another type.
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

			var cachePair = new Pair<Type, Type>(sourceType, targetType);
			IMapper mapper;

			lock (_lockObject)
			{
				if (!_mapperCache.TryGetValue(cachePair, out mapper))
				{
					Type type = typeof(DefaultMapper<,>);
					Type combinedType = type.MakeGenericType(sourceType, targetType);

					mapper = (IMapper)Activator.CreateInstance(combinedType, null);
					_mapperCache.Add(cachePair, mapper);
				}
			}

			return mapper;
		}
	}
}