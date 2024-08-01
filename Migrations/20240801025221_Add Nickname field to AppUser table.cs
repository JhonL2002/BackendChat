using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendChat.Migrations
{
    /// <inheritdoc />
    public partial class AddNicknamefieldtoAppUsertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "User",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_User_Nickname",
                table: "User",
                column: "Nickname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Nickname",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "User");
        }
    }
}
