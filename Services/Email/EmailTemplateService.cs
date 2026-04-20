using System.Text.Encodings.Web;

namespace PilatesStudio.Services.Email;

public class EmailTemplateService
{
    private readonly IHttpContextAccessor _http;

    public EmailTemplateService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string GeneraEmailAttivazioneAccount(string nomeInsegnante, string tokenGrezzo)
    {
        var req = _http.HttpContext!.Request;
        var baseUrl = $"{req.Scheme}://{req.Host}";
        var link = $"{baseUrl}/Account/PrimoAccesso?token={Uri.EscapeDataString(tokenGrezzo)}";
        var nomeEncoded = HtmlEncoder.Default.Encode(nomeInsegnante);

        return $"""
        <!DOCTYPE html>
        <html lang="it">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f4;padding:40px 0">
            <tr><td align="center">
              <table width="600" cellpadding="0" cellspacing="0"
                     style="background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.1)">
                <!-- Header -->
                <tr>
                  <td style="background:#2d6a4f;padding:32px 40px;text-align:center">
                    <h1 style="margin:0;color:#ffffff;font-size:24px;font-weight:600">Studio Pilates</h1>
                    <p style="margin:8px 0 0;color:#b7e4c7;font-size:14px">Gestionale Insegnanti</p>
                  </td>
                </tr>
                <!-- Body -->
                <tr>
                  <td style="padding:40px">
                    <h2 style="margin:0 0 16px;color:#1b4332;font-size:20px">Benvenuta!</h2>
                    <p style="margin:0 0 12px;color:#333;line-height:1.6">
                      Gentile <strong>{nomeEncoded}</strong>,
                    </p>
                    <p style="margin:0 0 24px;color:#333;line-height:1.6">
                      Il tuo account per il gestionale di Studio Pilates è stato creato.
                      Clicca il pulsante qui sotto per attivarlo e impostare la tua password personale.
                    </p>
                    <div style="text-align:center;margin:32px 0">
                      <a href="{link}"
                         style="background:#2d6a4f;color:#ffffff;padding:14px 32px;
                                border-radius:6px;text-decoration:none;font-size:16px;
                                font-weight:600;display:inline-block">
                        Attiva il tuo account
                      </a>
                    </div>
                    <p style="margin:24px 0 8px;color:#555;font-size:13px;line-height:1.5">
                      Se il pulsante non funziona, copia e incolla questo link nel browser:
                    </p>
                    <p style="margin:0 0 24px;word-break:break-all">
                      <a href="{link}" style="color:#2d6a4f;font-size:12px">{link}</a>
                    </p>
                    <div style="background:#fff3cd;border-left:4px solid #ffc107;padding:12px 16px;border-radius:4px">
                      <p style="margin:0;color:#856404;font-size:13px">
                        Questo link è valido per <strong>72 ore</strong> ed è utilizzabile una sola volta.
                        Se scade, contatta l'amministratore per richiedere un nuovo invito.
                      </p>
                    </div>
                  </td>
                </tr>
                <!-- Footer -->
                <tr>
                  <td style="background:#f8f9fa;padding:20px 40px;border-top:1px solid #e9ecef;text-align:center">
                    <p style="margin:0;color:#6c757d;font-size:12px">
                      Se non ti aspettavi questa email, ignorala — nessuna azione è richiesta.
                    </p>
                    <p style="margin:8px 0 0;color:#6c757d;font-size:12px">
                      © {DateTime.Now.Year} Studio Pilates
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
    }
}
