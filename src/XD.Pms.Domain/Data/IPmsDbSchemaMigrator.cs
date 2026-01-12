using System.Threading.Tasks;

namespace XD.Pms.Data;

public interface IPmsDbSchemaMigrator
{
    Task MigrateAsync();
}
