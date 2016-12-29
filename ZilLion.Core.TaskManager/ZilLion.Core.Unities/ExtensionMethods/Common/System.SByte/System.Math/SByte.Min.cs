





using System;

public static partial class Extensions
{
    /// <summary>
    ///     Returns the smaller of two 8-bit signed integers.
    /// </summary>
    /// <param name="val1">The first of two 8-bit signed integers to compare.</param>
    /// <param name="val2">The second of two 8-bit signed integers to compare.</param>
    /// <returns>Parameter  or , whichever is smaller.</returns>
    public static SByte Min(this SByte val1, SByte val2)
    {
        return Math.Min(val1, val2);
    }
}