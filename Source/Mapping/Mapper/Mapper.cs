using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Common;
using Junior.Map.Common;
using Junior.Map.Mapper.Conventions;

namespace Junior.Map.Mapper
{
	/// <summary>
	/// Maps a source object to a target object and allows creation of custom mappings at runtime.
	/// </summary>
	/// <typeparam name="TSource">A source type.</typeparam>
	/// <typeparam name="TTarget">A target type.</typeparam>
	public abstract class Mapper<TSource, TTarget> : IMapper<TSource, TTarget>, IMapper, IMappingProvider
	{
		private readonly MapperConfiguration<TSource, TTarget> _configuration;
		private readonly MappingMethodGenerator _generator = new MappingMethodGenerator();
		private readonly Lazy<Action<TSource, TTarget>> _mapMethodDelegate;
		private readonly MapperFlags _mapperFlags;
		private readonly MapperConventionEligiblePropertyFinder _propertyFinder = new MapperConventionEligiblePropertyFinder();
		private IEnumerable<IMapperConvention> _conventions = Enumerable.Empty<IMapperConvention>();
		private bool _isMapperConfigured;

		/// <summary>
		/// Initializes a new instance of the <see cref="Mapper{TSource,TTarget}"/> class.
		/// </summary>
		protected Mapper()
			: this(MapperFlags.ApplyDefaultMappingConventions)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Mapper{TSource,TTarget}"/> class.
		/// </summary>
		protected Mapper(MapperFlags mapperFlags)
			: this(mapperFlags, new MapperConfiguration<TSource, TTarget>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Mapper{TSource,TTarget}"/> class.
		/// </summary>
		/// <param name="configuration">The mapper configuration to use for mappings.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		protected Mapper(MapperConfiguration<TSource, TTarget> configuration)
			: this(MapperFlags.ApplyDefaultMappingConventions, configuration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Mapper{TSource,TTarget}"/> class.
		/// </summary>
		/// <param name="mapperFlags">Mapper flags.</param>
		/// <param name="configuration">The mapper configuration to use for mappings.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		protected Mapper(MapperFlags mapperFlags, MapperConfiguration<TSource, TTarget> configuration)
		{
			configuration.ThrowIfNull("configuration");

			_mapperFlags = mapperFlags;
			_configuration = configuration;
			_mapMethodDelegate = new Lazy<Action<TSource, TTarget>>(
				() =>
					{
						ConfigureMapper();

						return _generator.GenerateMappingMethod(_configuration);
					});
		}

		void IMapper.Map(object source, object target)
		{
			Map((TSource)source, (TTarget)target);
		}

		/// <summary>
		/// Maps a source object to a target object.
		/// </summary>
		/// <param name="source">The source object.</param>
		/// <param name="target">The target object.</param>
		public void Map(TSource source, TTarget target)
		{
			_mapMethodDelegate.Value(source, target);
		}

		/// <summary>
		/// Ensures that mappings are valid.
		/// </summary>
		public virtual void Validate()
		{
			ConfigureMapper();

			_configuration.Validate();
		}

		/// <summary>
		/// Maps a source object to a target object created by calling the provided factory Func.
		/// </summary>
		/// <param name="source">The source object.</param>
		/// <param name="factoryFunc">A factory func to use when creating the target object.</param>
		/// <returns>The created and mapped target object.</returns>
		public TTarget Map(TSource source, Func<TTarget> factoryFunc)
		{
			TTarget target = factoryFunc();
			_mapMethodDelegate.Value(source, target);

			return target;
		}

		/// <summary>
		/// Maps a source object to a target object created using the default constructor.
		/// </summary>
		/// <param name="source">The source object.</param>
		/// <returns>The created and mapped target object.</returns>
		public TMappingTarget Map<TMappingTarget>(TSource source)
			where TMappingTarget : TTarget, new()
		{
			var target = new TMappingTarget();
			_mapMethodDelegate.Value(source, target);

			return target;
		}

		/// <summary>
		/// Maps an enumerable of source objects to an enumerable of target objects created using the default constructor.
		/// </summary>
		/// <param name="sources">The source objects.</param>
		/// <returns>The created and mapped target objects.</returns>
		public IEnumerable<TMappingTarget> Map<TMappingTarget>(IEnumerable<TSource> sources)
			where TMappingTarget : TTarget, new()
		{
			var mappingTargets = new List<TMappingTarget>();
			foreach (TSource source in sources)
			{
				var mappingTarget = new TMappingTarget();
				mappingTargets.Add(mappingTarget);

				Map(source, mappingTarget);
			}

			return mappingTargets;
		}

		/// <summary>
		/// Maps an enumerable of source objects to an enumerable of target objects created using provided factory Func.
		/// </summary>
		/// <param name="sources">The source objects.</param>
		/// <param name="factoryFunc">A factory func to use when creating the target objects.</param>
		/// <returns>The created and mapped target objects.</returns>
		public IEnumerable<TTarget> Map(IEnumerable<TSource> sources, Func<TTarget> factoryFunc)
		{
			var mappingTargets = new List<TTarget>();
			foreach (TSource source in sources)
			{
				TTarget mappingTarget = factoryFunc();
				mappingTargets.Add(mappingTarget);

				Map(source, mappingTarget);
			}

			return mappingTargets;
		}

		private void ConfigureMapper()
		{
			if (_isMapperConfigured)
			{
				return;
			}

			ConfigureMapper(_configuration);
			_isMapperConfigured = true;
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		private void ConfigureMapper(MapperConfiguration<TSource, TTarget> configuration)
		{
			configuration.ThrowIfNull("configuration");

			_conventions = GetConventions();
			ApplyConventions(configuration);
			ConfigureCustomMapping(configuration);
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected virtual void ConfigureCustomMapping(MapperConfiguration<TSource, TTarget> configuration)
		{
		}

		/// <summary>
		/// Retrieves mapper conventions to use when configuring mappings.
		/// </summary>
		/// <returns>The mapper conventions to use when configuring mappings.</returns>
		protected virtual IEnumerable<IMapperConvention> GetConventions()
		{
			if ((_mapperFlags & MapperFlags.ApplyDefaultMappingConventions) == MapperFlags.ApplyDefaultMappingConventions)
			{
				foreach (IMapperConvention convention in DefaultMappingConventionsProvider.DefaultConventions.AsMapperConventions())
				{
					yield return convention;
				}
				yield return new NameAndTypeMatchMappingConvention();
			}
		}

		private void ApplyConventions(MapperConfiguration<TSource, TTarget> configuration)
		{
			foreach (IMapperConvention convention in _conventions)
			{
				convention.Apply(_propertyFinder, configuration);
			}
		}
	}
}