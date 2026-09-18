using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessRequestHub.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequesterForeignKeyAndValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_RequesterEmail",
                table: "AccessRequests",
                column: "RequesterEmail");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AccessRequests_Justification_NotBlank",
                table: "AccessRequests",
                sql: "length(btrim(\"Justification\")) > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRequests_Users_RequesterEmail",
                table: "AccessRequests",
                column: "RequesterEmail",
                principalTable: "Users",
                principalColumn: "Email",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessRequests_Users_RequesterEmail",
                table: "AccessRequests");

            migrationBuilder.DropIndex(
                name: "IX_AccessRequests_RequesterEmail",
                table: "AccessRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AccessRequests_Justification_NotBlank",
                table: "AccessRequests");
        }
    }
}
