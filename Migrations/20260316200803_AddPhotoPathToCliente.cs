using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilatesStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoPathToCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoProfiloPath",
                table: "Clienti",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoProfiloPath",
                table: "Clienti");
        }
    }
}
