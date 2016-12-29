





using System;

public static partial class Extensions
{
    /// <summary>
    ///     Returns the specified 16-bit signed integer value as an array of bytes.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>An array of bytes with length 2.</returns>
    public static Byte[] GetBytes(this Int16 value)
    {
        return BitConverter.GetBytes(value);
    }
}