using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESA_Terra_Argila.Data.Migrations
{
    /// <inheritdoc />
    public partial class addUsernameAcessLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AccessLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AccessLogs");
        }
    }
}
