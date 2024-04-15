using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextCommandFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "List",
                columns: table => new
                {
                    UserListId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_List", x => x.UserListId);
                });

            migrationBuilder.CreateTable(
                name: "Profile",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    DiscordId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Experience = table.Column<int>(type: "INTEGER", nullable: false),
                    Inventory = table.Column<string>(type: "TEXT", nullable: true),
                    Fight = table.Column<int>(type: "INTEGER", nullable: false),
                    CName = table.Column<string>(type: "TEXT", nullable: true),
                    CExpGain = table.Column<int>(type: "INTEGER", nullable: false),
                    CHP = table.Column<int>(type: "INTEGER", nullable: false),
                    CDamage = table.Column<int>(type: "INTEGER", nullable: false),
                    UserListId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profile_List_UserListId",
                        column: x => x.UserListId,
                        principalTable: "List",
                        principalColumn: "UserListId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profile_UserListId",
                table: "Profile",
                column: "UserListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profile");

            migrationBuilder.DropTable(
                name: "List");
        }
    }
}
