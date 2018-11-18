using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Framewerk.Core
{
    public interface IReflected
    {
        FieldInfo[] InjectFields { get;}    
        PropertyInfo[] InjectProperties { get;}    
    }
    
    public class Reflected : IReflected
    {
        public FieldInfo[] InjectFields { get; private set; }
        public PropertyInfo[] InjectProperties { get; private set; }

        public Reflected(object subject)
        {
            var type = subject.GetType();
            
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
            
            //==================
            
            var properties = subject.GetType().GetProperties(BindingFlags.Instance | 
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

        }
    }
}