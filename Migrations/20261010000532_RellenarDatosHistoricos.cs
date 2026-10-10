using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace carniceriaApp.Migrations
{
    /// <inheritdoc />
    public partial class RellenarDatosHistoricos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las líneas de productos de Antojitos que ya existían quedan marcadas.
            // Usa la categoría actual del producto, que es lo único disponible para el pasado.
            migrationBuilder.Sql(@"
                UPDATE DetalleVentas d
                JOIN Productos p ON p.Id = d.ProductoId
                SET d.EsAntojito = 1
                WHERE p.Categoria = 'Antojitos';");

            // Las ventas remotas no pertenecen a un solo negocio: se limpia el 0 heredado.
            // TipoOrigen: 0 = Presencial, 1 = WhatsApp, 2 = Llamada.
            migrationBuilder.Sql(@"
                UPDATE Ventas
                SET Negocio = NULL
                WHERE TipoOrigen <> 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sin reversión: los datos antiguos no se pueden reconstruir con certeza.
        }
    }
}