using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XD.Pms.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_BD_Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_BD_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_OIDC_Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrontChannelLogoutUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OIDC_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_OIDC_Scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resources = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OIDC_Scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_ApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    KeyPrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UsageCount = table.Column<long>(type: "bigint", nullable: false),
                    Roles = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Permissions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AllowedIpAddresses = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RateLimitPerMinute = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_ApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_AuditLogExcelFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_AuditLogExcelFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ImpersonatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImpersonatorUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ImpersonatorTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImpersonatorTenantName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionDuration = table.Column<int>(type: "int", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Exceptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_BackgroundJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    JobName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    JobArgs = table.Column<string>(type: "nvarchar(max)", maxLength: 1048576, nullable: false),
                    TryCount = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextTryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAbandoned = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)15),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_BackgroundJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_BlobContainers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_BlobContainers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_ClaimTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Required = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
                    Regex = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RegexDescription = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_ClaimTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_FeatureGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_FeatureGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_Features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ParentName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsVisibleToClients = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailableToHost = table.Column<bool>(type: "bit", nullable: false),
                    AllowedProviders = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ValueType = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_FeatureValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_FeatureValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_LinkUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_LinkUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_OrganizationUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(95)", maxLength: 95, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_OrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_SYS_OrganizationUnits_T_SYS_OrganizationUnits_ParentId",
                        column: x => x.ParentId,
                        principalTable: "T_SYS_OrganizationUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_PermissionGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_PermissionGrants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_PermissionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_PermissionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ParentName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MultiTenancySide = table.Column<byte>(type: "tinyint", nullable: false),
                    Providers = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StateCheckers = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_SecurityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationName = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    Identity = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenantName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_SecurityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Device = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IpAddresses = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SignedIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_SettingDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsVisibleToClients = table.Column<bool>(type: "bit", nullable: false),
                    Providers = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsInherited = table.Column<bool>(type: "bit", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_SettingDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_UserDelegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_UserDelegations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Surname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ShouldChangePasswordOnNextLogin = table.Column<bool>(type: "bit", nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    LastPasswordChangeTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_OIDC_Authorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OIDC_Authorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_OIDC_Authorizations_T_OIDC_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "T_OIDC_Applications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_AuditLogActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MethodName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionDuration = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_AuditLogActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_SYS_AuditLogActions_T_SYS_AuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "T_SYS_AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_EntityChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangeTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeType = table.Column<byte>(type: "tinyint", nullable: false),
                    EntityTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EntityTypeFullName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_EntityChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_SYS_EntityChanges_T_SYS_AuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "T_SYS_AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_Blobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", maxLength: 2147483647, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_Blobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_SYS_Blobs_T_SYS_BlobContainers_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "T_SYS_BlobContainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_OrganizationUnitRoles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_OrganizationUnitRoles", x => new { x.OrganizationUnitId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_T_SYS_OrganizationUnitRoles_T_SYS_OrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "T_SYS_OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_SYS_OrganizationUnitRoles_T_SYS_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "T_SYS_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_RoleClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClaimType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ClaimValue = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_SYS_RoleClaims_T_SYS_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "T_SYS_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_UserClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClaimType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ClaimValue = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_SYS_UserClaims_T_SYS_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_SYS_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_UserLogins",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(196)", maxLength: 196, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_UserLogins", x => new { x.UserId, x.LoginProvider });
                    table.ForeignKey(
                        name: "FK_T_SYS_UserLogins_T_SYS_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_SYS_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_UserOrganizationUnits",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_UserOrganizationUnits", x => new { x.OrganizationUnitId, x.UserId });
                    table.ForeignKey(
                        name: "FK_T_SYS_UserOrganizationUnits_T_SYS_OrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "T_SYS_OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_SYS_UserOrganizationUnits_T_SYS_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_SYS_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_T_SYS_UserRoles_T_SYS_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "T_SYS_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_SYS_UserRoles_T_SYS_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_SYS_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_UserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_T_SYS_UserTokens_T_SYS_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_SYS_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_OIDC_Tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OIDC_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_OIDC_Tokens_T_OIDC_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "T_OIDC_Applications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_T_OIDC_Tokens_T_OIDC_Authorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalTable: "T_OIDC_Authorizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "T_SYS_EntityPropertyChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityChangeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OriginalValue = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    PropertyName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PropertyTypeFullName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SYS_EntityPropertyChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_SYS_EntityPropertyChanges_T_SYS_EntityChanges_EntityChangeId",
                        column: x => x.EntityChangeId,
                        principalTable: "T_SYS_EntityChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_OIDC_Applications_ClientId",
                table: "T_OIDC_Applications",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OIDC_Authorizations_ApplicationId_Status_Subject_Type",
                table: "T_OIDC_Authorizations",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_T_OIDC_Scopes_Name",
                table: "T_OIDC_Scopes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_T_OIDC_Tokens_ApplicationId_Status_Subject_Type",
                table: "T_OIDC_Tokens",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_T_OIDC_Tokens_AuthorizationId",
                table: "T_OIDC_Tokens",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OIDC_Tokens_ReferenceId",
                table: "T_OIDC_Tokens",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_ApiKeys_ClientId",
                table: "T_SYS_ApiKeys",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_ApiKeys_IsActive",
                table: "T_SYS_ApiKeys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_ApiKeys_KeyHash",
                table: "T_SYS_ApiKeys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_ApiKeys_UserId",
                table: "T_SYS_ApiKeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_AuditLogActions_AuditLogId",
                table: "T_SYS_AuditLogActions",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_AuditLogActions_TenantId_ServiceName_MethodName_ExecutionTime",
                table: "T_SYS_AuditLogActions",
                columns: new[] { "TenantId", "ServiceName", "MethodName", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_AuditLogs_TenantId_ExecutionTime",
                table: "T_SYS_AuditLogs",
                columns: new[] { "TenantId", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_AuditLogs_TenantId_UserId_ExecutionTime",
                table: "T_SYS_AuditLogs",
                columns: new[] { "TenantId", "UserId", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_BackgroundJobs_IsAbandoned_NextTryTime",
                table: "T_SYS_BackgroundJobs",
                columns: new[] { "IsAbandoned", "NextTryTime" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_BlobContainers_TenantId_Name",
                table: "T_SYS_BlobContainers",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Blobs_ContainerId",
                table: "T_SYS_Blobs",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Blobs_TenantId_ContainerId_Name",
                table: "T_SYS_Blobs",
                columns: new[] { "TenantId", "ContainerId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_EntityChanges_AuditLogId",
                table: "T_SYS_EntityChanges",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_EntityChanges_TenantId_EntityTypeFullName_EntityId",
                table: "T_SYS_EntityChanges",
                columns: new[] { "TenantId", "EntityTypeFullName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_EntityPropertyChanges_EntityChangeId",
                table: "T_SYS_EntityPropertyChanges",
                column: "EntityChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_FeatureGroups_Name",
                table: "T_SYS_FeatureGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Features_GroupName",
                table: "T_SYS_Features",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Features_Name",
                table: "T_SYS_Features",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_FeatureValues_Name_ProviderName_ProviderKey",
                table: "T_SYS_FeatureValues",
                columns: new[] { "Name", "ProviderName", "ProviderKey" },
                unique: true,
                filter: "[ProviderName] IS NOT NULL AND [ProviderKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_LinkUsers_SourceUserId_SourceTenantId_TargetUserId_TargetTenantId",
                table: "T_SYS_LinkUsers",
                columns: new[] { "SourceUserId", "SourceTenantId", "TargetUserId", "TargetTenantId" },
                unique: true,
                filter: "[SourceTenantId] IS NOT NULL AND [TargetTenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_OrganizationUnitRoles_RoleId_OrganizationUnitId",
                table: "T_SYS_OrganizationUnitRoles",
                columns: new[] { "RoleId", "OrganizationUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_OrganizationUnits_Code",
                table: "T_SYS_OrganizationUnits",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_OrganizationUnits_ParentId",
                table: "T_SYS_OrganizationUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_PermissionGrants_TenantId_Name_ProviderName_ProviderKey",
                table: "T_SYS_PermissionGrants",
                columns: new[] { "TenantId", "Name", "ProviderName", "ProviderKey" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_PermissionGroups_Name",
                table: "T_SYS_PermissionGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Permissions_GroupName",
                table: "T_SYS_Permissions",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Permissions_Name",
                table: "T_SYS_Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_RoleClaims_RoleId",
                table: "T_SYS_RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Roles_NormalizedName",
                table: "T_SYS_Roles",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_SecurityLogs_TenantId_Action",
                table: "T_SYS_SecurityLogs",
                columns: new[] { "TenantId", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_SecurityLogs_TenantId_ApplicationName",
                table: "T_SYS_SecurityLogs",
                columns: new[] { "TenantId", "ApplicationName" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_SecurityLogs_TenantId_Identity",
                table: "T_SYS_SecurityLogs",
                columns: new[] { "TenantId", "Identity" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_SecurityLogs_TenantId_UserId",
                table: "T_SYS_SecurityLogs",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Sessions_Device",
                table: "T_SYS_Sessions",
                column: "Device");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Sessions_SessionId",
                table: "T_SYS_Sessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Sessions_TenantId_UserId",
                table: "T_SYS_Sessions",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_SettingDefinitions_Name",
                table: "T_SYS_SettingDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Settings_Name_ProviderName_ProviderKey",
                table: "T_SYS_Settings",
                columns: new[] { "Name", "ProviderName", "ProviderKey" },
                unique: true,
                filter: "[ProviderName] IS NOT NULL AND [ProviderKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_UserClaims_UserId",
                table: "T_SYS_UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_UserLogins_LoginProvider_ProviderKey",
                table: "T_SYS_UserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_UserOrganizationUnits_UserId_OrganizationUnitId",
                table: "T_SYS_UserOrganizationUnits",
                columns: new[] { "UserId", "OrganizationUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_UserRoles_RoleId_UserId",
                table: "T_SYS_UserRoles",
                columns: new[] { "RoleId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Users_Email",
                table: "T_SYS_Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Users_NormalizedEmail",
                table: "T_SYS_Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Users_NormalizedUserName",
                table: "T_SYS_Users",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_T_SYS_Users_UserName",
                table: "T_SYS_Users",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_BD_Books");

            migrationBuilder.DropTable(
                name: "T_OIDC_Scopes");

            migrationBuilder.DropTable(
                name: "T_OIDC_Tokens");

            migrationBuilder.DropTable(
                name: "T_SYS_ApiKeys");

            migrationBuilder.DropTable(
                name: "T_SYS_AuditLogActions");

            migrationBuilder.DropTable(
                name: "T_SYS_AuditLogExcelFiles");

            migrationBuilder.DropTable(
                name: "T_SYS_BackgroundJobs");

            migrationBuilder.DropTable(
                name: "T_SYS_Blobs");

            migrationBuilder.DropTable(
                name: "T_SYS_ClaimTypes");

            migrationBuilder.DropTable(
                name: "T_SYS_EntityPropertyChanges");

            migrationBuilder.DropTable(
                name: "T_SYS_FeatureGroups");

            migrationBuilder.DropTable(
                name: "T_SYS_Features");

            migrationBuilder.DropTable(
                name: "T_SYS_FeatureValues");

            migrationBuilder.DropTable(
                name: "T_SYS_LinkUsers");

            migrationBuilder.DropTable(
                name: "T_SYS_OrganizationUnitRoles");

            migrationBuilder.DropTable(
                name: "T_SYS_PermissionGrants");

            migrationBuilder.DropTable(
                name: "T_SYS_PermissionGroups");

            migrationBuilder.DropTable(
                name: "T_SYS_Permissions");

            migrationBuilder.DropTable(
                name: "T_SYS_RoleClaims");

            migrationBuilder.DropTable(
                name: "T_SYS_SecurityLogs");

            migrationBuilder.DropTable(
                name: "T_SYS_Sessions");

            migrationBuilder.DropTable(
                name: "T_SYS_SettingDefinitions");

            migrationBuilder.DropTable(
                name: "T_SYS_Settings");

            migrationBuilder.DropTable(
                name: "T_SYS_UserClaims");

            migrationBuilder.DropTable(
                name: "T_SYS_UserDelegations");

            migrationBuilder.DropTable(
                name: "T_SYS_UserLogins");

            migrationBuilder.DropTable(
                name: "T_SYS_UserOrganizationUnits");

            migrationBuilder.DropTable(
                name: "T_SYS_UserRoles");

            migrationBuilder.DropTable(
                name: "T_SYS_UserTokens");

            migrationBuilder.DropTable(
                name: "T_OIDC_Authorizations");

            migrationBuilder.DropTable(
                name: "T_SYS_BlobContainers");

            migrationBuilder.DropTable(
                name: "T_SYS_EntityChanges");

            migrationBuilder.DropTable(
                name: "T_SYS_OrganizationUnits");

            migrationBuilder.DropTable(
                name: "T_SYS_Roles");

            migrationBuilder.DropTable(
                name: "T_SYS_Users");

            migrationBuilder.DropTable(
                name: "T_OIDC_Applications");

            migrationBuilder.DropTable(
                name: "T_SYS_AuditLogs");
        }
    }
}
