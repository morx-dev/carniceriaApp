using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace carniceriaApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAntojitosAlCuadre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAntojitosGeneral",
                table: "CuadresDiarios",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAntojitosPresenciales",
                table: "CuadresDiarios",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAntojitosSistema",
                table: "CuadresDiarios",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAntojitosGeneral",
                table: "CuadresDiarios");

            migrationBuilder.DropColumn(
                name: "TotalAntojitosPresenciales",
                table: "CuadresDiarios");

            migrationBuilder.DropColumn(
                name: "TotalAntojitosSistema",
                table: "CuadresDiarios");
        }
    }
}
