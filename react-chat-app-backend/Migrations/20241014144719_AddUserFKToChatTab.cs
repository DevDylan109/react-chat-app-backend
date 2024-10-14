using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace react_chat_app_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFKToChatTab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ChatTabs_userId",
                table: "ChatTabs",
                column: "userId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatTabs_Users_userId",
                table: "ChatTabs",
                column: "userId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatTabs_Users_userId",
                table: "ChatTabs");

            migrationBuilder.DropIndex(
                name: "IX_ChatTabs_userId",
                table: "ChatTabs");
        }
    }
}
