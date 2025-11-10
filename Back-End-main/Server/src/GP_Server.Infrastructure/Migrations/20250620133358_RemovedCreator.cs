using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP_Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Studies_AspNetUsers_CreatorId",
                table: "Studies");

            migrationBuilder.DropIndex(
                name: "IX_Studies_CreatorId",
                table: "Studies");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Studies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Studies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_CreatorId",
                table: "Studies",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Studies_AspNetUsers_CreatorId",
                table: "Studies",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
