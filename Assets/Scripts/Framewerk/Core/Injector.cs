using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framewerk.Core
{
    public class InjectAttribute : Attribute
    {

    }

    //todo: introduce mechanism for injecting/reinjecting/deinjecting when mapping changes - instead of current Init() mechanism
    public interface IInjector
    {
        void InjectInto(object target);
        void MapSingleton<T>() where T : new();
        void MapSingletonOf<TRequested, TClass>() where TClass : TRequested, new();
        //TODO: create unique instance (already injected with everything needed)
        //void MapClass<TInterface, TClass>() where TClass : TInterface, new();
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
            IReflected reflected = new Reflected(target);
            
            foreach (var field in reflected.InjectFields)
            {
                field.SetValue(target, GetInjectionValue(target, field.FieldType));
            }
            
            foreach (var property in reflected.InjectProperties)
            {   
                property.SetValue(target, GetInjectionValue(target, property.PropertyType), null);
            }
        }

        //TODO: MOVE TO FACTORY
        //TODO: CHECK CYCLIC DEPENDENCY
        private object GetInjectionValue(object target, Type type)
        {
            if (!_injections.ContainsKey(type))
            {
                Debug.LogErrorFormat("<color=\"red\">Missing Injection rule for type {0} defined in {1} AvailableInjections: {2} </color>",type, target.GetType(), GetInjectionsString());
                return null;    
            }
            
            //Value Mapping
            if (_injections[type].Value == null)
            {
                var value = _injections[type].Source;
                InjectInto(value);
                _injections[type].Value = value;
                Debug.LogWarningFormat("<color=\"aqua\">{0} INITIALISING VALUE INJECTION: {1}</color>", "INJECTOR", type);
            }
            return _injections[type].Value;
            
            //Singleton Mapping (single instance for all)
            
            //New instance mapping(new instance for every injection)
        }

        public void MapSingleton<T>() where T : new()
        {
            //TODO: implement singleton after constructing is done
            MapValue(new T());
        }

        public void MapSingletonOf<TRequested, TClass>() where TClass : TRequested, new()
        {
            MapValue<TRequested>(new TClass());
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