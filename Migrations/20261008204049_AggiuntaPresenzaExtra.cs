using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilatesStudio.Migrations
{
    /// <inheritdoc />
    public partial class AggiuntaPresenzaExtra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExtra",
                table: "PresenzeCorso",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExtra",
                table: "PresenzeCorso");
        }
    }
}
