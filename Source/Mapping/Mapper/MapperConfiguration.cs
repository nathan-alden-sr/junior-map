using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Junior.Common.Net35;
using Junior.Map.Common;

namespace Junior.Map.Mapper
{
	/// <summary>
	/// Stores mapping delegates used to map members of <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public class MapperConfiguration<TSource, TTarget> : IMappingConfiguration<TSource, TTarget>
	{
		private readonly List<MemberMappingAction<TTarget, TSource>> _customMemberMappingActions = new List<MemberMappingAction<TTarget, TSource>>();
		private readonly MappingConfigurationHelper<TSource, TTarget> _mappingConfigurationHelper = new MappingConfigurationHelper<TSource, TTarget>();
		private readonly Dictionary<string, MemberMappingAction<TTarget, TSource>> _memberMappingActionsByMemberName = new Dictionary<string, MemberMappingAction<TTarget, TSource>>();

		/// <summary>
		/// Gets configured mapping actions.
		/// </summary>
		public IEnumerable<MemberMappingAction<TTarget, TSource>> Actions
		{
			get
			{
				return _memberMappingActionsByMemberName.Values.Concat(_customMemberMappingActions);
			}
		}

		/// <summary>
		/// Gets configured mappings.
		/// </summary>
		public IEnumerable<MemberMapping<TSource>> Mappings
		{
			get
			{
				return _mappingConfigurationHelper.Mappings;
			}
		}

		IMapping<TSource, TMember> IMappingConfiguration<TSource, TTarget>.Map<TMember>(Expression<Func<TTarget, TMember>> expression)
		{
			return Map(expression);
		}

		IMapping<TSource, object> IMappingConfiguration<TSource, TTarget>.Map(string memberName)
		{
			return Map(memberName);
		}

		/// <summary>
		/// Creates a mapping for the member of <typeparamref name="TTarget"/> referenced by the specified LINQ expression.
		/// </summary>
		/// <typeparam name="TMember">The type of the target member.</typeparam>
		/// <param name="expression">A LINQ expression whose body is a <see cref="MemberExpression"/>.</param>
		/// <returns>A configurable mapping.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
		public IMapperMapping<TSource, TTarget, TMember> Map<TMember>(Expression<Func<TTarget, TMember>> expression)
		{
			expression.ThrowIfNull("expression");

			IMapping<TSource, TMember> mapping = _mappingConfigurationHelper.Map(expression);

			return new MapperMapping<TMember>(mapping.MemberName, this, _mappingConfigurationHelper);
		}

		/// <summary>
		/// Creates a mapping for a member of <typeparamref name="TTarget"/> with the specified name.
		/// </summary>
		/// <param name="memberName">The name of a member of <typeparamref name="TTarget"/>.</param>
		/// <returns>A configurable mapping.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="memberName"/> is null.</exception>
		public IMapperMapping<TSource, TTarget, object> Map(string memberName)
		{
			memberName.ThrowIfNull("memberName");

			_mappingConfigurationHelper.Map(memberName);

			return new MapperMapping<object>(memberName, this, _mappingConfigurationHelper);
		}

		/// <summary>
		/// Adds the specified delegate as a custom mapping delegate.
		/// </summary>
		/// <param name="customDelegate">A delegate.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="customDelegate"/> is null.</exception>
		public void AddCustomActionDelegate(Action<TTarget, TSource> customDelegate)
		{
			customDelegate.ThrowIfNull("customDelegate");

			_customMemberMappingActions.Add(new MemberMappingAction<TTarget, TSource>(null, customDelegate));
		}

		/// <summary>
		/// Ensures a <typeparamref name="TSource"/> instance can be mapped to a <typeparamref name="TTarget"/> instance.
		/// </summary>
		public void Validate()
		{
			_mappingConfigurationHelper.Validate(
				propertyInfo =>
				{
					bool propertyIsMappedByInvokingAction = Actions.Any(arg => arg.MemberName == propertyInfo.Name);
					bool propertyIsIgnored = Actions.Any(arg => arg.MemberName == propertyInfo.Name && arg.MapDelegate == null);

					return propertyIsMappedByInvokingAction || propertyIsIgnored;
				});
		}

		private class MapperMapping<TMember> : IMapperMapping<TSource, TTarget, TMember>
		{
			private readonly MapperConfiguration<TSource, TTarget> _configuration;
			private readonly MappingConfigurationHelper<TSource, TTarget> _helper;
			private readonly string _memberName;

			public MapperMapping(string memberName, MapperConfiguration<TSource, TTarget> configuration, MappingConfigurationHelper<TSource, TTarget> helper)
			{
				_memberName = memberName;
				_configuration = configuration;
				_helper = helper;
			}

			/// <summary>
			/// The name of the member being mapped.
			/// </summary>
			public string MemberName
			{
				get
				{
					return _memberName;
				}
			}

			/// <summary>
			/// Provides a delegate that retrieves the value of the member.
			/// </summary>
			/// <param name="valueDelegate">A <see cref="Func{TSource,TMember}"/> that retrieves the value of the member.</param>
			/// <exception cref="ArgumentNullException">Thrown when <paramref name="valueDelegate"/> is null.</exception>
			public void From(Func<TSource, TMember> valueDelegate)
			{
				valueDelegate.ThrowIfNull("valueDelegate");

				_helper.SetMapping(_memberName, new MemberMapping<TSource>(_memberName, source => valueDelegate(source)));
			}

			/// <summary>
			/// Provides a delegate that maps the member.
			/// </summary>
			/// <param name="mapDelegate">An <see cref="Action{TTarget,TSource}"/> that maps the member.</param>
			/// <exception cref="ArgumentNullException">Thrown when <paramref name="mapDelegate"/> is null.</exception>
			public void ByInvoking(Action<TTarget, TSource> mapDelegate)
			{
				mapDelegate.ThrowIfNull("mapDelegate");

				_configuration._memberMappingActionsByMemberName[_memberName] = new MemberMappingAction<TTarget, TSource>(_memberName, mapDelegate);
			}

			/// <summary>
			/// Ignores the member when performing a mapping.
			/// </summary>
			public void ByIgnoring()
			{
				_configuration._memberMappingActionsByMemberName[_memberName] = new MemberMappingAction<TTarget, TSource>(_memberName, null);
			}

			/// <summary>
			/// Maps the member by constructing a new instance of <typeparamref name="TMapperTarget"/> and delegating the actual mapping to the provided mapper.
			/// </summary>
			/// <param name="memberAccessDelegate">A <see cref="Func{TSource,TMapperSource}"/> that retrieves an instance of type <typeparamref name="TMapperSource"/> from the source instance.</param>
			/// <param name="mapper">The mapper accepting the delegated mapping.</param>
			/// <typeparam name="TMapperSource">The type of mapper accepting the delegated mapping.</typeparam>
			/// <typeparam name="TMapperTarget">The member type to map.</typeparam>
			/// <exception cref="ArgumentNullException">Thrown when <paramref name="memberAccessDelegate"/> is null.</exception>
			/// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
			public void ByDelegatingTo<TMapperSource, TMapperTarget>(Func<TSource, TMapperSource> memberAccessDelegate, IMapper<TMapperSource, TMapperTarget> mapper)
				where TMapperSource : class
				where TMapperTarget : TMember, new()
			{
				memberAccessDelegate.ThrowIfNull("memberAccessDelegate");
				mapper.ThrowIfNull("mapper");

				var memberMapping = new MemberMapping<TSource>(
					_memberName,
					source =>
					{
						TMapperSource sourceObject = memberAccessDelegate(source);

						if (sourceObject == null)
						{
							return null;
						}

						var targetTypeMemberInstance = new TMapperTarget();

						mapper.Map(sourceObject, targetTypeMemberInstance);

						return targetTypeMemberInstance;
					});

				_helper.SetMapping(_memberName, memberMapping);
			}

			/// <summary>
			/// Maps the member with another member.
			/// </summary>
			/// <param name="expression">A LINQ expression whose body is a <see cref="MemberExpression"/>.</param>
			/// <returns>A mapper that can configure the mapping.</returns>
			/// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
			/// <exception cref="ArgumentException">Thrown when the body of <paramref name="expression"/> is not a <see cref="MemberExpression"/>.</exception>
			IMapperMapping<TSource, TTarget, TMember> IMapperMapping<TSource, TTarget, TMember>.And(Expression<Func<TTarget, TMember>> expression)
			{
				expression.ThrowIfNull("expression");

				var memberExpression = expression.Body as MemberExpression;

				if (memberExpression == null)
				{
					throw new ArgumentException("Expression body must be a MemberExpression.", "expression");
				}

				_configuration._memberMappingActionsByMemberName[_memberName] = new MemberMappingAction<TTarget, TSource>(_memberName, null);

				return new MapperMapping<TMember>(memberExpression.Member.Name, _configuration, _helper);
			}
		}
	}
}