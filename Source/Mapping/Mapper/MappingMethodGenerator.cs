using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Junior.Common;
using Junior.Mapping.Common;

namespace Junior.Mapping.Mapper
{
	/// <summary>
	/// Generates a mapping method for <see cref="MemberMappingAction{TTarget,TSource}"/> instances and <see cref="MemberMapping{TSource}"/> instances contained in a mapper configuration.
	/// </summary>
	public class MappingMethodGenerator
	{
		/// <summary>
		/// Generates a delegate that invokes the mappings and actions contained in the specified mapper configuration.
		/// </summary>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		/// <param name="configuration">A mapper configuration.</param>
		/// <returns>A delegate that invokes the mappings and actions contained in <paramref name="configuration"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
		public Action<TSource, TTarget> GenerateMappingMethod<TSource, TTarget>(MapperConfiguration<TSource, TTarget> configuration)
		{
			configuration.ThrowIfNull("configuration");

			configuration.Validate();

			Type funcArgumentType = typeof(Func<TSource, object>[]);
			Type actionArgumentType = typeof(Action<TTarget, TSource>[]);
			Type sourceType = typeof(TSource);
			Type targetType = typeof(TTarget);
			Type[] methodArguments =
				{
					funcArgumentType,
					actionArgumentType,
					sourceType,
					targetType
				};
			string methodName = String.Format("MappingMethod_{0}_to_{1}_{2:N}", sourceType.FullName, targetType.FullName, Guid.NewGuid());
			var mappingMethod = new DynamicMethod(methodName, typeof(void), methodArguments, targetType.Module);
			ILGenerator ilGenerator = mappingMethod.GetILGenerator();
			Action<TTarget, TSource>[] mapActions = configuration.Actions
				.Where(arg => arg.MapDelegate != null)
				.Select(arg => arg.MapDelegate)
				.ToArray();

			EmitMemberMappings(configuration.Mappings, targetType, ilGenerator);
			EmitMemberMapActions(mapActions, ilGenerator);

			ilGenerator.Emit(OpCodes.Ret);

			var methodDelegate = (MappingMethodDelegate<TSource, TTarget>)mappingMethod.CreateDelegate(typeof(MappingMethodDelegate<TSource, TTarget>));
			Func<TSource, object>[] mapFuncs = configuration.Mappings
				.Where(arg => arg.ValueDelegate != null)
				.Select(arg => arg.ValueDelegate)
				.ToArray();

			return (source, target) => methodDelegate(mapFuncs, mapActions, source, target);
		}

		private static void EmitMemberMappings<TSource>(IEnumerable<MemberMapping<TSource>> memberMappings, IReflect targetType, ILGenerator ilGenerator)
		{
			int memberMappingIndex = 0;

			foreach (var memberMapFunc in memberMappings)
			{
				PropertyInfo propertyInfo = targetType.GetProperty(memberMapFunc.MemberName, BindingFlags.Public | BindingFlags.Instance);
				MethodInfo propertySetMethod = propertyInfo.GetSetMethod(false);

				if (propertySetMethod == null)
				{
					continue;
				}

				ilGenerator.Emit(OpCodes.Ldarg_3); // Push target instance onto the stack
				ilGenerator.Emit(OpCodes.Ldarg_0); // Push delegate array onto stack
				ilGenerator.Emit(OpCodes.Ldc_I4, memberMappingIndex++);
				ilGenerator.Emit(OpCodes.Ldelem_Ref); // Get the delegate from the array
				ilGenerator.Emit(OpCodes.Ldarg_2); // Push the source instance onto the stack
				ilGenerator.Emit(OpCodes.Callvirt, typeof(Func<TSource, object>).GetMethod("Invoke")); // Invoke the delegate, passing (source)
				ilGenerator.Emit(propertyInfo.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, propertyInfo.PropertyType); // Cast or unbox the delegate result
				ilGenerator.Emit(OpCodes.Callvirt, propertySetMethod); // Set the property on the current target instance
			}
		}

		private static void EmitMemberMapActions<TSource, TTarget>(IEnumerable<Action<TTarget, TSource>> memberMappingActionDelegates, ILGenerator ilGenerator)
		{
			int count = memberMappingActionDelegates.Count();

			for (int mapDelegateIndex = 0; mapDelegateIndex < count; mapDelegateIndex++)
			{
				ilGenerator.Emit(OpCodes.Ldarg_1); // Push delegate array onto the stack
				ilGenerator.Emit(OpCodes.Ldc_I4, mapDelegateIndex);
				ilGenerator.Emit(OpCodes.Ldelem_Ref); // Get the delegate from the array
				ilGenerator.Emit(OpCodes.Ldarg_3); // Push target instance onto the stack
				ilGenerator.Emit(OpCodes.Ldarg_2); // Push source instance onto the stack
				ilGenerator.Emit(OpCodes.Callvirt, typeof(Action<TTarget, TSource>).GetMethod("Invoke")); // Invoke the delegate, passing (target, source)
			}
		}

		private delegate void MappingMethodDelegate<TSource, TTarget>(Func<TSource, object>[] memberMappings, Action<TTarget, TSource>[] memberMappingActionDelegates, TSource source, TTarget target);
	}
}