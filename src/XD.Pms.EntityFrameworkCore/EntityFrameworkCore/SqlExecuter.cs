using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace XD.Pms.EntityFrameworkCore;

public class SqlExecuter : ISqlExecuter, IScopedDependency
{
	private readonly IDbContextProvider<PmsDbContext> _dbContextProvider;
	public SqlExecuter(IDbContextProvider<PmsDbContext> dbContextProvider)
	{
		_dbContextProvider = dbContextProvider;
	}

	#region Dapper, using the current EF Core transaction.

	public async Task<int> ExecuteAsync(string sql, object? param = null)
	{
		var (connection, transaction) = await GetConnectionAsync();
		return await connection.ExecuteAsync(sql, param, transaction).ConfigureAwait(false);
	}

	public async Task<List<T>> QueryAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null) where T : class
	{
		var (connection, transaction) = await GetConnectionAsync();
		var result = await connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType).ConfigureAwait(false);
		return result.AsList();
	}

	public async Task<List<List<T>>> QueryMultipleAsync<T>(string sql, object? param = null) where T : class
	{
		var (connection, transaction) = await GetConnectionAsync();
		var results = new List<List<T>>();
		using var multi = await connection.QueryMultipleAsync(sql, param, transaction).ConfigureAwait(false);
		while (!multi.IsConsumed)
		{
			var data = await multi.ReadAsync<T>().ConfigureAwait(false);
			results.Add(data.AsList());
		}
		return results;
	}

	private async Task<(IDbConnection Connection, IDbTransaction? Transaction)> GetConnectionAsync()
	{
		var dbContext = await _dbContextProvider.GetDbContextAsync();
		return (dbContext.Database.GetDbConnection(), dbContext.Database.CurrentTransaction?.GetDbTransaction());
	}

	#endregion

	#region EF Core

	public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
	{
		var erpDbContext = await _dbContextProvider.GetDbContextAsync();
		return await erpDbContext.Database.ExecuteSqlRawAsync(sql, parameters).ConfigureAwait(false);
	}

	public async Task<List<T>> SqlQueryRawAsync<T>(string sql, params object[] parameters) where T : class
	{
		var erpDbContext = await _dbContextProvider.GetDbContextAsync();
		return await erpDbContext.Database.SqlQueryRaw<T>(sql, parameters).AsNoTracking().ToListAsync().ConfigureAwait(false);
	}

	#endregion

	#region ADO.NET

	public async Task<DataSet> ExecuteDataSetAsync(string sql, params object[] parameters)
	{
		var erpDbContext = await _dbContextProvider.GetDbContextAsync();
		using var connection = new SqlConnection(erpDbContext.Database.GetConnectionString());
		await connection.OpenAsync().ConfigureAwait(false);
		using var command = new SqlCommand(sql, connection);
		command.Parameters.AddRange(parameters);
		var dataSet = new DataSet();
		using var adapter = new SqlDataAdapter(command);
		adapter.Fill(dataSet); ;
		return dataSet;
	}

	public async Task<JsonArray> QueryAsJsonArrayAsync(string sql, params object[] parameters)
	{
		var erpDbContext = await _dbContextProvider.GetDbContextAsync();
		using var connection = new SqlConnection(erpDbContext.Database.GetConnectionString());
		await connection.OpenAsync().ConfigureAwait(false);
		using var command = new SqlCommand(sql, connection);
		command.Parameters.AddRange(parameters);
		var result = new JsonArray();
		using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
		do
		{
			var table = new JsonArray();
			while (await reader.ReadAsync().ConfigureAwait(false))
			{
				var row = new JsonObject();
				for (var i = 0; i < reader.FieldCount; i++)
				{
					row[reader.GetName(i)] = reader.GetValue(i) != DBNull.Value ? JsonValue.Create(reader.GetValue(i)) : null;
				}
				table.Add(row);
			}
			result.Add(table);
		}
		while (await reader.NextResultAsync().ConfigureAwait(false));
		return result;
	}

	#endregion
}