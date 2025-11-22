using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sparkly_server.Services.Users.Migrations
{
    /// <inheritdoc />
    public partial class Members_project : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_projects_ProjectName",
                table: "projects",
                column: "ProjectName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_projects_ProjectName",
                table: "projects");
        }
    }
}
