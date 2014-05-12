using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Junior.Common.Net35;

namespace Junior.Map.Common
{
	/// <summary>
	/// Stores mapping delegates used to map members of <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TTarget">The target type.</typeparam>
	public class MappingConfigurationHelper<TSource, TTarget>
	{
		private readonly Dictionary<string, MemberMapping<TSource>> _memberMappingsByMemberName = new Dictionary<string, MemberMapping<TSource>>();

		/// <summary>
		/// Gets configured mappings.
		/// </summary>
		public IEnumerable<MemberMapping<TSource>> Mappings
		{
			get
			{
				return _memberMappingsByMemberName.Values;
			}
		}

		/// <summary>
		/// Creates a mapping for the member of <typeparamref name="TTarget"/> referenced by the specified LINQ expression.
		/// </summary>
		/// <typeparam name="TMember">The type of the target member.</typeparam>
		/// <param name="expression">A LINQ expression whose body is a <see cref="MemberExpression"/>.</param>
		/// <returns>A configurable mapping.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
		public IMapping<TSource, TMember> Map<TMember>(Expression<Func<TTarget, TMember>> expression)
		{
			expression.ThrowIfNull("expression");

			var memberExpression = expression.Body as MemberExpression;

			if (memberExpression == null)
			{
				throw new ArgumentException("Expression body must be a MemberExpression.", "expression");
			}

			return new Mapping<TMember>(memberExpression.Member.Name, this);
		}

		/// <summary>
		/// Creates a mapping for a member of <typeparamref name="TTarget"/> with the specified name.
		/// </summary>
		/// <param name="memberName">The name of a member of <typeparamref name="TTarget"/>.</param>
		/// <returns>A configurable mapping.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="memberName"/> is null.</exception>
		public IMapping<TSource, object> Map(string memberName)
		{
			memberName.ThrowIfNull("memberName");

			return new Mapping<object>(memberName, this);
		}

		/// <summary>
		/// Ensures a <typeparamref name="TSource"/> instance can be mapped to a <typeparamref name="TTarget"/> instance.
		/// </summary>
		/// <param name="propertyValidationDelegate">An optional delegate that validates properties.</param>
		/// <exception cref="Exception">Thrown when mapping validation fails.</exception>
		public void Validate(Func<PropertyInfo, bool> propertyValidationDelegate = null)
		{
			var errorMessages = new List<string>();
			IEnumerable<PropertyInfo> publicInstanceProperties = typeof(TTarget).GetAllPublicInstanceProperties();

			foreach (PropertyInfo propertyInfo in publicInstanceProperties)
			{
				PropertyInfo tempPropertyInfo = propertyInfo;
				MethodInfo setMethod = tempPropertyInfo.GetSetMethod();

				if (setMethod == null)
				{
					if (PropertyIsMappedByInvokingDelegate(tempPropertyInfo))
					{
						string message = String.Format(
							"A mapping was provided as an assignment for target property '{0}.{1}' but the target property has no public instance setter.",
							typeof(TTarget).Name,
							tempPropertyInfo.Name);

						errorMessages.Add(message);
					}
					else if (propertyValidationDelegate == null || !propertyValidationDelegate(tempPropertyInfo))
					{
						string message = String.Format(
							"A mapping was not provided for read-only target property '{0}.{1}' from source '{2}'.",
							typeof(TTarget).Name,
							tempPropertyInfo.Name,
							typeof(TSource).Name);

						errorMessages.Add(message);
					}
				}
				else if (setMethod.IsStatic)
				{
					if (PropertyIsMappedByInvokingDelegate(tempPropertyInfo))
					{
						string message = String.Format(
							"A mapping was provided as an assignment for target property '{0}.{1}' but the target property setter is static.",
							typeof(TTarget).Name,
							tempPropertyInfo.Name);

						errorMessages.Add(message);
					}
					else if (propertyValidationDelegate == null || !propertyValidationDelegate(tempPropertyInfo))
					{
						string message = String.Format(
							"A mapping was not provided for static read-only target property '{0}.{1}' from source '{2}'.",
							typeof(TTarget).Name,
							tempPropertyInfo.Name,
							typeof(TSource).Name);

						errorMessages.Add(message);
					}
				}
				else if (!PropertyIsMappedByInvokingDelegate(tempPropertyInfo) && (propertyValidationDelegate == null || !propertyValidationDelegate(tempPropertyInfo)))
				{
					string message = String.Format(
						"A mapping was not provided for target property '{0}.{1}' from source '{2}'.",
						typeof(TTarget).Name,
						tempPropertyInfo.Name,
						typeof(TSource).Name);

					errorMessages.Add(message);
				}
			}

			if (!errorMessages.Any())
			{
				return;
			}

			errorMessages.Insert(0, "Mapping configuration is invalid.");

			throw new Exception(String.Join(Environment.NewLine, errorMessages));
		}

		/// <summary>
		/// Sets a mapping for the specified member name.
		/// </summary>
		/// <param name="memberName">The name of a member of <typeparamref name="TTarget"/>.</param>
		/// <param name="mapping">A member mapping.</param>
		public void SetMapping(string memberName, MemberMapping<TSource> mapping)
		{
			_memberMappingsByMemberName[memberName] = mapping;
		}

		private bool PropertyIsMappedByInvokingDelegate(PropertyInfo propertyInfo)
		{
			return Mappings.Any(arg => arg.MemberName == propertyInfo.Name);
		}

		private class Mapping<TMember> : IMapping<TSource, TMember>
		{
			private readonly MappingConfigurationHelper<TSource, TTarget> _helper;
			private readonly string _memberName;

			public Mapping(string memberName, MappingConfigurationHelper<TSource, TTarget> helper)
			{
				_memberName = memberName;
				_helper = helper;
			}

			public string MemberName
			{
				get
				{
					return _memberName;
				}
			}

			public void From(Func<TSource, TMember> valueDelegate)
			{
				valueDelegate.ThrowIfNull("valueDelegate");

				_helper._memberMappingsByMemberName[_memberName] = new MemberMapping<TSource>(_memberName, source => valueDelegate(source));
			}
		}
	}
}