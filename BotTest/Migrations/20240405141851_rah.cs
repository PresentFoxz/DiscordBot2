using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TextCommandFramework.Migrations
{
    /// <inheritdoc />
    public partial class rah : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Damage",
                table: "Weapon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Weapon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "Weapon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Weapon",
                columns: new[] { "Id", "Damage", "Level", "Name", "Value" },
                values: new object[,]
                {
                    { 1, 1, 1, "Fists", 0 },
                    { 2, 5, 1, "Sword", 2 },
                    { 3, 5, 1, "Spear", 3 },
                    { 4, 7, 1, "Axe", 5 },
                    { 5, 12, 1, "GreatSword", 8 },
                    { 6, 200000, 1, "Rock", 10000 },
                    { 7, 3, 1, "Dagger", 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Weapon",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Weapon",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Weapon",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Weapon",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Weapon",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Weapon",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Weapon",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "Damage",
                table: "Weapon");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Weapon");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Weapon");
        }
    }
}
