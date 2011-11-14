using System;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Maps bi-directionally between two types and allows creation of custom mappings at runtime.
	/// </summary>
	public abstract class BidirectionalMapper<T1, T2> : Mapper<T1, T2>
	{
		private readonly MappingMethodGenerator _generator = new MappingMethodGenerator();
		private readonly Lazy<Action<T2, T1>> _mapMethod;
		private readonly MapperConfiguration<T2, T1> _reverseConfiguration;
		private bool _isMappingConfigured;

		/// <summary>
		/// Initializes a new instance of the <see cref="BidirectionalMapper{T1,T2}"/> class.
		/// </summary>
		protected BidirectionalMapper()
			: this(new MapperConfiguration<T1, T2>(), new MapperConfiguration<T2, T1>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BidirectionalMapper{T1,T2}"/> class.
		/// <param name="configuration">A mapper configuration.</param>
		/// <param name="reverseConfiguration">A mapper configuration.</param>
		/// </summary>
		protected BidirectionalMapper(MapperConfiguration<T1, T2> configuration, MapperConfiguration<T2, T1> reverseConfiguration)
			: base(configuration)
		{
			_reverseConfiguration = reverseConfiguration;
			_mapMethod = new Lazy<Action<T2, T1>>(
				() =>
					{
						ConfigureReverseMapping();

						return _generator.GenerateMappingMethod(reverseConfiguration);
					});
		}

		/// <summary>
		/// Maps a source object to a target object.
		/// </summary>
		/// <param name="source">The source object.</param>
		/// <param name="target">The target object.</param>
		public virtual void Map(T2 source, T1 target)
		{
			_mapMethod.Value(source, target);
		}

		/// <summary>
		/// Ensures that mappings are valid.
		/// </summary>
		public override void Validate()
		{
			base.Validate();

			ConfigureReverseMapping();

			_reverseConfiguration.Validate();
		}

		/// <summary>
		/// Allows configuration of custom mappings at runtime through the specified mapper configuration.
		/// </summary>
		/// <param name="configuration">A mapper configuration.</param>
		protected abstract void ConfigureMapper(MapperConfiguration<T2, T1> configuration);

		private void ConfigureReverseMapping()
		{
			if (_isMappingConfigured)
			{
				return;
			}

			ConfigureMapper(_reverseConfiguration);
			_isMappingConfigured = true;
		}
	}
}