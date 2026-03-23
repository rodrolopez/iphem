using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iphem_web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarResultados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotasLaboratorio",
                table: "turnos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaPdfResultado",
                table: "turnos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotasLaboratorio",
                table: "turnos");

            migrationBuilder.DropColumn(
                name: "RutaPdfResultado",
                table: "turnos");
        }
    }
}
