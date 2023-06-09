using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriadorDeCaes.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalMigrationAnimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecoCompraAux",
                table: "Animais");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrecoCompraAux",
                table: "Animais",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
