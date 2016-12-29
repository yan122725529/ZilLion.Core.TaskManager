





using System;

public static partial class Extensions
{
    /// <summary>
    ///     An object extension method that converts the @this to a nullable short.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as a short?</returns>
    public static short? ToNullableShort(this object @this)
    {
        if (@this == null || @this == DBNull.Value)
        {
            return null;
        }

        return Convert.ToInt16(@this);
    }
}