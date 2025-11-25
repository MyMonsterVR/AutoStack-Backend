using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicatePackageLinksInStac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StackInfos_StackId_PackageLink",
                table: "StackInfos",
                columns: new[] { "StackId", "PackageLink" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StackInfos_StackId_PackageLink",
                table: "StackInfos");
        }
    }
}
