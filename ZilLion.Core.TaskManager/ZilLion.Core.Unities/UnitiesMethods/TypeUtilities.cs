using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace ZilLion.Core.Unities.UnitiesMethods
{
    public static class TypeUtilities
    {
        /// <summary>
        ///     Get the string value defined within <see cref="DescriptionAttribute" /> from <paramref name="value" />.
        /// </summary>
        /// <param name="value">
        ///     The source object.
        /// </param>
        /// <returns>
        ///     Description string defined within <see cref="DescriptionAttribute" />.
        /// </returns>
        public static string GetDescription(object value)
        {
            Contract.Requires(value != null);
            try
            {
                var fi = value.GetType().GetMember(value.ToString());
                if (fi.Length == 0)
                    return null;
                var attributes =
                    (DescriptionAttribute[]) fi[0].GetCustomAttributes(typeof (DescriptionAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Description : null;
            }
            catch
            {
                return null;
            }
        }

        public static T SafelyConvert<T>(object obj)
        {
            return SafelyConvert(obj, default(T));
        }

        /// <summary>
        /// </summary>
        /// <param name="obj">
        ///     The obj.
        /// </param>
        /// <param name="fallbackValue">
        ///     The fallback value.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public static T SafelyConvert<T>(object obj, T fallbackValue)
        {
            return (T) SafelyConvert(obj, typeof (T), fallbackValue);
        }

        private static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>));
        }




        /// <summary>
        ///     The safely convert.
        /// </summary>
        /// <param name="obj">
        ///     The obj.
        /// </param>
        /// <returns>
        /// </returns>
        public static object SafelyConvert(object obj, Type type, object fallbackValue)
        {
            if (obj == null) return fallbackValue;


            try
            {
                if (type.IsEnum)
                {
                    return Enum.Parse(type, obj.ToString());
                }

                if (obj is string && type == typeof (DateTime))
                {
                    var str = (string) obj;
                    return Convert.ChangeType(DateTime.Parse(str), type);
                }

                if (type == typeof (string))
                {
                    return Convert.ChangeType(obj.ToString(), type);
                }

                if (IsNullableType(type) )
                {
                    var converter = new NullableConverter(type);
                    return converter.ConvertFromString(obj.ToString());
                }

                var ret = Convert.ChangeType(obj, type);
                if (ret == null && type == typeof (string))
                {
                    return Convert.ChangeType(string.Empty, type);
                }


             

                return ret;
            }
            catch
            {
                return fallbackValue;
            }
        }

        public static object GetDefaultValue(object value)
        {
            try
            {
                var fi = value.GetType().GetMember(value.ToString());
                if (fi.Length == 0)
                    return null;
                var attributes =
                    (DefaultValueAttribute[]) fi[0].GetCustomAttributes(typeof (DefaultValueAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Value : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     The get all types implements interface from current app domain.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        /// </returns>
        public static Type[] GetAllTypesImplementsInterfaceFromCurrentAppDomain(Type type)
        {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<Type[]>() != null);
            return (from ass in AppDomain.CurrentDomain.GetAssemblies()
                from s in ass.GetTypes()
                where s.GetInterfaces().Any(x => x.IsAssignableFrom(type))
                select s).ToArray();
        }

        /// <summary>
        ///     Get property value by dot separated property path
        /// </summary>
        /// <param name="src"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object src, string path)
        {
            Contract.Requires(src != null && path != null);
            foreach (var s in path.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (src == null) return null;
                var type = src.GetType();
                var prop = type.GetProperty(s);
                if (prop == null) return null;
                //if (prop.GetIndexParameters().Length > 0)
                //TODO: add indexed parameter support
                src = prop.GetValue(src, null);
            }
            return src;
        }

        /// <summary>
        ///     This is used to clone the object.
        ///     Override the method to provide a more efficient clone.
        ///     The default implementation simply reflects across
        ///     the object copying every field.
        /// </summary>
        /// <returns>Clone of current object</returns>
        public static Dictionary<string, object> GetPropertyValueSnapshot(object src)
        {
            Contract.Requires(src != null);
            Contract.Ensures(Contract.Result<Dictionary<string, object>>() != null);
            return src.GetType().GetProperties(BindingFlags.Public |
                                               BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(pi => pi.CanRead && pi.GetIndexParameters().Length == 0)
                .Select(pi => new {Key = pi.Name, Value = pi.GetValue(src, null)})
                .ToDictionary(k => k.Key, k => k.Value);
        }

        /// <summary>
        ///     This restores the state of the current object from the passed clone object.
        /// </summary>
        /// <param name="fieldValues">Object to restore state from</param>
        public static void RestorePropertyValueSnapshot(object src, Dictionary<string, object> fieldValues)
        {
            Contract.Requires(src != null && fieldValues != null);

            foreach (var pi in src.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(pi => pi.CanWrite && pi.GetIndexParameters().Length == 0))
            {
                object value;
                if (fieldValues.TryGetValue(pi.Name, out value))
                    pi.SetValue(src, value, null);
                else
                {
                    Debug.WriteLine("Failed to restore property " +
                                    pi.Name + " from cloned values, property not found in Dictionary.");
                }
            }
        }
    }
}