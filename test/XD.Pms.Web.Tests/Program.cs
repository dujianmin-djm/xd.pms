using Microsoft.AspNetCore.Builder;
using XD.Pms;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("XD.Pms.Web.csproj"); 
await builder.RunAbpModuleAsync<PmsWebTestModule>(applicationName: "XD.Pms.Web");

public partial class Program
{
}
