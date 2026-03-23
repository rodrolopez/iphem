using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace iphem_web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaTriage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "turnos",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHora",
                table: "Auditorias",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "Triages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTurno = table.Column<int>(type: "integer", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PesoMayor50Kg = table.Column<bool>(type: "boolean", nullable: false),
                    SentirseBienHoy = table.Column<bool>(type: "boolean", nullable: false),
                    TatuajesPiercings12Meses = table.Column<bool>(type: "boolean", nullable: false),
                    Cirugias12Meses = table.Column<bool>(type: "boolean", nullable: false),
                    TomoAntibioticos7Dias = table.Column<bool>(type: "boolean", nullable: false),
                    EmbarazoOLactancia = table.Column<bool>(type: "boolean", nullable: false),
                    AceptoDeclaracionJurada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Triages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Triages_turnos_IdTurno",
                        column: x => x.IdTurno,
                        principalTable: "turnos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Triages_IdTurno",
                table: "Triages",
                column: "IdTurno",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Triages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "turnos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHora",
                table: "Auditorias",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
