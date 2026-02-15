using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;
using XD.Pms.Enums;

namespace XD.Pms.EntityFrameworkCore;

public static class PmsEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new();

    public static void Configure()
    {
        PmsGlobalFeatureConfigurator.Configure();
        PmsModuleExtensionConfigurator.Configure();

        OneTimeRunner.Run(() =>
        {
			/* You can configure extra properties for the
			 * entities defined in the modules used by your application.
			 *
			 * This class can be used to map these extra properties to table fields in the database.
			 *
			 * USE THIS CLASS ONLY TO CONFIGURE EF CORE RELATED MAPPING.
			 * USE PmsModuleExtensionConfigurator CLASS (in the Domain.Shared project)
			 * FOR A HIGH LEVEL API TO DEFINE EXTRA PROPERTIES TO ENTITIES OF THE USED MODULES
			 *
			 * Example: Map a property to a table field:

				 ObjectExtensionManager.Instance
					 .MapEfCoreProperty<IdentityUser, string>(
						 "MyProperty",
						 (entityBuilder, propertyBuilder) =>
						 {
							 propertyBuilder.HasMaxLength(128);
						 }
					 );

			 * See the documentation for more:
			 * https://docs.abp.io/en/abp/latest/Customizing-Application-Modules-Extending-Entities
			 */

			ObjectExtensionManager.Instance
				.AddOrUpdate<IdentityUser>(options =>
				{
					options.AddOrUpdateProperty<string>(
						"Description",
						property =>
						{
							property.Attributes.Add(new StringLengthAttribute(256));
							property.MapEfCore((entityBuilder, propertyBuilder) =>
							{
								propertyBuilder.HasMaxLength(256);
								propertyBuilder.HasDefaultValue("");
							});
						}
					);

					options.AddOrUpdateProperty<Gender>(
						"Gender",
						property =>
						{
							property.DefaultValue = Gender.Unknown;
							property.Attributes.Add(new RequiredAttribute());
							property.MapEfCore((entityBuilder, propertyBuilder) =>
							{
								propertyBuilder.HasColumnType("int");
								propertyBuilder.HasDefaultValue(Gender.Unknown);
								propertyBuilder.IsRequired();
							});
							property.Validators.Add(context =>
							{
								if (!Enum.IsDefined(typeof(Gender), context.Value!))
								{
									var validValues = Enum.GetValues<Gender>().Select(g => $"{(int)g}:{g}");
									context.ValidationErrors.Add(
										new ValidationResult(
											$"请选择有效的性别值: {string.Join(", ", validValues)}",
											["Gender"]
										)
									);
								}
							});
						}
					);
				});


			ObjectExtensionManager.Instance
				.AddOrUpdate<IdentityRole>(options =>
				{
					options.AddOrUpdateProperty<string>(
						"Number",
						property =>
						{
							property.Attributes.Add(new RequiredAttribute());
							property.Attributes.Add(new StringLengthAttribute(50));
							property.MapEfCore((entityBuilder, propertyBuilder) =>
							{
								propertyBuilder.HasMaxLength(50);
								//propertyBuilder.IsRequired();
								entityBuilder.HasIndex("Number").IsUnique();
							});
						}
					);

					options.AddOrUpdateProperty<string>(
						"Description",
						property =>
						{
							property.Attributes.Add(new StringLengthAttribute(256));
							property.MapEfCore((entityBuilder, propertyBuilder) =>
							{
								propertyBuilder.HasMaxLength(256);
								propertyBuilder.HasDefaultValue("");
							});
						}
					);

					options.AddOrUpdateProperty<bool>(
						"IsActive",
						property =>
						{
							property.DefaultValue = false;
							property.MapEfCore((entityBuilder, propertyBuilder) => propertyBuilder.HasDefaultValue(false));
						}
					);
				});
		});
    }
}
