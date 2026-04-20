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
