using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilatesStudio.Migrations
{
    /// <inheritdoc />
    public partial class aggiuntaColoreCorsi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Colore",
                table: "TipologieCorsi",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colore",
                table: "TipologieCorsi");
        }
    }
}
