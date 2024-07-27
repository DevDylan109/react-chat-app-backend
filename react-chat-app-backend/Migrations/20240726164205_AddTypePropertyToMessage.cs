using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace react_chat_app_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTypePropertyToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "messages");
        }
    }
}
