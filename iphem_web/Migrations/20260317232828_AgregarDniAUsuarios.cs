using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iphem_web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDniAUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dni",
                table: "usuarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dni",
                table: "usuarios");
        }
    }
}
