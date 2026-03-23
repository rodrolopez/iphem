using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iphem_web.Migrations
{
    /// <inheritdoc />
    public partial class VincularTurnoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "turnos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_turnos_IdUsuario",
                table: "turnos",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_turnos_usuarios_IdUsuario",
                table: "turnos",
                column: "IdUsuario",
                principalTable: "usuarios",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_turnos_usuarios_IdUsuario",
                table: "turnos");

            migrationBuilder.DropIndex(
                name: "IX_turnos_IdUsuario",
                table: "turnos");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "turnos");
        }
    }
}
