





using System;
using System.Data;
using System.Data.Common;

public static partial class Extensions
{
    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="parameters">Options for controlling the operation.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, string cmdText, DbParameter[] parameters, CommandType commandType, DbTransaction transaction)
    {
        using (DbCommand command = @this.CreateCommand())
        {
            command.CommandText = cmdText;
            command.CommandType = commandType;
            command.Transaction = transaction;

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using (IDataReader reader = command.ExecuteReader())
            {
                reader.Read();
                return reader.ToExpandoObject();
            }
        }
    }

    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="commandFactory">The command factory.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, Action<DbCommand> commandFactory)
    {
        using (DbCommand command = @this.CreateCommand())
        {
            commandFactory(command);

            using (IDataReader reader = command.ExecuteReader())
            {
                reader.Read();
                return reader.ToExpandoObject();
            }
        }
    }

    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, string cmdText)
    {
        return @this.ExecuteExpandoObject(cmdText, null, CommandType.Text, null);
    }

    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, string cmdText, DbTransaction transaction)
    {
        return @this.ExecuteExpandoObject(cmdText, null, CommandType.Text, transaction);
    }

    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, string cmdText, CommandType commandType)
    {
        return @this.ExecuteExpandoObject(cmdText, null, commandType, null);
    }

    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, string cmdText, CommandType commandType, DbTransaction transaction)
    {
        return @this.ExecuteExpandoObject(cmdText, null, commandType, transaction);
    }

    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="parameters">Options for controlling the operation.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, string cmdText, DbParameter[] parameters)
    {
        return @this.ExecuteExpandoObject(cmdText, parameters, CommandType.Text, null);
    }

    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="parameters">Options for controlling the operation.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, string cmdText, DbParameter[] parameters, DbTransaction transaction)
    {
        return @this.ExecuteExpandoObject(cmdText, parameters, CommandType.Text, transaction);
    }

    /// <summary>
    ///     A DbConnection extension method that executes the expando object operation.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="cmdText">The command text.</param>
    /// <param name="parameters">Options for controlling the operation.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <returns>A dynamic.</returns>
    public static dynamic ExecuteExpandoObject(this DbConnection @this, string cmdText, DbParameter[] parameters, CommandType commandType)
    {
        return @this.ExecuteExpandoObject(cmdText, parameters, commandType, null);
    }
}