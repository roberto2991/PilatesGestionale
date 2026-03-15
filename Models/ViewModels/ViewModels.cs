using System.ComponentModel.DataAnnotations;

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
