using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace react_chat_app_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_UserData_RelatedUserId",
                table: "UserFriendShips");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_UserData_UserId",
                table: "UserFriendShips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserData",
                table: "UserData");

            migrationBuilder.RenameTable(
                name: "UserData",
                newName: "Users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "userId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_Users_RelatedUserId",
                table: "UserFriendShips");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_Users_UserId",
                table: "UserFriendShips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "UserData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserData",
                table: "UserData",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriendShips_UserData_RelatedUserId",
                table: "UserFriendShips",
                column: "RelatedUserId",
                principalTable: "UserData",
                principalColumn: "userId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriendShips_UserData_UserId",
                table: "UserFriendShips",
                column: "UserId",
                principalTable: "UserData",
                principalColumn: "userId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
