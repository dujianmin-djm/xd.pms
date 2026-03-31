# XD.Pms
 项目是一套完整的企业级后台管理解决方案，前后端分离的SPA应用。前端采用 Vue 3 + TypeScript + Vite 技术栈，基于 [soybean-admin](https://github.com/soybeanjs/soybean-admin) 进行二次开发；后端采用 .NET 技术栈，基于 [ABP Framework](https://github.com/abpframework/abp) 实现领域驱动设计，提供 RESTful API 接口。

本仓库是系统后端，关键词 RESTful API、SQL Server、EF Core。已实现用户管理、角色管理、员工、岗位、部门等模块的 CRUD 接口，并完成 JWT 认证、权限列表返回等功能，配套前端（soybean-admin）使用。[前端仓库地址](https://github.com/dujianmin-djm/soybean)。（仓库的Web层项目未使用可删除）

## 关于解决方案

这是一个基于[领域驱动设计（DDD）](https://abp.io/docs/latest/framework/architecture/domain-driven-design)实践的分层启动解决方案。所有基本的ABP模块均已安装。使用本仓库代码需要对ABP框架有一定的了解，如需更多信息，请查阅[应用启动模板](https://abp.io/docs/latest/solution-templates/layered-web-application)文档。

### 先决条件

* [.NET10.0 SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node v18 or 20](https://nodejs.org/en)
* [SQL Server 2016+](https://www.microsoft.com/zh-cn/sql-server/)

### 配置

该解决方案附带了一个即开即用的默认配置。但是，在运行解决方案之前，您可能会考虑更改以下配置：

* 请检查`XD.Pms.HttpApi.Host`和`XD.Pms.DbMigrator`项目下`appsettings.json`文件中的`ConnectionStrings`，并根据需要进行更改。

### 运行应用程序之前

* 在你的解决方案文件夹中运行 `abp install-libs` 命令，以安装客户端包依赖项。如果你没有特别禁用此步骤，那么在创建新解决方案时，此步骤会自动执行。但是，如果你首先从源代码管理中克隆了此解决方案，或者向解决方案中添加了新的客户端包依赖项，则应自行运行此命令。
* 运行`XD.Pms.DbMigrator`以创建初始数据库。如果你没有特别禁用此步骤，那么在创建新解决方案时，此步骤也会自动执行。首次运行时，应执行此步骤。如果后续向解决方案中添加了新的数据库迁移，也需要执行此步骤。

#### 生成签名证书

在生产环境中，需要使用生产签名证书。ABP Framework会在应用程序中设置签名和加密证书，并期望在应用程序中找到一个`openiddict.pfx`文件。

要生成签名证书，可以使用以下命令：:

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p a048bd79-c2f6-4a46-9067-12f0a06afb2e
```

> `a048bd79-c2f6-4a46-9067-12f0a06afb2e` 是证书的密码，可以将其更改为您想要的任何密码。

建议使用**两个**与HTTPS所用证书不同的RSA证书：一个用于加密，一个用于签名。

如需更多信息，请参阅：[OpenIddict证书配置](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html#registering-a-certificate-recommended-for-production-ready-scenarios)

> 此外，如需更多信息，请参阅[配置OpenIddict](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment)文档。

### 解决方案结构

这是一个分层单体应用程序，由以下应用程序组成：

* `XD.Pms.DbMigrator`: 一个控制台应用程序，用于应用迁移并初始化数据。它在开发环境和生产环境中都很有用。
* `XD.Pms.HttpApi.Host`: 一个ASP.NET Core Web应用程序，作为HTTP API的宿主（非ABP框架自带，自定义用于提供RESTful API），它包含了所需的基础设施配置（如依赖注入、中间件配置等）。
* `XD.Pms.Web`: 这是一个基于ASP.NET Core MVC和Razor Pages的应用程序，是该解决方案中必不可少的Web应用程序（ABP框架自带，本项目生产环境未用，仅用于测试，可删除）。


## 部署应用程序

部署ABP应用程序的过程与部署任何.NET或ASP.NET Core应用程序的过程相同。但是，有一些重要的注意事项需要牢记。如需详细指导，请参阅ABP的[部署文档](https://abp.io/docs/latest/Deployment/Index).

### 额外资源

可以通过以下资源来深入了解解决方案以及ABP框架：

* [Web应用开发教程](https://abp.io/docs/latest/tutorials/book-store/part-1)
* [应用启动模板](https://abp.io/docs/latest/startup-templates/application/index)
