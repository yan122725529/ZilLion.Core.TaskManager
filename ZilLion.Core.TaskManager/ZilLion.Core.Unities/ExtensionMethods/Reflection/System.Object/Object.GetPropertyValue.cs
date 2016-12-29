





using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

public static partial class Extensions
{
    /// <summary>
    ///     A T extension method that gets property value.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>The property value.</returns>
    public static object GetPropertyValue<T>(this T @this, string propertyName)
    {
        var type = @this.GetType();
        var property = type.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        return property.GetValue(@this, null);
    }
    /// <summary>
    ///     Retrieves the name of a property referenced by a lambda expression.
    /// </summary>
    /// <typeparam name="TObject">The type of object containing the property.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="this">The object containing the property.</param>
    /// <param name="expression">A lambda expression selecting the property from the containing object.</param>
    /// <returns>The name of the property referenced by <paramref name="expression" />.</returns>
    [Pure]
    public static string GetPropertyNameByExpression<TObject, TProperty>(this TObject @this,
        Expression<Func<TObject, TProperty>> expression)
    {
        // For more information on the technique used here, see these blog posts:
        // Note that the following blog post:
        // uses a similar technique, but must also account for implicit casts to object by checking for UnaryExpression.
        // Our solution uses generics, so this additional test is not necessary.
        return ((MemberExpression) expression.Body).Member.Name;
    }
}