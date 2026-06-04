using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PilatesStudio.Migrations
{
    /// <inheritdoc />
    public partial class GestioneOccorenzeCorsi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Archiviato",
                table: "TipologieCorsi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataArchiviazione",
                table: "TipologieCorsi",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OccorrenzeCorso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipologiaCorsoId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OraInizio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    OraFine = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Stato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataAnnullamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MotivoAnnullamento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccorrenzeCorso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccorrenzeCorso_TipologieCorsi_TipologiaCorsoId",
                        column: x => x.TipologiaCorsoId,
                        principalTable: "TipologieCorsi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PresenzeCorso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OccorrenzaCorsoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Presente = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DataRegistrazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RegistrataDa = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresenzeCorso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresenzeCorso_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PresenzeCorso_OccorrenzeCorso_OccorrenzaCorsoId",
                        column: x => x.OccorrenzaCorsoId,
                        principalTable: "OccorrenzeCorso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OccorrenzeCorso_TipologiaCorsoId_Data_OraInizio",
                table: "OccorrenzeCorso",
                columns: new[] { "TipologiaCorsoId", "Data", "OraInizio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PresenzeCorso_ClienteId",
                table: "PresenzeCorso",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenzeCorso_OccorrenzaCorsoId_ClienteId",
                table: "PresenzeCorso",
                columns: new[] { "OccorrenzaCorsoId", "ClienteId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresenzeCorso");

            migrationBuilder.DropTable(
                name: "OccorrenzeCorso");

            migrationBuilder.DropColumn(
                name: "Archiviato",
                table: "TipologieCorsi");

            migrationBuilder.DropColumn(
                name: "DataArchiviazione",
                table: "TipologieCorsi");
        }
    }
}
