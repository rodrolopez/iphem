using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace iphem_web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. ESTO ES LO ÚNICO QUE QUEDA ACTIVO: Crear la tabla Auditorias
            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    Accion = table.Column<string>(type: "text", nullable: false),
                    Tabla = table.Column<string>(type: "text", nullable: false),
                    ValoresAntiguos = table.Column<string>(type: "text", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "text", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                });

            // =======================================================================
            // 2. COMENTAMOS TODO EL RESTO PARA QUE POSTGRESQL NO TIRE ERROR
            // =======================================================================
            /*
            migrationBuilder.CreateTable(
                name: "carrusel",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "text", nullable: true),
                    subtitulo = table.Column<string>(type: "text", nullable: true),
                    imagenpath = table.Column<string>(type: "text", nullable: false),
                    enlace = table.Column<string>(type: "text", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrusel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracionturnos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    horainicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    cantidadturnos = table.Column<int>(type: "integer", nullable: false),
                    intervalominutos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracionturnos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estadosturno",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("estadosturno_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tiposturno",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tiposturno_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombrecompleto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    idrol = table.Column<int>(type: "integer", nullable: false),
                    fechaalta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuarios_pkey", x => x.id);
                    table.ForeignKey(
                        name: "usuarios_idrol_fkey",
                        column: x => x.idrol,
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "turnos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fechahora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    dnisolicitante = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombresolicitante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidosolicitante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gruposanguineo = table.Column<string>(type: "text", nullable: true),
                    departamento = table.Column<string>(type: "text", nullable: true),
                    emailsolicitante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    telefonosolicitante = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pacientedestino = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    idtipoturno = table.Column<int>(type: "integer", nullable: false),
                    idestadoturno = table.Column<int>(type: "integer", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("turnos_pkey", x => x.id);
                    table.ForeignKey(
                        name: "turnos_idestadoturno_fkey",
                        column: x => x.idestadoturno,
                        principalTable: "estadosturno",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "turnos_idtipoturno_fkey",
                        column: x => x.idtipoturno,
                        principalTable: "tiposturno",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "noticias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subtitulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    epigrafe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cuerpo = table.Column<string>(type: "text", nullable: false),
                    imagenpath = table.Column<string>(type: "text", nullable: true),
                    imagenurl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    piedefoto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fechapublicacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    idusuariocreador = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("noticias_pkey", x => x.id);
                    table.ForeignKey(
                        name: "noticias_idusuariocreador_fkey",
                        column: x => x.idusuariocreador,
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_noticias_idusuariocreador",
                table: "noticias",
                column: "idusuariocreador");

            migrationBuilder.CreateIndex(
                name: "roles_nombre_key",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_turnos_idestadoturno",
                table: "turnos",
                column: "idestadoturno");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_idtipoturno",
                table: "turnos",
                column: "idtipoturno");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_idrol",
                table: "usuarios",
                column: "idrol");

            migrationBuilder.CreateIndex(
                name: "usuarios_email_key",
                table: "usuarios",
                column: "email",
                unique: true);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // En el Down también dejamos solo la eliminación de Auditorias
            migrationBuilder.DropTable(
                name: "Auditorias");

            /*
            migrationBuilder.DropTable(
                name: "carrusel");

            migrationBuilder.DropTable(
                name: "configuracionturnos");

            migrationBuilder.DropTable(
                name: "noticias");

            migrationBuilder.DropTable(
                name: "turnos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "estadosturno");

            migrationBuilder.DropTable(
                name: "tiposturno");

            migrationBuilder.DropTable(
                name: "roles");
            */
        }
    }
}