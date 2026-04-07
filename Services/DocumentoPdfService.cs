using PilatesStudio.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PilatesStudio.Services;

public class DocumentoPdfService
{
    private readonly IWebHostEnvironment _env;

    public DocumentoPdfService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Genera il PDF di iscrizione precompilato e lo salva su disco.
    /// Restituisce il path relativo web (es. /uploads/clienti/contratti/guid.pdf).
    /// Se <paramref name="firmaRelativePath"/> è valorizzato, incorpora l'immagine della firma nel PDF.
    /// </summary>
    public string GeneraContratto(Cliente cliente, string? firmaRelativePath = null)
    {
        var cartella = Path.Combine(_env.WebRootPath, "uploads", "clienti", "contratti");
        Directory.CreateDirectory(cartella);

        var nomeFile = $"contratto_{cliente.Id}_{Guid.NewGuid():N}.pdf";
        var percorsoAssoluto = Path.Combine(cartella, nomeFile);

        string? firmaAssoluta = null;
        if (!string.IsNullOrEmpty(firmaRelativePath))
        {
            firmaAssoluta = Path.Combine(
                _env.WebRootPath,
                firmaRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(firmaAssoluta)) firmaAssoluta = null;
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, cliente, firmaAssoluta));
                page.Footer().Element(ComposeFooter);
            });
        });

        document.GeneratePdf(percorsoAssoluto);

        return $"/uploads/clienti/contratti/{nomeFile}";
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("STUDIO PILATES")
                        .FontSize(22).Bold().FontColor("#6b4c9a");
                    c.Item().Text("Centro Benessere & Movimento")
                        .FontSize(10).FontColor("#888888");
                });
                row.ConstantItem(120).AlignRight().Column(c =>
                {
                    c.Item().Text("MODULO DI ISCRIZIONE")
                        .FontSize(9).Bold().FontColor("#6b4c9a").AlignRight();
                    c.Item().Text($"Data: {DateTime.Today:dd/MM/yyyy}")
                        .FontSize(9).FontColor("#888888").AlignRight();
                });
            });

            col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor("#6b4c9a");
        });
    }

    private void ComposeContent(IContainer container, Cliente cliente, string? firmaAssoluta = null)
    {
        container.PaddingTop(16).Column(col =>
        {
            // ── Titolo sezione dati anagrafici ──────────────────────────
            col.Item().PaddingBottom(8).Text("DATI ANAGRAFICI")
                .FontSize(11).Bold().FontColor("#6b4c9a");

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                });

                void Cella(string label, string? valore)
                {
                    table.Cell().PaddingBottom(10).Column(c =>
                    {
                        c.Item().Text(label).FontSize(7.5f).FontColor("#888888").Bold();
                        c.Item().PaddingTop(2).Text(string.IsNullOrEmpty(valore) ? "—" : valore)
                            .FontSize(10);
                        c.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor("#cccccc");
                    });
                }

                Cella("Nome", cliente.Nome);
                Cella("Cognome", cliente.Cognome);
                Cella("Data di Nascita", cliente.DataNascita?.ToString("dd/MM/yyyy"));
                Cella("Codice Fiscale", cliente.CodiceFiscale);
                Cella("Email", cliente.Email);
                Cella("Telefono", cliente.Telefono);
                Cella("Città", cliente.Citta);
                Cella("CAP", cliente.Cap);

                // Indirizzo su riga intera (span manuale: 4 celle separate)
                table.Cell().PaddingBottom(10).Column(c =>
                {
                    c.Item().Text("Indirizzo").FontSize(7.5f).FontColor("#888888").Bold();
                    c.Item().PaddingTop(2).Text(string.IsNullOrEmpty(cliente.Indirizzo) ? "—" : cliente.Indirizzo)
                        .FontSize(10);
                    c.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor("#cccccc");
                });
                // Riempi le restanti 3 celle vuote per allineamento
                table.Cell().Text("");
                table.Cell().Text("");
                table.Cell().Text("");
            });

            col.Item().PaddingTop(4).PaddingBottom(12).LineHorizontal(0.5f).LineColor("#dddddd");

            // ── Note ────────────────────────────────────────────────────
            if (!string.IsNullOrEmpty(cliente.Note))
            {
                col.Item().PaddingBottom(6).Text("NOTE").FontSize(11).Bold().FontColor("#6b4c9a");
                col.Item().PaddingBottom(12).Text(cliente.Note).FontSize(9).FontColor("#555555");
                col.Item().PaddingBottom(12).LineHorizontal(0.5f).LineColor("#dddddd");
            }

            // ── Informativa Privacy ──────────────────────────────────────
            col.Item().PaddingBottom(8).Text("INFORMATIVA PRIVACY (art. 13 GDPR 2016/679)")
                .FontSize(11).Bold().FontColor("#6b4c9a");

            col.Item().PaddingBottom(12).Text(
                "I dati personali raccolti verranno trattati esclusivamente per la gestione del rapporto " +
                "contrattuale con lo studio, per l'invio di comunicazioni relative ai servizi offerti e per " +
                "adempiere agli obblighi di legge. Il titolare del trattamento è Studio Pilates. " +
                "I dati non saranno ceduti a terzi senza consenso esplicito. " +
                "L'interessato ha diritto di accesso, rettifica, cancellazione e opposizione ai sensi del GDPR."
            ).FontSize(8.5f).FontColor("#555555");

            col.Item().PaddingBottom(16).LineHorizontal(0.5f).LineColor("#dddddd");

            // ── Consenso e firma ────────────────────────────────────────
            col.Item().PaddingBottom(8).Text("CONSENSO E FIRMA")
                .FontSize(11).Bold().FontColor("#6b4c9a");

            col.Item().PaddingBottom(6).Text(
                "Il/La sottoscritto/a, presa visione dell'informativa sopra riportata, " +
                "acconsente al trattamento dei propri dati personali per le finalità indicate " +
                "e dichiara di aver letto e accettato le condizioni di iscrizione allo Studio Pilates."
            ).FontSize(9).FontColor("#333333");

            col.Item().PaddingTop(24).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Luogo e data: ________________________  {DateTime.Today:dd/MM/yyyy}")
                        .FontSize(9);
                });
                row.RelativeItem().Column(c =>
                {
                    if (firmaAssoluta != null)
                    {
                        c.Item().AlignRight().Text("Firma del cliente:").FontSize(9);
                        c.Item().AlignRight().Width(160).Height(65).Image(firmaAssoluta);
                    }
                    else
                    {
                        c.Item().Text("Firma del cliente: ________________________")
                            .FontSize(9).AlignRight();
                    }
                });
            });

            col.Item().PaddingTop(36).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Timbro e firma dello Studio:")
                        .FontSize(9).FontColor("#888888");
                    c.Item().PaddingTop(40).LineHorizontal(0.5f).LineColor("#cccccc");
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Studio Pilates  ·  Modulo di iscrizione generato il ")
                    .FontSize(7).FontColor("#aaaaaa");
                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    .FontSize(7).FontColor("#aaaaaa");
            });
            row.ConstantItem(60).AlignRight().Text(text =>
            {
                text.Span("Pag. ").FontSize(7).FontColor("#aaaaaa");
                text.CurrentPageNumber().FontSize(7).FontColor("#aaaaaa");
                text.Span(" / ").FontSize(7).FontColor("#aaaaaa");
                text.TotalPages().FontSize(7).FontColor("#aaaaaa");
            });
        });
    }

    /// <summary>Elimina il file immagine firma dal disco se esiste.</summary>
    public void EliminaFirma(string? relativePath) => EliminaFile(relativePath);

    /// <summary>Elimina il file PDF dal disco se esiste.</summary>
    public void EliminaContratto(string? relativePath) => EliminaFile(relativePath);

    private void EliminaFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var assoluto = Path.Combine(
            _env.WebRootPath,
            relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(assoluto))
            File.Delete(assoluto);
    }
}
