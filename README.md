# XD.Pms
 项目是一套完整的企业级后台管理解决方案，前后端分离的SPA应用。前端采用 Vue 3 + TypeScript + Vite 技术栈，基于 [soybean-admin](https://github.com/soybeanjs/soybean-admin) 进行二次开发；后端采用 .NET 技术栈，基于 [ABP Framework](https://github.com/abpframework/abp) 实现领域驱动设计，提供 RESTful API 接口。

本仓库是系统后端，关键词 RESTful API、SQL Server、EF Core。已实现用户管理、角色管理、员工、岗位、部门等模块的 CRUD 接口，并完成 JWT 认证、权限列表返回等功能，配套前端（soybean-admin）使用。[前端仓库地址](https://github.com/dujianmin-djm/soybean)。（仓库的Web层项目未使用可删除）

## About this solution

这是一个基于[领域驱动设计（DDD）](https://abp.io/docs/latest/framework/architecture/domain-driven-design)实践的分层启动解决方案。所有基本的ABP模块均已安装。使用本仓库代码需要对ABP框架有一定的了解，如需更多信息，请查阅[应用启动模板](https://abp.io/docs/latest/solution-templates/layered-web-application)文档。

### Pre-requirements

* [.NET10.0 SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node v18 or 20](https://nodejs.org/en)

### Configurations

The solution comes with a default configuration that works out of the box. However, you may consider to change the following configuration before running your solution:

* Check the `ConnectionStrings` in `appsettings.json` files under the `XD.Pms.Web` and `XD.Pms.DbMigrator` projects and change it if you need.

### Before running the application

* Run `abp install-libs` command on your solution folder to install client-side package dependencies. This step is automatically done when you create a new solution, if you didn't especially disabled it. However, you should run it yourself if you have first cloned this solution from your source control, or added a new client-side package dependency to your solution.
* Run `XD.Pms.DbMigrator` to create the initial database. This step is also automatically done when you create a new solution, if you didn't especially disabled it. This should be done in the first run. It is also needed if a new database migration is added to the solution later.

#### Generating a Signing Certificate

In the production environment, you need to use a production signing certificate. ABP Framework sets up signing and encryption certificates in your application and expects an `openiddict.pfx` file in your application.

To generate a signing certificate, you can use the following command:

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p a048bd79-c2f6-4a46-9067-12f0a06afb2e
```

> `a048bd79-c2f6-4a46-9067-12f0a06afb2e` is the password of the certificate, you can change it to any password you want.

It is recommended to use **two** RSA certificates, distinct from the certificate(s) used for HTTPS: one for encryption, one for signing.

For more information, please refer to: [OpenIddict Certificate Configuration](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html#registering-a-certificate-recommended-for-production-ready-scenarios)

> Also, see the [Configuring OpenIddict](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment) documentation for more information.

### Solution structure

This is a layered monolith application that consists of the following applications:

* `XD.Pms.DbMigrator`: A console application which applies the migrations and also seeds the initial data. It is useful on development as well as on production environment.
* `XD.Pms.Web`: ASP.NET Core MVC / Razor Pages application that is the essential web application of the solution.


## Deploying the application

Deploying an ABP application follows the same process as deploying any .NET or ASP.NET Core application. However, there are important considerations to keep in mind. For detailed guidance, refer to ABP's [deployment documentation](https://abp.io/docs/latest/Deployment/Index).

### Additional resources

You can see the following resources to learn more about your solution and the ABP Framework:

* [Web Application Development Tutorial](https://abp.io/docs/latest/tutorials/book-store/part-1)
* [Application Startup Template](https://abp.io/docs/latest/startup-templates/application/index)
