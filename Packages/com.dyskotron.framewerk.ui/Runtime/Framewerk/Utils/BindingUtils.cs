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
    }
}
