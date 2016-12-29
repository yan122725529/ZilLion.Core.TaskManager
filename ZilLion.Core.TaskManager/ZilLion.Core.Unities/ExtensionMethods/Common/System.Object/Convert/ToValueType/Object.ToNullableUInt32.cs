





using System;

public static partial class Extensions
{
    /// <summary>
    ///     An object extension method that converts the @this to a nullable u int 32.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as an uint?</returns>
    public static uint? ToNullableUInt32(this object @this)
    {
        if (@this == null || @this == DBNull.Value)
        {
            return null;
        }

        return Convert.ToUInt32(@this);
    }
}