using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnomasticsDataSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CityNormIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_person_birth_city",
                table: "person");

            migrationBuilder.CreateIndex(
                name: "IX_person_birth_city_norm",
                table: "person",
                column: "birth_city_norm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_person_birth_city_norm",
                table: "person");

            migrationBuilder.CreateIndex(
                name: "IX_person_birth_city",
                table: "person",
                column: "birth_city");
        }
    }
}
