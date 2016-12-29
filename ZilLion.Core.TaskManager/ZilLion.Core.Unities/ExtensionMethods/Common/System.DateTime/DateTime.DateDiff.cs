using System;

public static partial class Extensions
{
    public enum DateInterval
    {
        Year,
        Month,
        Weekday,
        Day
    }

    public static long DateDiff(this DateTime date, DateInterval dateInterval, DateTime date2)
    {
        var ts = Convert.ToDateTime(date2.ToShortDateString()) - Convert.ToDateTime(date.ToShortDateString());
        switch (dateInterval)
        {
            case DateInterval.Year:
                return date2.Year - date.Year;
            case DateInterval.Month:
                return date2.Month - date.Month + 12*(date2.Year - date.Year);
            case DateInterval.Weekday:
                return Fix(ts.Days/7.0);
            case DateInterval.Day:
                return ts.Days;
            default:
                return ts.Days;
        }
    }

    private static long Fix(double number)
    {
        if (number >= 0)
            return (long) Math.Floor(number);
        return (long) Math.Ceiling(number);
    }
}