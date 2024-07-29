using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace react_chat_app_backend.Migrations
{
    /// <inheritdoc />
    public partial class addFriendShipDbSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShip_UserData_RelatedUserId",
                table: "UserFriendShip");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShip_UserData_UserId",
                table: "UserFriendShip");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFriendShip",
                table: "UserFriendShip");

            migrationBuilder.RenameTable(
                name: "UserFriendShip",
                newName: "UserFriendShips");

            migrationBuilder.RenameIndex(
                name: "IX_UserFriendShip_RelatedUserId",
                table: "UserFriendShips",
                newName: "IX_UserFriendShips_RelatedUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFriendShips",
                table: "UserFriendShips",
                columns: new[] { "UserId", "RelatedUserId" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_UserData_RelatedUserId",
                table: "UserFriendShips");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriendShips_UserData_UserId",
                table: "UserFriendShips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFriendShips",
                table: "UserFriendShips");

            migrationBuilder.RenameTable(
                name: "UserFriendShips",
                newName: "UserFriendShip");

            migrationBuilder.RenameIndex(
                name: "IX_UserFriendShips_RelatedUserId",
                table: "UserFriendShip",
                newName: "IX_UserFriendShip_RelatedUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFriendShip",
                table: "UserFriendShip",
                columns: new[] { "UserId", "RelatedUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriendShip_UserData_RelatedUserId",
                table: "UserFriendShip",
                column: "RelatedUserId",
                principalTable: "UserData",
                principalColumn: "userId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriendShip_UserData_UserId",
                table: "UserFriendShip",
                column: "UserId",
                principalTable: "UserData",
                principalColumn: "userId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
