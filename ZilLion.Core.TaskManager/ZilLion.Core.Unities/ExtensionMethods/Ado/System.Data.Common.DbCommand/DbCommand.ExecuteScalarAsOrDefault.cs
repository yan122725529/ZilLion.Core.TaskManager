





using System;
using System.Data.Common;

public static partial class Extensions
{
    /// <summary>
    ///     A DbCommand extension method that executes the scalar as or default operation.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A T.</returns>
    public static T ExecuteScalarAsOrDefault<T>(this DbCommand @this)
    {
        try
        {
            return (T) @this.ExecuteScalar();
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    /// <summary>
    ///     A DbCommand extension method that executes the scalar as or default operation.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>A T.</returns>
    public static T ExecuteScalarAsOrDefault<T>(this DbCommand @this, T defaultValue)
    {
        try
        {
            return (T) @this.ExecuteScalar();
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    /// <summary>
    ///     A DbCommand extension method that executes the scalar as or default operation.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>A T.</returns>
    public static T ExecuteScalarAsOrDefault<T>(this DbCommand @this, Func<DbCommand, T> defaultValueFactory)
    {
        try
        {
            return (T) @this.ExecuteScalar();
        }
        catch (Exception)
        {
            return defaultValueFactory(@this);
        }
    }
}