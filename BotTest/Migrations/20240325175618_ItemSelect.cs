using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextCommandFramework.Migrations
{
    /// <inheritdoc />
    public partial class ItemSelect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemSelected",
                table: "Profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemSelected",
                table: "Profile");
        }
    }
}
