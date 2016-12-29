





using System.Data;
using System.Data.SQLite;

public static partial class Extensions
{
    /// <summary>
    ///     Executes the query, and returns the result set as DataSet.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A DataSet that is equivalent to the result set.</returns>
    public static DataSet ExecuteDataSet(this SQLiteCommand @this)
    {
        var ds = new DataSet();
        using (var dataAdapter = new SQLiteDataAdapter(@this))
        {
            dataAdapter.Fill(ds);
        }

        return ds;
    }
}