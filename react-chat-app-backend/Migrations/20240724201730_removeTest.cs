using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace react_chat_app_backend.Migrations
{
    /// <inheritdoc />
    public partial class removeTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "test",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "test",
                table: "Messages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
