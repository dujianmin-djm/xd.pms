using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XD.Pms.Migrations
{
    /// <inheritdoc />
    public partial class Employee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_BD_Department",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApproverName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApprovalTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_BD_Department", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_BD_Department_T_BD_Department_ParentId",
                        column: x => x.ParentId,
                        principalTable: "T_BD_Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_BD_Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApproverName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApprovalTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_BD_Employee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_BD_Position",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsLeader = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApproverName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApprovalTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_BD_Position", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_BD_Position_T_BD_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "T_BD_Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_BD_EmployeePosition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_BD_EmployeePosition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_BD_EmployeePosition_T_BD_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "T_BD_Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_BD_EmployeePosition_T_BD_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "T_BD_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_BD_EmployeePosition_T_BD_Position_PositionId",
                        column: x => x.PositionId,
                        principalTable: "T_BD_Position",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_BD_Department_Number",
                table: "T_BD_Department",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_BD_Department_ParentId",
                table: "T_BD_Department",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_T_BD_Employee_Number",
                table: "T_BD_Employee",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_BD_EmployeePosition_DepartmentId",
                table: "T_BD_EmployeePosition",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_T_BD_EmployeePosition_EmployeeId_DepartmentId_PositionId",
                table: "T_BD_EmployeePosition",
                columns: new[] { "EmployeeId", "DepartmentId", "PositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_BD_EmployeePosition_PositionId",
                table: "T_BD_EmployeePosition",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_BD_Position_DepartmentId",
                table: "T_BD_Position",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_T_BD_Position_Number",
                table: "T_BD_Position",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_BD_EmployeePosition");

            migrationBuilder.DropTable(
                name: "T_BD_Employee");

            migrationBuilder.DropTable(
                name: "T_BD_Position");

            migrationBuilder.DropTable(
                name: "T_BD_Department");
        }
    }
}
