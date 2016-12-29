





using System;

public static partial class Extensions
{
    /// <summary>
    ///     An Int64 extension method that minutes the given this.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A TimeSpan.</returns>
    public static TimeSpan Minutes(this Int64 @this)
    {
        return TimeSpan.FromMinutes(@this);
    }
}