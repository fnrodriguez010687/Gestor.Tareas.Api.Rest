using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestor.Tareas.Api.Rest.Migrations
{
    /// <inheritdoc />
    public partial class ConvertTaskStatusToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Tasks" ADD COLUMN "StatusTemp" integer;

                UPDATE "Tasks"
                SET "StatusTemp" = CASE "Status"
                    WHEN 'Pendiente' THEN 1
                    WHEN 'EnCurso' THEN 2
                    WHEN 'Hecha' THEN 3
                    ELSE NULL
                END;

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Tasks" WHERE "StatusTemp" IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Unknown task status found in Tasks.Status';
                    END IF;
                END $$;

                ALTER TABLE "Tasks" ALTER COLUMN "StatusTemp" SET NOT NULL;
                ALTER TABLE "Tasks" DROP COLUMN "Status";
                ALTER TABLE "Tasks" RENAME COLUMN "StatusTemp" TO "Status";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Tasks" ADD COLUMN "StatusTemp" character varying(20);

                UPDATE "Tasks"
                SET "StatusTemp" = CASE "Status"
                    WHEN 1 THEN 'Pendiente'
                    WHEN 2 THEN 'EnCurso'
                    WHEN 3 THEN 'Hecha'
                    ELSE NULL
                END;

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Tasks" WHERE "StatusTemp" IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Unknown numeric task status found in Tasks.Status';
                    END IF;
                END $$;

                ALTER TABLE "Tasks" ALTER COLUMN "StatusTemp" SET NOT NULL;
                ALTER TABLE "Tasks" ALTER COLUMN "StatusTemp" TYPE character varying(20);
                ALTER TABLE "Tasks" DROP COLUMN "Status";
                ALTER TABLE "Tasks" RENAME COLUMN "StatusTemp" TO "Status";
                """);
        }
    }
}
