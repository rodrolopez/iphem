using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iphem_web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFirmaLaboratorista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirmaLaboratorista",
                table: "turnos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirmaLaboratorista",
                table: "turnos");
        }
    }
}
