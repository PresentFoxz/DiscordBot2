using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextCommandFramework.Migrations
{
    /// <inheritdoc />
    public partial class ShopItemsSave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShopItemsSave",
                table: "Profile",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopItemsSave",
                table: "Profile");
        }
    }
}
