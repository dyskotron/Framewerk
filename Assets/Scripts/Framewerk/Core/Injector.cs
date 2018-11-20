using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framewerk.Core
{
    public class InjectAttribute : Attribute
    {
        public object Name { get; private set; }

        public InjectAttribute(object name = null)
        {
            Name = name;
        }
    }
    
    

    [AttributeUsage(AttributeTargets.Constructor, 
                    AllowMultiple = false,
                    Inherited = true)]
    public class InjectConstructorAttribute : Attribute
    {

    }

    public interface IInjector
    {
        void InjectInto(object target);
        void MapSingleton<T>(object name = null);
        void MapSingletonOf<TRequested, TValue>(object name = null) where TValue : TRequested;
        void MapClass<TRequested, TClass>(object name = null) where TClass : TRequested;
        void MapClass<T>(object name = null);
        void MapValue<T>(T value, object name = null);
        void MapValue(Type type, object value,  object name = null);
        void Unmap<T>();
        void Unmap(object obj);
        void Destroy();
    }

    public class Injector : IInjector
    {
        private Dictionary<Type, IInjection> _injections;
        private Dictionary<object, Dictionary<Type, IInjection>> _namedInjections;

        public Injector()
        {
            _injections = new Dictionary<Type, IInjection>();
            _namedInjections = new Dictionary<object, Dictionary<Type, IInjection>>();
        }

        public void InjectInto(object target)
        {
            IReflected reflected = new Reflected(target.GetType());
            
            for(var i = 0; i < reflected.InjectFields.Length;i++)
            {
                var field = reflected.InjectFields[i];
                var name = reflected.FieldNames[i];
                field.SetValue(target, GetInjectionValue(target, field.FieldType, name));
            }
            
            foreach (var property in reflected.InjectProperties)
            {   
                property.SetValue(target, GetInjectionValue(target, property.PropertyType, null), null);
            }
            
            //TODO: methods
            
            //TODO: Call PostInject Methods
        }

        //TODO: MOVE TO FACTORY
        //TODO: CHECK CYCLIC DEPENDENCY
        
        #region factory
        private object GetInjectionValue(object target, Type type, object name = null)
        {
            var injection = GetInjection(type, name);
            
            if (injection == null)
            {
                Debug.LogErrorFormat("<color=\"red\">Missing Injection rule for type {0} defined in {1} AvailableInjections: {2} </color>",type, target.GetType(), GetInjectionsString());
                return null;    
            }

            switch (injection.InjectionType)
            {
                case InjectionType.Unique:
                    
                    //CREATE INSTANCE
                    object classInstance = CreateInstance(injection.Source as Type);
                    InjectInto(classInstance);
                    injection.Value = classInstance;
                    //Debug.LogWarningFormat("<color=\"aqua\">{0} INITIALISING CLASS INJECTION: {1}</color>", "INJECTOR", type);

                    return classInstance;
                    
                    break;
                case InjectionType.Singleton:
                    
                    if (injection.Value == null)
                    {
                        //CREATE INSTANCE
                        object sinletonInstance = CreateInstance(injection.Source as Type);
                        InjectInto(sinletonInstance);
                        injection.Value = sinletonInstance;
                        //Debug.LogWarningFormat("<color=\"aqua\">{0} INITIALISING SINGLETON INJECTION: {1}</color>", "INJECTOR", type);
                    }
                    
                    return injection.Value;
                    
                    break;
                case InjectionType.Value:
                    
                    if (injection.Value == null)
                    {
                        var value = injection.Source;
                        InjectInto(value);
                        injection.Value = value;
                        //Debug.LogWarningFormat("<color=\"aqua\">{0} INITIALISING VALUE INJECTION: {1}</color>", "INJECTOR", type);
                    }
                    
                    return injection.Value;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return null;
        }

        private IInjection GetInjection(Type type, object name = null)
        {
            if (name == null && _injections.ContainsKey(type))
                return _injections[type];

            if (name != null && _namedInjections.ContainsKey(name) && _namedInjections[name].ContainsKey(type))
                return _namedInjections[name][type];
            
            return null;
        }
        
        private void SetInjection(Type type, IInjection injection, object name = null)
        {
            //Debug.LogWarningFormat("<color=\"aqua\">====================> SET INJECTION <==================== \n " + 
            //                       "type: {0} injection: {1}name: {2}</color>", type, injection, name);
            
            
            if (name == null)
            {
                _injections[type] = injection;
            }
            else
            {
                if(!_namedInjections.ContainsKey(name))
                    _namedInjections[name] = new Dictionary<Type, IInjection>();
                _namedInjections[name][type] = injection;
            }
        }
        
        //todo set injection

        private object CreateInstance(Type type)
        {
            IReflected reflected = new Reflected(type);
            var paramsReflected = reflected.Constructor.GetParameters();
            
            object instance;
            if (paramsReflected.Length == 0)
            {
                instance = Activator.CreateInstance(type);   
                //Debug.LogWarningFormat("<color=\"red\">[{0}] CREATED INSTANCE OF: {1}</color>", "INJECTOR", type);
            }
            else
            {
                var injectParams = new object[paramsReflected.Length];
                for (var i = 0; i < paramsReflected.Length; i++)
                {
                    injectParams[i] = GetInjectionValue(null, paramsReflected[i].ParameterType);
                }

                instance = Activator.CreateInstance(type, injectParams);
                //Debug.LogWarningFormat("<color=\"red\">[{0}] CREATED INSTANCE WITH PARAMS: {1}</color>", "INJECTOR", type);
            }   
            
            return instance;
        }
        
        #endregion
        
        public void MapSingleton<T>(object name = null) 
        {
            if(GetInjection(typeof(T), name) != null)
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapSingleton() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, typeof(T));

            SetInjection(typeof(T), new Injection(typeof(T), InjectionType.Singleton), name);
        }

        public void MapSingletonOf<TRequested, TValue>(object name = null) where TValue : TRequested
        {
            if(GetInjection(typeof(TRequested), name) != null)
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapSingletonOf() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, typeof(TRequested));

            SetInjection(typeof(TRequested), new Injection(typeof(TValue), InjectionType.Singleton), name);
        }

        public void MapClass<TRequested, TClass>(object name = null) where TClass : TRequested
        {
            if(GetInjection(typeof(TRequested), name) != null)
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapClass() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, typeof(TRequested));

            SetInjection(typeof(TRequested), new Injection(typeof(TClass), InjectionType.Unique), name);
        }

        public void MapClass<T>(object name = null)
        {
            MapClass<T, T>(name);
        }

        public void MapValue<T>(T value, object name = null)
        {
            if(GetInjection(typeof(T), name) != null)
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapValue() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, typeof(T));

            SetInjection(typeof(T), new Injection(value, InjectionType.Value), name);
        }

        public void MapValue(Type type, object value,  object name = null)
        {
            if(GetInjection(type, name) != null)
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapValue() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, value.GetType());

            SetInjection(type, new Injection(value, InjectionType.Value), name);
        }

        public void Unmap<T>()
        {
            if (_injections.ContainsKey(typeof(T)))
                _injections.Remove(typeof(T));
            else
                Debug.LogWarningFormat("<color=\"aqua\">{0}.Unmap() : There is no Mapping for {1} </color>", this, typeof(T));

        }

        public void Unmap(object obj)
        {
            if (_injections.ContainsKey(obj.GetType()))
                _injections.Remove(obj.GetType());
            else
                Debug.LogWarningFormat("<color=\"aqua\">{0}.Unmap() : There is no Mapping for {1} </color>", this, obj.GetType());

        }

        public void Destroy()
        {
            _injections.Clear();
        }

        private string GetInjectionsString()
        {
            var injString = "";
            foreach (var injection in _injections)
            {
                injString += injection.Key + ", ";
            }

            return injString;
        }

    }
}