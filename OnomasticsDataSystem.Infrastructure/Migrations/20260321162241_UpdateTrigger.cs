using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnomasticsDataSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
        CREATE OR REPLACE FUNCTION prevent_null_overwrite()
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
                RETURN NULL; -- 🔥 namiesto RAISE EXCEPTION
            END IF;

            RETURN NEW;
        END;
        $$ LANGUAGE plpgsql;
    ");
			migrationBuilder.Sql(@"
        DROP TRIGGER IF EXISTS trg_prevent_null_overwrite ON person;

        CREATE TRIGGER trg_prevent_null_overwrite
        BEFORE INSERT OR UPDATE ON person
        FOR EACH ROW
        EXECUTE FUNCTION prevent_null_overwrite();
    ");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
        DROP TRIGGER IF EXISTS trg_prevent_null_overwrite ON person;
    ");

			migrationBuilder.Sql(@"
        CREATE OR REPLACE FUNCTION prevent_null_overwrite()
        RETURNS trigger AS $$
        BEGIN
            IF NEW.birth_year IS NULL THEN
                IF EXISTS (
                    SELECT 1 FROM person p
                    WHERE p.first_name_norm = NEW.first_name_norm
                      AND p.last_name_norm = NEW.last_name_norm
                      AND p.birth_city_norm = NEW.birth_city_norm
                      AND p.birth_year IS NOT NULL
                ) THEN
                    RAISE EXCEPTION 'Cannot insert NULL birth_year when specific year exists';
                END IF;
            END IF;

            RETURN NEW;
        END;
        $$ LANGUAGE plpgsql;
    ");
		}
    }
}
