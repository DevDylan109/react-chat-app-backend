using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace react_chat_app_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeToFriendShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_Users_RelatedUserId",
                table: "UserFriendShips");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_Users_UserId",
                table: "UserFriendShips");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriendShips_Users_RelatedUserId",
                table: "UserFriendShips",
                column: "RelatedUserId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriendShips_Users_UserId",
                table: "UserFriendShips",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_Users_RelatedUserId",
                table: "UserFriendShips");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_Users_UserId",
                table: "UserFriendShips");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriendShips_Users_RelatedUserId",
                table: "UserFriendShips",
                column: "RelatedUserId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriendShips_Users_UserId",
                table: "UserFriendShips",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
