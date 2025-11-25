using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToSeparatePackageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StackInfos_StackId_PackageLink",
                table: "StackInfos");

            migrationBuilder.DropColumn(
                name: "PackageLink",
                table: "StackInfos");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "StackInfos");

            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                table: "StackInfos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StackInfos_PackageId",
                table: "StackInfos",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StackInfos_StackId_PackageId",
                table: "StackInfos",
                columns: new[] { "StackId", "PackageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_IsVerified",
                table: "Packages",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Link",
                table: "Packages",
                column: "Link",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Name",
                table: "Packages",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_StackInfos_Packages_PackageId",
                table: "StackInfos",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StackInfos_Packages_PackageId",
                table: "StackInfos");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_StackInfos_PackageId",
                table: "StackInfos");

            migrationBuilder.DropIndex(
                name: "IX_StackInfos_StackId_PackageId",
                table: "StackInfos");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "StackInfos");

            migrationBuilder.AddColumn<string>(
                name: "PackageLink",
                table: "StackInfos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                table: "StackInfos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StackInfos_StackId_PackageLink",
                table: "StackInfos",
                columns: new[] { "StackId", "PackageLink" },
                unique: true);
        }
    }
}
