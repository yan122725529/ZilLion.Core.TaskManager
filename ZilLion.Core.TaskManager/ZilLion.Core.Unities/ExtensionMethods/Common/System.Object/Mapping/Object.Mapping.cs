using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ZilLion.Core.Unities.UnitiesMethods;

public static partial class Extensions
{
    ///// <summary>
    /////     自动关联两个对象,复制相同属性名的属性
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <returns></returns>
    public static T AutoConvert<T>(this object @this, T terget)
    {
        try
        {
            var re = terget;
            var tergetprops = re.GetType().GetProperties();
            var sourceprops = @this.GetType().GetProperties();
            foreach (var tprops in tergetprops)
                if (tprops.CanWrite)
                {
                    var sourceprop = sourceprops.FirstOrDefault(x => x.Name == tprops.Name);


                    if (sourceprop != null)
                        tprops.SetValue(re, sourceprop.GetValue(@this, null), null);
                }

            return re;
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    /// <summary>
    ///     自动关联两个对象,复制相同属性名的属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T AutoConvert<T>(this object @this, List<Expression<Func<T, object>>> ignorePropertMap = null)
        where T : new()
    {
        try
        {
            var re = new T();
            var ignoreProper = new List<string>();
            if (ignorePropertMap != null)
                ignoreProper.AddRange(
                    ignorePropertMap.Select(ignore => (ReflectionUtil.GetProperty(ignore) as PropertyInfo).Name));
            var tergetprops =
                re.GetType()
                    .GetProperties().Where(x => ignoreProper.All(y => x.Name != y));
            var sourceprops = @this.GetType().GetProperties();
            foreach (var tprops in tergetprops)
                if (tprops.CanWrite)
                {
                    var sourceprop = sourceprops.FirstOrDefault(x => x.Name == tprops.Name);


                    if (sourceprop != null)
                        tprops.SetValue(re, TypeUtilities.SafelyConvert(sourceprop.GetValue(@this, null),
                            tprops.PropertyType,
                            null), null);
                }
            return re;
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    public static T AutoConvert<T, TSource>(this TSource @this) where T : new()
    {
        try
        {
            var re = new T();
            var tergetprops = re.GetType().GetProperties();
            var sourceprops = typeof(TSource).GetProperties();
            foreach (var tprops in tergetprops)
                if (tprops.CanWrite)
                {
                    var sourceprop = sourceprops.FirstOrDefault(x => x.Name == tprops.Name);


                    if (sourceprop != null)
                        tprops.SetValue(re, sourceprop.GetValue(@this, null), null);
                }
            return re;
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    /// <summary>
    ///     自动关联两个对象,复制相同属性名的属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static T AutoConvert<T, TSource>(this TSource @this, T terget)
    {
        try
        {
            var re = terget;
            var tergetprops = re.GetType().GetProperties();
            var sourceprops = typeof(TSource).GetProperties();
            foreach (var tprops in tergetprops)
                if (tprops.CanWrite)
                {
                    var sourceprop = sourceprops.FirstOrDefault(x => x.Name == tprops.Name);


                    if (sourceprop != null)
                        tprops.SetValue(re, sourceprop.GetValue(@this, null), null);
                }

            return re;
        }
        catch (Exception)
        {
            return default(T);
        }
    }


    public static T CopyTo<T, TSource>(this TSource @this, T terget)
    {
        return @this.AutoConvert(terget);
    }
}