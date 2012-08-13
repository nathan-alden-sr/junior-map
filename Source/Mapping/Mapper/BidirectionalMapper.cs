using System;
using System.Collections.Generic;
using System.Linq;

using Junior.Map.Common;
using Junior.Map.Mapper.Conventions;

namespace Junior.Map.Mapper
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
	    private readonly MapperConventionEligiblePropertyFinder _propertyFinder = new MapperConventionEligiblePropertyFinder();
	    private IEnumerable<IMapperConvention> _conventions = Enumerable.Empty<IMapperConvention>();
        private static object _lockObject = new object();

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

	    private void ConfigureReverseMapping()
		{
			if (_isMappingConfigured)
			{
				return;
			}

            lock (_lockObject)
            {
                if (_isMappingConfigured)
                {
                    return;
                }

                _conventions = GetConventions();
                ApplyReverseConventions(_reverseConfiguration);
                ConfigureCustomMapping(_reverseConfiguration);
                _isMappingConfigured = true;                
            }
		}

        /// <summary>
        /// Retrieves mapper conventions to use when configuring mappings.
        /// </summary>
        /// <returns>The mapper conventions to use when configuring mappings.</returns>
        protected override IEnumerable<IMapperConvention> GetConventions()
        {
            yield return new NameAndTypeMatchMappingConvention();
            foreach (IMapperConvention convention in DefaultMappingConventionsProvider.DefaultConventions.AsMapperConventions())
            {
                yield return convention;
            }
        }

	    /// <summary>
	    /// Allows configuration of custom mappings at runtime through the specified mapper configuration.
	    /// </summary>
	    /// <param name="configuration">A mapper configuration.</param>
	    protected virtual void ConfigureCustomMapping(MapperConfiguration<T2, T1> configuration)
	    {	        
	    }

	    private void ApplyConventions(MapperConfiguration<T1, T2> configuration)
	    {
	        foreach (IMapperConvention convention in _conventions)
	        {
	            convention.Apply(_propertyFinder, configuration);
	        }
	    }

	    private void ApplyReverseConventions(MapperConfiguration<T2, T1> configuration)
	    {
	        foreach (IMapperConvention convention in _conventions)
	        {
	            convention.Apply(_propertyFinder, configuration);
	        }
	    }
	}
}