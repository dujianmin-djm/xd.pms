using System.Collections.Generic;
using System.Data;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace XD.Pms;

public interface ISqlExecuter
{
	Task<int> ExecuteAsync(string sql, object? param = null);
	Task<List<T>> QueryAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null) where T : class;
	Task<List<List<T>>> QueryMultipleAsync<T>(string sql, object? param = null) where T : class;
	Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
	Task<List<T>> SqlQueryRawAsync<T>(string sql, params object[] parameters) where T : class;
	Task<DataSet> ExecuteDataSetAsync(string sql, params object[] parameters);
	Task<JsonArray> QueryAsJsonArrayAsync(string sql, params object[] parameters);
}
