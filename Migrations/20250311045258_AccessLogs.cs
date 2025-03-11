using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESA_Terra_Argila.Migrations
{
    /// <inheritdoc />
    public partial class AccessLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ip",
                table: "LogEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ip",
                table: "LogEntries");
        }
    }
}
