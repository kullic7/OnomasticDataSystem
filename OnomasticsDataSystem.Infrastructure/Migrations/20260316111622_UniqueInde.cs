using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnomasticsDataSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueInde : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_person_first_name_norm_last_name_norm_birth_year_birth_city~",
                table: "person");

            migrationBuilder.CreateIndex(
                name: "IX_person_first_name_norm_last_name_norm_birth_year_birth_city",
                table: "person",
                columns: new[] { "first_name_norm", "last_name_norm", "birth_year", "birth_city" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_person_first_name_norm_last_name_norm_birth_year_birth_city",
                table: "person");

            migrationBuilder.CreateIndex(
                name: "IX_person_first_name_norm_last_name_norm_birth_year_birth_city~",
                table: "person",
                columns: new[] { "first_name_norm", "last_name_norm", "birth_year", "birth_city", "source_id" },
                unique: true);
        }
    }
}
