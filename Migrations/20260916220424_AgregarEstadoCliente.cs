using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace carniceriaApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Clientes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Clientes");
        }
    }
}
