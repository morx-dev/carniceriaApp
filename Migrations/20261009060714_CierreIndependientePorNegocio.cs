using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace carniceriaApp.Migrations
{
    /// <inheritdoc />
    public partial class CierreIndependientePorNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CuadresDiarios_Fecha",
                table: "CuadresDiarios");

            migrationBuilder.AddColumn<int>(
                name: "Negocio",
                table: "CuadresDiarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CuadresDiarios_Fecha_Negocio",
                table: "CuadresDiarios",
                columns: new[] { "Fecha", "Negocio" },
                unique: true);

            // Los cuadres antiguos guardaban ambos negocios en una sola fila:
            // se crea una fila de Antojitos por cada uno y se limpia la de Carnicería.
            migrationBuilder.Sql(@"
                INSERT INTO CuadresDiarios
                    (Fecha, UsuarioId, TotalVentasPresenciales, TotalVentasSistema, TotalGeneral,
                     TotalAntojitosPresenciales, TotalAntojitosSistema, TotalAntojitosGeneral,
                     FechaCreacion, Negocio)
                SELECT Fecha, UsuarioId, TotalAntojitosPresenciales, TotalAntojitosSistema, TotalAntojitosGeneral,
                       0, 0, 0, FechaCreacion, 1
                FROM CuadresDiarios
                WHERE Negocio = 0;");

            migrationBuilder.Sql(@"
                UPDATE CuadresDiarios
                SET TotalAntojitosPresenciales = 0,
                    TotalAntojitosSistema = 0,
                    TotalAntojitosGeneral = 0
                WHERE Negocio = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CuadresDiarios_Fecha_Negocio",
                table: "CuadresDiarios");

            migrationBuilder.DropColumn(
                name: "Negocio",
                table: "CuadresDiarios");

            migrationBuilder.CreateIndex(
                name: "IX_CuadresDiarios_Fecha",
                table: "CuadresDiarios",
                column: "Fecha",
                unique: true);
        }
    }
}