using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnomasticsDataSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNullYearTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_invalid_insert()
                RETURNS trigger AS $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM person p
                        WHERE p.first_name_norm = NEW.first_name_norm
                          AND p.last_name_norm = NEW.last_name_norm
                          AND p.birth_city_norm = NEW.birth_city_norm
                          AND (
                                (p.birth_year IS NULL AND NEW.birth_year IS NULL)
                             OR (p.birth_year = NEW.birth_year)
                             OR (p.birth_year IS NOT NULL AND NEW.birth_year IS NULL)
                          )
                    ) THEN
                        RETURN NULL; -- ❗ ticho ignoruje insert
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

			        migrationBuilder.Sql(@"
                CREATE TRIGGER trg_prevent_invalid_insert
                BEFORE INSERT OR UPDATE ON person
                FOR EACH ROW
                EXECUTE FUNCTION prevent_invalid_insert();
            ");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trg_prevent_null_overwrite ON person;
            ");

			        migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS prevent_null_overwrite;
            ");
		}
    }
}
