using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

using Junior.Common;
using Junior.Map.Common;

namespace Junior.Map.Adapter
{
	/// <summary>
	/// Generates <see cref="IAdapterFactory{TSource,TTargetInterface}"/> instances given a set of mappings.
	/// </summary>
	public class AdapterFactoryGenerator
	{
		/// <summary>
		/// The singleton instance of <see cref="AdapterFactoryGenerator"/>.
		/// </summary>
		public static readonly AdapterFactoryGenerator Instance = new AdapterFactoryGenerator();

		private readonly AssemblyBuilder _assembly;
		private readonly ModuleBuilder _moduleBuilder;

		private AdapterFactoryGenerator()
		{
			_assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("AdapterClassGeneratorDynamicAssembly"), AssemblyBuilderAccess.Run);
			_moduleBuilder = _assembly.DefineDynamicModule("GeneratedAdapterClasses");
		}

		/// <summary>
		/// Generates an adapter factory given a set of mappings and returns a factory that can be used to create an adapter factory.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTargetInterface">The target interface type.</typeparam>
		/// <param name="memberMappings">Mappings.</param>
		/// <returns>An <see cref="IAdapterFactory{TSource,TTargetInterface}"/> instance that can be used to create the adapter factory.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="memberMappings"/> is null.</exception>
		public IAdapterFactory<TSource, TTargetInterface> Generate<TSource, TTargetInterface>(IEnumerable<MemberMapping<TSource>> memberMappings)
			where TSource : class
			where TTargetInterface : class
		{
			memberMappings.ThrowIfNull("memberMappings");

			memberMappings = memberMappings.ToArray();

			ValidateMapping<TSource, TTargetInterface>(memberMappings);
			ValidateTargetType<TTargetInterface>();

			Type sourceType = typeof(TSource);
			Type targetInterfaceType = typeof(TTargetInterface);
			Dictionary<string, Func<TSource, object>> mappingDictionary = GetMappingDictionary(memberMappings);
			string adapterTypeName = GetAdapterTypeName<TSource, TTargetInterface>();
			TypeBuilder typeBuilder = _moduleBuilder.DefineType(
				adapterTypeName,
				TypeAttributes.Class | TypeAttributes.Public,
				typeof(object),
				new[] { targetInterfaceType });
			FieldBuilder sourceInstanceField = typeBuilder.DefineField("_" + sourceType.Name, sourceType, FieldAttributes.Private);
			FieldAndValueDelegate<TSource>[] fieldsAndValueDelegates = targetInterfaceType
				.GetAllPublicInstanceProperties()
				.Select(arg => AddPropertyWithBackingValueDelegateField(typeBuilder, arg, sourceInstanceField, mappingDictionary))
				.ToArray();
			Type[] constructorArgumentTypes = sourceType
				.ToEnumerable()
				.Concat(fieldsAndValueDelegates.Select(arg => arg.Field.FieldType))
				.ToArray();

			AddConstructor(sourceInstanceField, typeBuilder, constructorArgumentTypes, fieldsAndValueDelegates);

			Type type = typeBuilder.CreateType();
			ConstructorInfo constructor = type.GetConstructor(constructorArgumentTypes);

			// Array copying minimizes time spent preparing the constructor arguments each time the adapter is instantiated
			object[] constructorDelegateArgumentsAsObjectArray = fieldsAndValueDelegates
				.Select(arg => arg.ValueDelegate)
				.Cast<object>()
				.ToArray();
			Func<object[], TTargetInterface> adapterConstructorDelegate = GetActivator<TTargetInterface>(constructor);
			int constructorArgumentCount = constructorArgumentTypes.Length;

			// Return the factory with a delegate that calls the constructor expression more efficiently
			return new AdapterFactory<TSource, TTargetInterface>(
				source =>
					{
						var arguments = new object[constructorArgumentCount];

						arguments[0] = source;
						constructorDelegateArgumentsAsObjectArray.CopyTo(arguments, 1);

						return adapterConstructorDelegate(arguments);
					});
		}

		/// <summary>
		/// Validates that mappings exist for every public instance property of <typeparamref name="TTargetInterface"/>.
		/// </summary>
		/// <param name="memberMappings">Mappings.</param>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTargetInterface">The target interface type.</typeparam>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="memberMappings"/> is null.</exception>
		/// <exception cref="Exception">Thrown when a property of <typeparamref name="TTargetInterface"/> does not have an associated mapping.</exception>
		public void ValidateMapping<TSource, TTargetInterface>(IEnumerable<MemberMapping<TSource>> memberMappings)
		{
			memberMappings.ThrowIfNull("memberMappings");

			List<string> errorMessages = typeof(TTargetInterface)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(arg => memberMappings.All(mapping => mapping.MemberName != arg.Name))
				.Select(arg => String.Format("Mapping for '{0}.{1}' from '{2}' not provided.", typeof(TTargetInterface).Name, arg.Name, typeof(TSource).Name))
				.ToList();

			if (!errorMessages.Any())
			{
				return;
			}

			errorMessages.Insert(0, "Mapping configuration is invalid.");

			throw new Exception(String.Join(Environment.NewLine, errorMessages));
		}

		private static void ValidateTargetType<TTargetInterface>()
		{
			Type targetInterfaceType = typeof(TTargetInterface);

			if (targetInterfaceType.GetMethods().Any(arg => !arg.IsHideBySig))
			{
				throw new InvalidGenericTypeArgumentException("Cannot generate an adapter for an interface that contains methods.", "TTargetInterface");
			}
		}

		private static Dictionary<string, Func<TSource, object>> GetMappingDictionary<TSource>(IEnumerable<MemberMapping<TSource>> mappings)
		{
			return mappings.ToDictionary(mapping => mapping.MemberName, mapping => mapping.ValueDelegate);
		}

		private static string GetAdapterTypeName<TSource, TTargetInterface>()
		{
			Type sourceType = typeof(TSource);
			Type targetInterfaceType = typeof(TTargetInterface);

			return String.Format("Adapter_{0}_to_{1}_{2:N}", sourceType.FullName, targetInterfaceType.FullName, Guid.NewGuid());
		}

		private static Func<object[], T> GetActivator<T>(ConstructorInfo constructor)
		{
			ParameterInfo[] parameterInfos = constructor.GetParameters();
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "arguments");
			var argumentsExpression = new Expression[parameterInfos.Length];

			for (int i = 0; i < parameterInfos.Length; i++)
			{
				Expression index = Expression.Constant(i);
				Type parameterType = parameterInfos[i].ParameterType;
				Expression parameterAccessorExpression = Expression.ArrayIndex(parameterExpression, index);
				Expression parameterCastExpression = Expression.Convert(parameterAccessorExpression, parameterType);

				argumentsExpression[i] = parameterCastExpression;
			}

			NewExpression newExpression = Expression.New(constructor, argumentsExpression);
			LambdaExpression lambdaExpression = Expression.Lambda<Func<object[], T>>(newExpression, parameterExpression);

			return (Func<object[], T>)lambdaExpression.Compile();
		}

		private static void AddConstructor<TSource>(
			FieldInfo sourceInstanceField,
			TypeBuilder typeBuilder,
			Type[] constructorArgumentTypes,
			IEnumerable<FieldAndValueDelegate<TSource>> fieldsAndValueDelegates)
		{
			ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorArgumentTypes);
			ILGenerator generator = constructorBuilder.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Call, typeBuilder.BaseType.GetConstructor(Type.EmptyTypes));
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Stfld, sourceInstanceField);

			int argumentIndex = 2;

			foreach (FieldAndValueDelegate<TSource> fieldAndValueDelegate in fieldsAndValueDelegates)
			{
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Ldarg, argumentIndex);
				generator.Emit(OpCodes.Stfld, fieldAndValueDelegate.Field);
				argumentIndex++;
			}

			generator.Emit(OpCodes.Ret);
		}

		private static FieldAndValueDelegate<TSource> AddPropertyWithBackingValueDelegateField<TSource>(
			TypeBuilder typeBuilder,
			PropertyInfo propertyInfo,
			FieldInfo sourceInstanceField,
			IDictionary<string, Func<TSource, object>> mappingDictionary)
		{
			Func<TSource, object> valueDelegate = mappingDictionary[propertyInfo.Name];
			FieldBuilder valueDelegateField = typeBuilder.DefineField("_" + propertyInfo.Name, valueDelegate.GetType(), FieldAttributes.Private | FieldAttributes.InitOnly);

			EmitProperty<TSource>(typeBuilder, sourceInstanceField, valueDelegateField, propertyInfo);

			return new FieldAndValueDelegate<TSource>(valueDelegateField, valueDelegate);
		}

		private static void EmitProperty<TSource>(TypeBuilder typeBuilder, FieldInfo sourceInstanceField, FieldInfo mappingDelegateField, PropertyInfo propertyInfo)
		{
			PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
				propertyInfo.Name,
				propertyInfo.Attributes,
				propertyInfo.PropertyType,
				new[] { propertyInfo.PropertyType });

			if (propertyInfo.CanWrite)
			{
				throw new ArgumentException("Generated adapters cannot have settable properties.", "propertyInfo");
			}

			MethodInfo methodInfo = propertyInfo.GetGetMethod();
			MethodBuilder getterBuilder = typeBuilder.DefineMethod(
				"get_" + propertyInfo.Name,
				methodInfo.Attributes ^ MethodAttributes.Abstract,
				methodInfo.CallingConvention,
				methodInfo.ReturnType,
				Type.EmptyTypes);
			ILGenerator getterGenerator = getterBuilder.GetILGenerator();

			getterGenerator.Emit(OpCodes.Ldarg_0);
			getterGenerator.Emit(OpCodes.Ldfld, mappingDelegateField);
			getterGenerator.Emit(OpCodes.Ldarg_0);
			getterGenerator.Emit(OpCodes.Ldfld, sourceInstanceField);
			getterGenerator.Emit(OpCodes.Callvirt, typeof(Func<TSource, object>).GetMethod("Invoke"));
			getterGenerator.Emit(propertyInfo.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, propertyInfo.PropertyType);
			getterGenerator.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(getterBuilder);
		}

		private class AdapterFactory<TSource, TTargetInterface> : IAdapterFactory<TSource, TTargetInterface>
			where TSource : class
			where TTargetInterface : class
		{
			private readonly Func<TSource, TTargetInterface> createDelegate;

			public AdapterFactory(Func<TSource, TTargetInterface> createDelegate)
			{
				this.createDelegate = createDelegate;
			}

			public TTargetInterface Create(TSource source)
			{
				return createDelegate(source);
			}
		}

		private struct FieldAndValueDelegate<TSource>
		{
			private readonly FieldInfo _field;
			private readonly Func<TSource, object> _valueDelegate;

			public FieldAndValueDelegate(FieldInfo field, Func<TSource, object> valueDelegate)
			{
				_field = field;
				_valueDelegate = valueDelegate;
			}

			public FieldInfo Field
			{
				get
				{
					return _field;
				}
			}

			public Func<TSource, object> ValueDelegate
			{
				get
				{
					return _valueDelegate;
				}
			}
		}
	}
}