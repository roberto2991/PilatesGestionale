using System.ComponentModel.DataAnnotations;
using PilatesStudio.Models;

namespace PilatesStudio.Models.ViewModels;

// ─────────────────────── LOGIN ───────────────────────
public class LoginViewModel
{
    [Required(ErrorMessage = "L'email è obbligatoria")]
    [EmailAddress(ErrorMessage = "Email non valida")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La password è obbligatoria")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ricordami")]
    public bool RicordaMe { get; set; }
}

// ─────────────────────── DASHBOARD ───────────────────────
public class DashboardViewModel
{
    public int TotaleClienti { get; set; }
    public int ClientiAttivi { get; set; }
    public int NuoviClientiMese { get; set; }
    public int AbbonatiAttivi { get; set; }
    public decimal IncassoMese { get; set; }
    public decimal IncassoAnno { get; set; }
    public List<ClienteRecenteDto> UltimiClienti { get; set; } = new();
    public List<AbbonamentoScadenzaDto> AbbonatiInScadenza { get; set; } = new();
}

public class ClienteRecenteDto
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataIscrizione { get; set; }
    public bool Attivo { get; set; }
}

public class AbbonamentoScadenzaDto
{
    public int ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string TipoAbbonamento { get; set; } = string.Empty;
    public DateTime DataScadenza { get; set; }
    public int GiorniRimanenti { get; set; }
}

// ─────────────────────── CLIENTE LIST ───────────────────────
public class ClienteListViewModel
{
    public List<Cliente> Clienti { get; set; } = new();
    public string? Ricerca { get; set; }
    public bool? SoloAttivi { get; set; }
    public int PaginaCorrente { get; set; } = 1;
    public int TotalePagine { get; set; }
    public int TotaleClienti { get; set; }
}

// ─────────────────────── INSEGNANTI ───────────────────────
public class InsegnanteCreateViewModel
{
    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Il cognome è obbligatorio")]
    [MaxLength(100)]
    [Display(Name = "Cognome")]
    public string Cognome { get; set; } = "";

    [Required(ErrorMessage = "Il codice fiscale è obbligatorio")]
    [MaxLength(16)]
    [RegularExpression(@"^[A-Za-z]{6}[0-9]{2}[A-Za-z][0-9]{2}[A-Za-z][0-9]{3}[A-Za-z]$",
        ErrorMessage = "Formato codice fiscale non valido")]
    [Display(Name = "Codice Fiscale")]
    public string CodiceFiscale { get; set; } = "";

    [Required(ErrorMessage = "L'email è obbligatoria")]
    [EmailAddress(ErrorMessage = "Email non valida")]
    [MaxLength(200)]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [MaxLength(300)]
    [Display(Name = "Indirizzo di Domicilio")]
    public string? Indirizzo { get; set; }

    [MaxLength(200)]
    [Display(Name = "Titolo di Studio")]
    public string? TitoloDiStudio { get; set; }

    [Display(Name = "Stato Contrattuale")]
    public StatoContrattuale StatoContratto { get; set; } = StatoContrattuale.Bozza;
}

public class InsegnanteEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Il cognome è obbligatorio")]
    [MaxLength(100)]
    [Display(Name = "Cognome")]
    public string Cognome { get; set; } = "";

    [Required(ErrorMessage = "Il codice fiscale è obbligatorio")]
    [MaxLength(16)]
    [RegularExpression(@"^[A-Za-z]{6}[0-9]{2}[A-Za-z][0-9]{2}[A-Za-z][0-9]{3}[A-Za-z]$",
        ErrorMessage = "Formato codice fiscale non valido")]
    [Display(Name = "Codice Fiscale")]
    public string CodiceFiscale { get; set; } = "";

    [Required(ErrorMessage = "L'email è obbligatoria")]
    [EmailAddress(ErrorMessage = "Email non valida")]
    [MaxLength(200)]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [MaxLength(300)]
    [Display(Name = "Indirizzo di Domicilio")]
    public string? Indirizzo { get; set; }

    [MaxLength(200)]
    [Display(Name = "Titolo di Studio")]
    public string? TitoloDiStudio { get; set; }

    [Display(Name = "Stato Contrattuale")]
    public StatoContrattuale StatoContratto { get; set; }
}

public class InsegnanteListViewModel
{
    public List<Insegnante> Insegnanti { get; set; } = new();
    public string? Ricerca { get; set; }
    public int PaginaCorrente { get; set; } = 1;
    public int TotalePagine { get; set; }
    public int TotaleInsegnanti { get; set; }
}

// ─────────────────────── CORSI ───────────────────────

public class CorsoListViewModel
{
    public List<TipologiaCorso> Corsi { get; set; } = new();
    public string? Ricerca { get; set; }
    public bool? SoloAttivi { get; set; }
    public int PaginaCorrente { get; set; } = 1;
    public int TotalePagine { get; set; }
    public int TotaleCorsi { get; set; }
    public Dictionary<int, int> NumeroIscrittiPerCorso { get; set; } = new();
}

public class CorsiAssegnatiInsegnante
{
    public List<TipologiaCorso> Corsi { get; set; } = new();
    public Insegnante? Insegnante { get; set; }
    public Dictionary<int, int> NumeroIscrittiPerCorso { get; set; } = new();

}

public class SessioneCorsoInputModel
{
    public int GiornoSettimana { get; set; } = 1; // 1 = Lunedì
    public string OraInizio { get; set; } = "09:00";
    public string OraFine { get; set; } = "10:00";
}

public class CorsoCreateViewModel
{
    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [MaxLength(150)]
    [Display(Name = "Nome Corso")]
    public string Nome { get; set; } = "";

    [MaxLength(500)]
    [Display(Name = "Descrizione")]
    public string? Descrizione { get; set; }

    [Required(ErrorMessage = "La capacità massima è obbligatoria")]
    [Range(1, 200, ErrorMessage = "La capacità deve essere tra 1 e 200")]
    [Display(Name = "Capacità Massima")]
    public int CapacitaMax { get; set; } = 10;

    [Required(ErrorMessage = "La data di inizio è obbligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Data Inizio")]
    public DateTime DataInizio { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La data di fine è obbligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Data Fine")]
    public DateTime DataFine { get; set; } = DateTime.Today.AddMonths(3);

    [Display(Name = "Attivo")]
    public bool Attivo { get; set; } = true;

    [MaxLength(7)]
    [RegularExpression(@"^#(?:[0-9a-fA-F]{6})$", ErrorMessage = "Colore non valido (formato #RRGGBB)")]
    [Display(Name = "Colore")]
    public string Colore { get; set; } = "#3b82f6";

    public List<int> InsegnantiSelezionati { get; set; } = new();
    public List<SessioneCorsoInputModel> Sessioni { get; set; } = new();

    // Per la view: lista insegnanti disponibili
    public List<Insegnante> InsegnantiDisponibili { get; set; } = new();
}

public class CorsoEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [MaxLength(150)]
    [Display(Name = "Nome Corso")]
    public string Nome { get; set; } = "";

    [MaxLength(500)]
    [Display(Name = "Descrizione")]
    public string? Descrizione { get; set; }

    [Required(ErrorMessage = "La capacità massima è obbligatoria")]
    [Range(1, 200, ErrorMessage = "La capacità deve essere tra 1 e 200")]
    [Display(Name = "Capacità Massima")]
    public int CapacitaMax { get; set; } = 10;

    [Required(ErrorMessage = "La data di inizio è obbligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Data Inizio")]
    public DateTime DataInizio { get; set; }

    [Required(ErrorMessage = "La data di fine è obbligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Data Fine")]
    public DateTime DataFine { get; set; }

    [Display(Name = "Attivo")]
    public bool Attivo { get; set; } = true;

    [MaxLength(7)]
    [RegularExpression(@"^#(?:[0-9a-fA-F]{6})$", ErrorMessage = "Colore non valido (formato #RRGGBB)")]
    [Display(Name = "Colore")]
    public string Colore { get; set; } = "#3b82f6";

    public List<int> InsegnantiSelezionati { get; set; } = new();
    public List<SessioneCorsoInputModel> Sessioni { get; set; } = new();

    // Per la view: lista insegnanti disponibili
    public List<Insegnante> InsegnantiDisponibili { get; set; } = new();

    public int NumeroIscrittiAttuali { get; set; }
}

public class CorsoDetailsViewModel
{
    public TipologiaCorso Corso { get; set; } = null!;
    public List<IscrizioneCorso> Iscrizioni { get; set; } = new();
    public List<Cliente> ClientiIscrivibili { get; set; } = new();
    public int ClienteIdDaIscrivere { get; set; }
}

// ─────────────────────── OCCORRENZE / PRESENZE ───────────────────────

/// <summary>Riga di sintesi di una singola occorrenza nell'elenco.</summary>
public class OccorrenzaRigaViewModel
{
    public OccorrenzaCorso Occorrenza { get; set; } = null!;
    public int NumPresenti { get; set; }
    public int NumPresenzeRegistrate { get; set; }
}

public class OccorrenzeListViewModel
{
    public TipologiaCorso Corso { get; set; } = null!;
    public List<OccorrenzaRigaViewModel> Occorrenze { get; set; } = new();
    public int NumeroIscritti { get; set; }

    // Filtri
    public StatoOccorrenza? Stato { get; set; }
    public bool SoloFuture { get; set; }
}

/// <summary>Riga della lista presenze di una occorrenza (un cliente iscritto).</summary>
public class RigaPresenzaViewModel
{
    public int ClienteId { get; set; }
    public string NomeCompleto { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Presente { get; set; }
    public bool PresenzaRegistrata { get; set; }
}

public class OccorrenzaDettaglioViewModel
{
    public OccorrenzaCorso Occorrenza { get; set; } = null!;
    public TipologiaCorso Corso { get; set; } = null!;
    public List<RigaPresenzaViewModel> Partecipanti { get; set; } = new();

    public int NumPresenti => Partecipanti.Count(p => p.Presente && p.PresenzaRegistrata);
    public int NumRegistrate => Partecipanti.Count(p => p.PresenzaRegistrata);
}

// ─────────────────────── CALENDARIO ───────────────────────

public class CalendarioCorsoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Colore { get; set; } = "#3b82f6";
}

public class CalendarioViewModel
{
    /// <summary>Corsi visibili all'utente (legenda + definizione calendari Toast UI).</summary>
    public List<CalendarioCorsoDto> Corsi { get; set; } = new();

    /// <summary>Se valorizzato, il calendario è filtrato su un singolo corso.</summary>
    public int? CorsoIdFiltro { get; set; }
    public string? NomeCorsoFiltro { get; set; }
}

/// <summary>Input per la modifica di una singola occorrenza.</summary>
public class OccorrenzaEditInputModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La data è obbligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Data")]
    public DateTime Data { get; set; }

    [Required(ErrorMessage = "L'ora di inizio è obbligatoria")]
    [Display(Name = "Ora Inizio")]
    public string OraInizio { get; set; } = "09:00";

    [Required(ErrorMessage = "L'ora di fine è obbligatoria")]
    [Display(Name = "Ora Fine")]
    public string OraFine { get; set; } = "10:00";

    [MaxLength(500)]
    [Display(Name = "Note")]
    public string? Note { get; set; }
}

// ─────────────────────── PRIMO ACCESSO / PASSWORD ───────────────────────
public class ImpostaPasswordViewModel
{
    [Required(ErrorMessage = "La password è obbligatoria")]
    [MinLength(8, ErrorMessage = "Minimo 8 caratteri")]
    [MaxLength(128)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        ErrorMessage = "Deve contenere almeno: 1 maiuscola, 1 minuscola, 1 numero, 1 carattere speciale")]
    [DataType(DataType.Password)]
    [Display(Name = "Nuova Password")]
    public string NuovaPassword { get; set; } = "";

    [Required(ErrorMessage = "La conferma è obbligatoria")]
    [Compare(nameof(NuovaPassword), ErrorMessage = "Le password non coincidono")]
    [DataType(DataType.Password)]
    [Display(Name = "Conferma Password")]
    public string ConfermaPassword { get; set; } = "";
}
