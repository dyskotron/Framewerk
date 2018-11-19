using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framewerk.Core
{
    public class InjectAttribute : Attribute
    {

    }

    public class InjectConstructorAttribute : Attribute
    {

    }

    //todo: introduce mechanism for injecting/reinjecting/deinjecting when mapping changes - instead of current Init() mechanism
    public interface IInjector
    {
        void InjectInto(object target);
        
        void MapSingleton<T>();
        void MapSingletonOf<TRequested, TValue>() where TValue : TRequested;
        
        void MapClass<TInterface, TClass>() where TClass : TInterface;
        void MapClass<T>();
        
        void MapValue<T>(T value);
        void MapValue(object value, Type type);
        
        void Unmap<T>();
        void Unmap(object obj);
        
        void Destroy();
    }

    public class Injector : IInjector
    {
        private Dictionary<Type, IInjection> _injections;

        public Injector()
        {
            _injections = new Dictionary<Type, IInjection>();
        }

        public void InjectInto(object target)
        {
            IReflected reflected = new Reflected(target.GetType());
            
            foreach (var field in reflected.InjectFields)
            {
                field.SetValue(target, GetInjectionValue(target, field.FieldType));
            }
            
            foreach (var property in reflected.InjectProperties)
            {   
                property.SetValue(target, GetInjectionValue(target, property.PropertyType), null);
            }
            
            //TODO: methods
            
            //TODO: Call PostInject Methods
        }

        //TODO: MOVE TO FACTORY
        //TODO: CHECK CYCLIC DEPENDENCY
        
        #region factory
        private object GetInjectionValue(object target, Type type)
        {
            if (!_injections.ContainsKey(type))
            {
                Debug.LogErrorFormat("<color=\"red\">Missing Injection rule for type {0} defined in {1} AvailableInjections: {2} </color>",type, target.GetType(), GetInjectionsString());
                return null;    
            }

            var injection = _injections[type];

            switch (injection.Type)
            {
                case InjectionType.Unique:
                    
                    //CREATE INSTANCE
                    object classInstance = CreateInstance(injection.Source as Type);
                    InjectInto(classInstance);
                    injection.Value = classInstance;
                    Debug.LogWarningFormat("<color=\"aqua\">{0} INITIALISING CLASS INJECTION: {1}</color>", "INJECTOR", type);

                    return classInstance;
                    
                    break;
                case InjectionType.Singleton:
                    
                    if (injection.Value == null)
                    {
                        //CREATE INSTANCE
                        object sinletonInstance = CreateInstance(injection.Source as Type);
                        InjectInto(sinletonInstance);
                        injection.Value = sinletonInstance;
                        Debug.LogWarningFormat("<color=\"aqua\">{0} INITIALISING SINGLETON INJECTION: {1}</color>", "INJECTOR", type);
                    }
                    
                    return _injections[type].Value;
                    
                    
                    break;
                case InjectionType.Value:
                    
                    if (injection.Value == null)
                    {
                        var value = injection.Source;
                        InjectInto(value);
                        injection.Value = value;
                        Debug.LogWarningFormat("<color=\"aqua\">{0} INITIALISING VALUE INJECTION: {1}</color>", "INJECTOR", type);
                    }
                    return _injections[type].Value;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return null;
        }

        private object CreateInstance(Type type)
        {
            IReflected reflected = new Reflected(type);
            var paramsReflected = reflected.Constructor.GetParameters();
            
            object instance;
            if (paramsReflected.Length == 0)
            {
                instance = Activator.CreateInstance(type);   
                Debug.LogWarningFormat("<color=\"red\">[{0}] CREATED INSTANCE OF: {1}</color>", "INJECTOR", type);
            }
            else
            {
                var injectParams = new object[paramsReflected.Length];
                for (var i = 0; i < paramsReflected.Length; i++)
                {
                    injectParams[i] = GetInjectionValue(null, paramsReflected[i].ParameterType);
                }

                instance = Activator.CreateInstance(type, injectParams);
                Debug.LogWarningFormat("<color=\"red\">[{0}] CREATED INSTANCE WITH PARAMS: {1}</color>", "INJECTOR", type);
            }   
            
            return instance;
        }
        
        #endregion
        
        public void MapSingleton<T>() 
        {
            if(_injections.ContainsKey(typeof(T)))
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapSingleton() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, typeof(T));

            _injections[typeof(T)] = new Injection(typeof(T), InjectionType.Singleton);
        }

        public void MapSingletonOf<TRequested, TValue>() where TValue : TRequested
        {
            if(_injections.ContainsKey(typeof(TRequested)))
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapSingletonOf() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, typeof(TRequested));

            _injections[typeof(TRequested)] = new Injection(typeof(TValue), InjectionType.Singleton);
        }

        public void MapClass<TRequested, TClass>() where TClass : TRequested
        {
            if(_injections.ContainsKey(typeof(TRequested)))
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapClass() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, typeof(TRequested));

            _injections[typeof(TRequested)] = new Injection(typeof(TRequested), InjectionType.Unique);
        }

        public void MapClass<T>()
        {
            MapClass<T, T>();
        }

        public void MapValue<T>(T value)
        {
            if(_injections.ContainsKey(typeof(T)))
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapValue() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, typeof(T));

            _injections[typeof(T)] = new Injection(value, InjectionType.Value);
        }

        public void MapValue(object value, Type type)
        {
            if(_injections.ContainsKey(type))
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapValue() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, value.GetType());

            _injections[type] = new Injection(value, InjectionType.Value);
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