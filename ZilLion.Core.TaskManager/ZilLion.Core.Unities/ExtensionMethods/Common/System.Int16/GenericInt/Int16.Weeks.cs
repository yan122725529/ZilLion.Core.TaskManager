





using System;

public static partial class Extensions
{
    /// <summary>
    ///     An Int16 extension method that weeks the given this.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A TimeSpan.</returns>
    public static TimeSpan Weeks(this Int16 @this)
    {
        return TimeSpan.FromDays(@this*7);
    }
}