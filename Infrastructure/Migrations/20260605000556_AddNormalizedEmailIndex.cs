using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedEmailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = 'EmailIndex'
                AND object_id = OBJECT_ID('AspNetUsers')
            )
            BEGIN
                CREATE UNIQUE INDEX [EmailIndex]
                ON [AspNetUsers] ([NormalizedEmail])
                WHERE [NormalizedEmail] IS NOT NULL;
            END
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DROP INDEX IF EXISTS [EmailIndex] ON [AspNetUsers];
        ");

        }
    }
}
