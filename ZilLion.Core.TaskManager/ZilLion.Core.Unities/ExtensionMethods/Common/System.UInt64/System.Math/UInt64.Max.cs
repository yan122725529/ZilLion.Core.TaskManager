





using System;

public static partial class Extensions
{
    /// <summary>
    ///     Returns the larger of two 64-bit unsigned integers.
    /// </summary>
    /// <param name="val1">The first of two 64-bit unsigned integers to compare.</param>
    /// <param name="val2">The second of two 64-bit unsigned integers to compare.</param>
    /// <returns>Parameter  or , whichever is larger.</returns>
    public static UInt64 Max(this UInt64 val1, UInt64 val2)
    {
        return Math.Max(val1, val2);
    }
}