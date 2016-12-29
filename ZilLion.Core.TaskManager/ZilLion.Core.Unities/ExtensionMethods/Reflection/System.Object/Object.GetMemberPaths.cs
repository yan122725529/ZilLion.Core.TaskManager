using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

public static partial class Extensions
{
    /// <summary>A T extension method that gets member paths.</summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="path">Full pathname of the file.</param>
    /// <returns>An array of member information.</returns>
    public static MemberInfo[] GetMemberPaths<T>(this T @this, string path)
    {
        var lastType = @this.GetType();
        var paths = path.Split('.');

        var memberPaths = new List<MemberInfo>();

        foreach (var item in paths)
        {
            var propertyInfo = lastType.GetProperty(item);
            var fieldInfo = lastType.GetField(item);

            if (propertyInfo != null)
            {
                memberPaths.Add(propertyInfo);
                lastType = propertyInfo.PropertyType;
            }
            if (fieldInfo == null) continue;
            memberPaths.Add(fieldInfo);
            lastType = fieldInfo.FieldType;
        }

        return memberPaths.ToArray();
    }


    /// <summary>
    ///     Gets the member info represented by an expression.
    /// </summary>
    /// <param name="expression">The member expression.</param>
    /// <returns>The member info represeted by the expression.</returns>
    public static MemberInfo GetMemberInfo(this Expression expression)
    {
        var lambda = (LambdaExpression) expression;

        MemberExpression memberExpression;
        var body = lambda.Body as UnaryExpression;
        if (body != null)
        {
            var unaryExpression = body;
            memberExpression = (MemberExpression) unaryExpression.Operand;
        }
        else memberExpression = (MemberExpression) lambda.Body;

        return memberExpression.Member;
    }

    public static object GetValue(this Expression expression)
    {
        var lambda = (LambdaExpression) expression;
        return lambda.Compile().DynamicInvoke();
    }
}