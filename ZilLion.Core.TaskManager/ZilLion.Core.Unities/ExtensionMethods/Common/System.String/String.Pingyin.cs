using ZilLion.Core.Unities.UnitiesMethods;

public static partial class Extensions
{
    /// <summary>A string extension method that query if '@this' is palindrome.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if palindrome, false if not.</returns>
    public static string GetPinyin(this string @this)
    {
        return PingyinHelper.GetPymFromStr(@this);
    }
}