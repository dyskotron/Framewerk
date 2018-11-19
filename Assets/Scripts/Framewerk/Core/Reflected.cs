using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Framewerk.Core
{
    public interface IReflected
    {
        ConstructorInfo Constructor { get;}    
        FieldInfo[] InjectFields { get;}    
        PropertyInfo[] InjectProperties { get;}    
    }
    
    public class Reflected : IReflected
    {
        public ConstructorInfo Constructor { get; private set; }
        public FieldInfo[] InjectFields { get; private set; }
        public PropertyInfo[] InjectProperties { get; private set; }

        public Reflected(Type type)
        {
            //Fields
            //==================
            
            var fields = type.GetFields(BindingFlags.Instance | 
                                        BindingFlags.Public | 
                                        BindingFlags.NonPublic | 
                                        BindingFlags.FlattenHierarchy);
            
            Dictionary<string, FieldInfo> fieldInfos = new Dictionary<string, FieldInfo>();
            
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes(typeof(InjectAttribute), false).Length > 0)
                {
                    fieldInfos[field.Name] = field;
                    Debug.LogWarningFormat("<color=\"aqua\">{0} WANNA INJECT ==> {1}  -  ({2})</color>", type ,field.Name, field.FieldType );
                }
            }

            InjectFields = fieldInfos.Values.ToArray();
            
            //Properties
            //==================
            
            var properties = type.GetProperties(BindingFlags.Instance | 
                                                             BindingFlags.Public | 
                                                             BindingFlags.NonPublic);
            
            Dictionary<string, PropertyInfo> propertyInfos = new Dictionary<string, PropertyInfo>();
            
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(typeof(InjectAttribute), false).Length > 0)
                {
                    propertyInfos[property.Name] = property;
                    Debug.LogWarningFormat("<color=\"aqua\">WANNA INJECT ==> {0}  -  ({1})</color>", property.Name, property.PropertyType );
                }
            }

            InjectProperties = propertyInfos.Values.ToArray();
            
            //Constructors
            //==================
            
            var constructorInfos = type.GetConstructors(BindingFlags.FlattenHierarchy | 
                                                        BindingFlags.Public | 
                                                        BindingFlags.Instance |
                                                        BindingFlags.InvokeMethod);
            
            ConstructorInfo constructor;
            if(constructorInfos.Length == 1)
            {
                Constructor = constructorInfos[0];
            }

            var minParameters = int.MaxValue;

            foreach (var constructorInfo in constructorInfos)
            {
                if (constructorInfo.GetCustomAttributes(typeof(InjectConstructorAttribute), false).Length > 0)
                {
                    constructor = constructorInfo;
                    break;
                }
                var parameters = constructorInfo.GetParameters();
                if (parameters.Length < minParameters)
                {
                    minParameters = parameters.Length;
                    constructor = constructorInfo;
                }
            }
            
            //Methods PostInject + SettingParams
            //==================
        }
    }
}