using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XD.Pms.Migrations
{
    /// <inheritdoc />
    public partial class IdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproverName",
                table: "T_BD_Position");

            migrationBuilder.DropColumn(
                name: "ApproverName",
                table: "T_BD_Employee");

            migrationBuilder.DropColumn(
                name: "ApproverName",
                table: "T_BD_Department");

            migrationBuilder.AddColumn<bool>(
                name: "Leaved",
                table: "T_SYS_Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "PropertyTypeFullName",
                table: "T_SYS_EntityPropertyChanges",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "EntityTypeFullName",
                table: "T_SYS_EntityChanges",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Leaved",
                table: "T_SYS_Users");

            migrationBuilder.AlterColumn<string>(
                name: "PropertyTypeFullName",
                table: "T_SYS_EntityPropertyChanges",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "EntityTypeFullName",
                table: "T_SYS_EntityChanges",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "ApproverName",
                table: "T_BD_Position",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproverName",
                table: "T_BD_Employee",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproverName",
                table: "T_BD_Department",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
