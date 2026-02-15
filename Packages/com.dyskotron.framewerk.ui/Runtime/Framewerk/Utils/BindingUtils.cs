using System;
using System.Collections.Generic;
using strange.extensions.injector.api;

namespace Framewerk.Utils
{
    public static class BindingUtils
    {
        public static List<Type> GetBindTypes(object param, bool includeInterfaces = true, bool includeBaseClasses = false)
        {
            var types = new List<Type>();
            var concreteType = param.GetType();
            types.Add(concreteType);

            if (includeInterfaces)
                types.AddRange(concreteType.GetInterfaces());

            if (includeBaseClasses)
            {
                var baseType = concreteType.BaseType;
                while (baseType != null && baseType != typeof(object))
                {
                    types.Add(baseType);
                    baseType = baseType.BaseType;
                }
            }

            return types;
        }

        public static void Bind(IInjectionBinder binder, bool includeInterfaces, bool includeBaseClasses, params object[] bindparams)
        {
            if (bindparams == null) return;
            foreach (var param in bindparams)
                foreach (var type in GetBindTypes(param, includeInterfaces, includeBaseClasses))
                    binder.Bind(type).ToValue(param);
        }

        public static void Unbind(IInjectionBinder binder, bool includeInterfaces, bool includeBaseClasses, params object[] bindparams)
        {
            if (bindparams == null) return;
            foreach (var param in bindparams)
                foreach (var type in GetBindTypes(param, includeInterfaces, includeBaseClasses))
                    binder.Unbind(type);
        }

        // Explicit type overloads — pass through, no auto-detection
        public static void Bind(IInjectionBinder binder, params Tuple<object, Type>[] bindparamsWithType)
        {
            if (bindparamsWithType == null) return;
            foreach (var param in bindparamsWithType)
                binder.Bind(param.Item2).ToValue(param.Item1);
        }

        public static void Unbind(IInjectionBinder binder, params Tuple<object, Type>[] bindparamsWithType)
        {
            if (bindparamsWithType == null) return;
            foreach (var param in bindparamsWithType)
                binder.Unbind(param.Item2);
        }

        /// <summary>
        /// Binds an instance and supplies it to specific target types using SupplyTo.
        /// SupplyTo ensures the binding is only used when injecting into the specified target types.
        /// </summary>
        /// <param name="binder">The injection binder</param>
        /// <param name="instance">The instance to bind</param>
        /// <param name="targetTypes">The types that should receive this binding during injection</param>
        /// <param name="includeInterfaces">Whether to bind interfaces of the instance type</param>
        /// <param name="includeBaseClasses">Whether to bind base classes of the instance type</param>
        public static void SupplyToTypes(
            IInjectionBinder binder,
            object instance,
            IEnumerable<Type> targetTypes,
            bool includeInterfaces = true,
            bool includeBaseClasses = false)
        {
            if (instance == null || targetTypes == null) return;

            var bindTypes = GetBindTypes(instance, includeInterfaces, includeBaseClasses);

            foreach (var bindType in bindTypes)
            {
                var binding = binder.Bind(bindType).ToValue(instance);

                foreach (var targetType in targetTypes)
                {
                    binding.SupplyTo(targetType);
                }
            }
        }

        /// <summary>
        /// Removes supply bindings for an instance from specific target types, then unbinds the instance.
        /// This is the cleanup counterpart to SupplyToTypes.
        /// </summary>
        /// <param name="binder">The injection binder</param>
        /// <param name="instance">The instance to unbind</param>
        /// <param name="targetTypes">The types that were receiving this binding</param>
        /// <param name="includeInterfaces">Whether to unbind interfaces of the instance type</param>
        /// <param name="includeBaseClasses">Whether to unbind base classes of the instance type</param>
        public static void UnsupplyAndUnbind(
            IInjectionBinder binder,
            object instance,
            IEnumerable<Type> targetTypes,
            bool includeInterfaces = true,
            bool includeBaseClasses = false)
        {
            if (instance == null || targetTypes == null) return;

            var bindTypes = GetBindTypes(instance, includeInterfaces, includeBaseClasses);

            // First, remove all supply relationships
            foreach (var bindType in bindTypes)
            {
                foreach (var targetType in targetTypes)
                {
                    binder.Unsupply(bindType, targetType);
                }
            }

            // Then unbind the types
            foreach (var bindType in bindTypes)
            {
                binder.Unbind(bindType);
            }
        }
    }
}
