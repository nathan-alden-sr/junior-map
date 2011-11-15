using System;

using Junior.Common;
using Junior.Map.Common;

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
		private bool _isMapperConfigured;

		/// <summary>
		/// Initializes a new instance of the <see cref="Mapper{TSource,TTarget}"/> class.
		/// </summary>
		protected Mapper()
			: this(new MapperConfiguration<TSource, TTarget>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Mapper{TSource,TTarget}"/> class.
		/// </summary>
		/// <param name="configuration">The mapper configuration to use for mappings.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		protected Mapper(MapperConfiguration<TSource, TTarget> configuration)
		{
			configuration.ThrowIfNull("configuration");

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
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected abstract void ConfigureMapper(MapperConfiguration<TSource, TTarget> configuration);

		private void ConfigureMapper()
		{
			if (_isMapperConfigured)
			{
				return;
			}

			ConfigureMapper(_configuration);
			_isMapperConfigured = true;
		}
	}
}