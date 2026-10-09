using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace carniceriaApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNegocioVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Negocio",
                table: "Ventas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Negocio",
                table: "Ventas");
        }
    }
}
