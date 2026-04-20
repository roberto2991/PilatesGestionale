using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PilatesStudio.Migrations
{
    /// <inheritdoc />
    public partial class GestioneInsegnati : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailNotificaLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Destinatario = table.Column<string>(type: "text", nullable: false),
                    Oggetto = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    ErroreDettaglio = table.Column<string>(type: "text", nullable: true),
                    TentativiEffettuati = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    InviatoAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotificaLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insegnanti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodiceFiscale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Indirizzo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TitoloDiStudio = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StatoContratto = table.Column<string>(type: "text", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: true),
                    AccountAttivato = table.Column<bool>(type: "boolean", nullable: false),
                    DataAttivazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UltimoAggiornamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insegnanti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Insegnanti_Utenti_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TokenAttivazioneAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ScadenzaUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Utilizzato = table.Column<bool>(type: "boolean", nullable: false),
                    DataUtilizzo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenAttivazioneAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenAttivazioneAccount_Utenti_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Insegnanti_ApplicationUserId",
                table: "Insegnanti",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Insegnanti_CodiceFiscale",
                table: "Insegnanti",
                column: "CodiceFiscale",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Insegnanti_Email",
                table: "Insegnanti",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenAttivazioneAccount_ApplicationUserId_Utilizzato",
                table: "TokenAttivazioneAccount",
                columns: new[] { "ApplicationUserId", "Utilizzato" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenAttivazioneAccount_TokenHash",
                table: "TokenAttivazioneAccount",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailNotificaLog");

            migrationBuilder.DropTable(
                name: "Insegnanti");

            migrationBuilder.DropTable(
                name: "TokenAttivazioneAccount");
        }
    }
}
