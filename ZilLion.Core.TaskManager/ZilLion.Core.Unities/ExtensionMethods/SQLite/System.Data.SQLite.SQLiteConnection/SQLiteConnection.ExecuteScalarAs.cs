





using System;
using System.Data;
using System.Data.SQLite;

public static partial class Extensions
{
    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="parameters">Options for controlling the operation.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, string cmdText, SQLiteParameter[] parameters, CommandType commandType, SQLiteTransaction transaction)
    {
        using (SQLiteCommand command = @this.CreateCommand())
        {
            command.CommandText = cmdText;
            command.CommandType = commandType;
            command.Transaction = transaction;

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return (T) command.ExecuteScalar();
        }
    }

    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="commandFactory">The command factory.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, Action<SQLiteCommand> commandFactory)
    {
        using (SQLiteCommand command = @this.CreateCommand())
        {
            commandFactory(command);

            return (T) command.ExecuteScalar();
        }
    }

    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, string cmdText)
    {
        return @this.ExecuteScalarAs<T>(cmdText, null, CommandType.Text, null);
    }

    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, string cmdText, SQLiteTransaction transaction)
    {
        return @this.ExecuteScalarAs<T>(cmdText, null, CommandType.Text, transaction);
    }

    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, string cmdText, CommandType commandType)
    {
        return @this.ExecuteScalarAs<T>(cmdText, null, commandType, null);
    }

    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, string cmdText, CommandType commandType, SQLiteTransaction transaction)
    {
        return @this.ExecuteScalarAs<T>(cmdText, null, commandType, transaction);
    }

    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="parameters">Options for controlling the operation.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, string cmdText, SQLiteParameter[] parameters)
    {
        return @this.ExecuteScalarAs<T>(cmdText, parameters, CommandType.Text, null);
    }

    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="parameters">Options for controlling the operation.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, string cmdText, SQLiteParameter[] parameters, SQLiteTransaction transaction)
    {
        return @this.ExecuteScalarAs<T>(cmdText, parameters, CommandType.Text, transaction);
    }

    /// <summary>
    ///     A SQLiteConnection extension method that executes the scalar operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="parameters">Options for controlling the operation.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <returns>An object.</returns>
    public static T ExecuteScalarAs<T>(this SQLiteConnection @this, string cmdText, SQLiteParameter[] parameters, CommandType commandType)
    {
        return @this.ExecuteScalarAs<T>(cmdText, parameters, commandType, null);
    }
}