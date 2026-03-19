using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnomasticsDataSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CityNorm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "birth_city_norm",
                table: "person",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birth_city_norm",
                table: "person");
        }
    }
}
