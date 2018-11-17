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
        void InjectInto(object subject);
        void Init();
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
        private bool _inited;

        public Injector()
        {
            _injections = new Dictionary<Type, IInjection>();
        }

        public void InjectInto(object subject)
        {
            IReflected reflected = new Reflected(subject);
            
            foreach (var field in reflected.InjectFields)
            {
                if (_injections.ContainsKey(field.FieldType))
                {
                    var value = _injections[field.FieldType].Value;
                    field.SetValue(subject, value);
                }
                else
                {
                    Debug.LogErrorFormat("<color=\"red\">Missing Injection rule for type {0} defined in {1} AvailableInjections: {2} </color>",field.FieldType, subject.GetType(), GetInjectionsString());
                }    
            }
            
            
            foreach (var property in reflected.InjectProperties)
            {   
                if (_injections.ContainsKey(property.PropertyType))
                {
                    var value = _injections[property.PropertyType].Value;
                    property.SetValue(subject, value, null);
                }
                else
                {
                    Debug.LogErrorFormat("<color=\"red\">Missing Injection rule for type {0} defined in {1} AvailableInjections: {2} </color>",property.PropertyType, subject.GetType(), GetInjectionsString());
                }
            }
        }

        public void Init()
        {
            _inited = true;
            foreach (var injection in _injections)
            {
                InjectInto(injection.Value.Value);
            }
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
            if(_inited)
                InjectInto(value);
        }

        public void MapValue(object value, Type type)
        {
            if(_injections.ContainsKey(type))
                Debug.LogWarningFormat("<color=\"aqua\">{0}.MapValue() : Mapping for {1} already defined, you should unmap first if you want to change the mapping</color>", this, value.GetType());

            _injections[type] = new Injection(value, InjectionType.Value);
            if(_inited)
                InjectInto(value);
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