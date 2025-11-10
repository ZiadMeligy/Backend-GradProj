using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP_Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssignDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedDoctorId",
                table: "Studies",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Studies_AssignedDoctorId",
                table: "Studies",
                column: "AssignedDoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Studies_AspNetUsers_AssignedDoctorId",
                table: "Studies",
                column: "AssignedDoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Studies_AspNetUsers_AssignedDoctorId",
                table: "Studies");

            migrationBuilder.DropIndex(
                name: "IX_Studies_AssignedDoctorId",
                table: "Studies");

            migrationBuilder.DropColumn(
                name: "AssignedDoctorId",
                table: "Studies");
        }
    }
}
