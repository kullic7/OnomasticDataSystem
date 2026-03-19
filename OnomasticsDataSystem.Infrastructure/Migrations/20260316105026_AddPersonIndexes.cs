using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnomasticsDataSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_person_birth_city",
                table: "person",
                column: "birth_city");

            migrationBuilder.CreateIndex(
                name: "IX_person_birth_year",
                table: "person",
                column: "birth_year");

            migrationBuilder.CreateIndex(
                name: "IX_person_first_name_norm",
                table: "person",
                column: "first_name_norm");

            migrationBuilder.CreateIndex(
                name: "IX_person_last_name_norm",
                table: "person",
                column: "last_name_norm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_person_birth_city",
                table: "person");

            migrationBuilder.DropIndex(
                name: "IX_person_birth_year",
                table: "person");

            migrationBuilder.DropIndex(
                name: "IX_person_first_name_norm",
                table: "person");

            migrationBuilder.DropIndex(
                name: "IX_person_last_name_norm",
                table: "person");
        }
    }
}
