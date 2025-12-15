using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStackVoting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownvoteCount",
                table: "Stacks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpvoteCount",
                table: "Stacks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StackVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StackId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsUpvote = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StackVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StackVotes_Stacks_StackId",
                        column: x => x.StackId,
                        principalTable: "Stacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StackVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StackVotes_StackId",
                table: "StackVotes",
                column: "StackId");

            migrationBuilder.CreateIndex(
                name: "IX_StackVotes_UserId",
                table: "StackVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StackVotes_UserId_StackId",
                table: "StackVotes",
                columns: new[] { "UserId", "StackId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StackVotes");

            migrationBuilder.DropColumn(
                name: "DownvoteCount",
                table: "Stacks");

            migrationBuilder.DropColumn(
                name: "UpvoteCount",
                table: "Stacks");
        }
    }
}
