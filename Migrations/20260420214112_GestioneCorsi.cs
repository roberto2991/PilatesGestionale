using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PilatesStudio.Migrations
{
    /// <inheritdoc />
    public partial class GestioneCorsi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipologieCorsi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CapacitaMax = table.Column<int>(type: "integer", nullable: false),
                    DataInizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataFine = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UltimoAggiornamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipologieCorsi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IscrizioniCorso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipologiaCorsoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    DataIscrizione = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IscrizioniCorso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IscrizioniCorso_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IscrizioniCorso_TipologieCorsi_TipologiaCorsoId",
                        column: x => x.TipologiaCorsoId,
                        principalTable: "TipologieCorsi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessioniCorso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipologiaCorsoId = table.Column<int>(type: "integer", nullable: false),
                    GiornoSettimana = table.Column<int>(type: "integer", nullable: false),
                    OraInizio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    OraFine = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessioniCorso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessioniCorso_TipologieCorsi_TipologiaCorsoId",
                        column: x => x.TipologiaCorsoId,
                        principalTable: "TipologieCorsi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TipologieCorsoInsegnanti",
                columns: table => new
                {
                    TipologiaCorsoId = table.Column<int>(type: "integer", nullable: false),
                    InsegnanteId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipologieCorsoInsegnanti", x => new { x.TipologiaCorsoId, x.InsegnanteId });
                    table.ForeignKey(
                        name: "FK_TipologieCorsoInsegnanti_Insegnanti_InsegnanteId",
                        column: x => x.InsegnanteId,
                        principalTable: "Insegnanti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TipologieCorsoInsegnanti_TipologieCorsi_TipologiaCorsoId",
                        column: x => x.TipologiaCorsoId,
                        principalTable: "TipologieCorsi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IscrizioniCorso_ClienteId",
                table: "IscrizioniCorso",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_IscrizioniCorso_TipologiaCorsoId_ClienteId",
                table: "IscrizioniCorso",
                columns: new[] { "TipologiaCorsoId", "ClienteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessioniCorso_TipologiaCorsoId",
                table: "SessioniCorso",
                column: "TipologiaCorsoId");

            migrationBuilder.CreateIndex(
                name: "IX_TipologieCorsoInsegnanti_InsegnanteId",
                table: "TipologieCorsoInsegnanti",
                column: "InsegnanteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IscrizioniCorso");

            migrationBuilder.DropTable(
                name: "SessioniCorso");

            migrationBuilder.DropTable(
                name: "TipologieCorsoInsegnanti");

            migrationBuilder.DropTable(
                name: "TipologieCorsi");
        }
    }
}
