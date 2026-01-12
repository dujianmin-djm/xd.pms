using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using XD.Pms.Authentication;
using XD.Pms.Books;

namespace XD.Pms.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class PmsDbContext : AbpDbContext<PmsDbContext>, IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<Book> Books { get; set; }
	public DbSet<RefreshToken> RefreshTokens { get; set; }

	#region Entities from the modules

	/* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

	// Identity
	public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    #endregion

    public PmsDbContext(DbContextOptions<PmsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();
        
        builder.Entity<Book>(b =>
        {
            b.ToTable(PmsConsts.DbTablePrefix + "Books", PmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        });

		/* Configure your own tables/entities inside here */

		builder.Entity<RefreshToken>(b =>
		{
			b.ToTable(PmsConsts.DbTablePrefix + "RefreshTokens", PmsConsts.DbSchema);

			//b.Ignore(x => x.Clock);

			b.ConfigureByConvention();

			b.HasKey(x => x.Id);

			b.Property(x => x.Token)
				.IsRequired()
				.HasMaxLength(AuthenticationConsts.RefreshTokenMaxLength);

			b.Property(x => x.DeviceId)
				.HasMaxLength(AuthenticationConsts.DeviceIdMaxLength);

			b.Property(x => x.UserAgent)
				.HasMaxLength(AuthenticationConsts.ClientInfoMaxLength);

			b.Property(x => x.ClientIp)
				.HasMaxLength(64);

			// Ë÷Òý
			b.HasIndex(x => x.Token).IsUnique();
			b.HasIndex(x => x.UserId);
			b.HasIndex(x => new { x.UserId, x.IsRevoked });
			b.HasIndex(x => x.ExpiresAt);
		});
	}
}
