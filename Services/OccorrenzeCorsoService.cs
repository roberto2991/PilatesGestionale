using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;

namespace PilatesStudio.Services;

/// <summary>
/// Genera e mantiene allineate le <see cref="OccorrenzaCorso"/> (sessioni datate) di un corso
/// a partire dalle <see cref="SessioneCorso"/> (template settimanali ricorrenti) e dal periodo
/// del corso. Pensato per preservare lo storico: non tocca mai occorrenze passate o con presenze.
/// </summary>
public class OccorrenzeCorsoService
{
    private readonly ApplicationDbContext _db;

    public OccorrenzeCorsoService(ApplicationDbContext db) => _db = db;

    public readonly record struct OccorrenzaSlot(DateTime Data, TimeSpan OraInizio, TimeSpan OraFine);

    /// <summary>
    /// Calcola, in memoria, tutti gli slot (data + orario) previsti dalle sessioni settimanali
    /// nell'intervallo [dataInizio, dataFine] (estremi inclusi).
    /// </summary>
    public static List<OccorrenzaSlot> CalcolaSlot(
        DateTime dataInizio, DateTime dataFine, IEnumerable<SessioneCorso> sessioni)
    {
        var sessioniList = sessioni.ToList();
        var slots = new List<OccorrenzaSlot>();

        for (var giorno = dataInizio.Date; giorno <= dataFine.Date; giorno = giorno.AddDays(1))
        {
            foreach (var s in sessioniList.Where(x => x.GiornoSettimana == giorno.DayOfWeek))
                slots.Add(new OccorrenzaSlot(giorno, s.OraInizio, s.OraFine));
        }

        return slots;
    }

    /// <summary>
    /// Sincronizza le occorrenze del corso col calendario settimanale corrente, preservando lo storico:
    /// <list type="bullet">
    /// <item>aggiunge le occorrenze mancanti previste dal calendario;</item>
    /// <item>rimuove SOLO le occorrenze future, prive di presenze e non annullate, che non sono più previste.</item>
    /// </list>
    /// Le occorrenze passate, annullate o con presenze registrate non vengono mai toccate.
    /// Non chiama SaveChanges: è responsabilità del chiamante, così da restare nella stessa transazione.
    /// </summary>
    /// <returns>Numero di occorrenze aggiunte.</returns>
    public async Task<int> SincronizzaAsync(
        int corsoId, DateTime dataInizio, DateTime dataFine, DateTime oggi)
    {
        var sessioni = await _db.SessioniCorso
            .Where(s => s.TipologiaCorsoId == corsoId)
            .ToListAsync();

        var occorrenze = await _db.OccorrenzeCorso
            .Where(o => o.TipologiaCorsoId == corsoId)
            .Select(o => new
            {
                o.Id,
                o.Data,
                o.OraInizio,
                o.Stato,
                NumPresenze = o.Presenze.Count
            })
            .ToListAsync();

        var slots = CalcolaSlot(dataInizio, dataFine, sessioni);
        var slotKeys = slots.Select(s => (s.Data.Date, s.OraInizio)).ToHashSet();
        var existingKeys = occorrenze.Select(o => (o.Data.Date, o.OraInizio)).ToHashSet();

        // 1) Aggiunte: slot previsti non ancora presenti
        var aggiunte = 0;
        foreach (var slot in slots)
        {
            if (existingKeys.Add((slot.Data.Date, slot.OraInizio)))
            {
                _db.OccorrenzeCorso.Add(new OccorrenzaCorso
                {
                    TipologiaCorsoId = corsoId,
                    Data = slot.Data.Date,
                    OraInizio = slot.OraInizio,
                    OraFine = slot.OraFine,
                    Stato = StatoOccorrenza.Programmata
                });
                aggiunte++;
            }
        }

        // 2) Rimozioni SICURE: future, senza presenze, non annullate, non più previste
        var idDaRimuovere = occorrenze
            .Where(o => o.Data.Date >= oggi.Date
                     && o.NumPresenze == 0
                     && o.Stato != StatoOccorrenza.Annullata
                     && !slotKeys.Contains((o.Data.Date, o.OraInizio)))
            .Select(o => o.Id)
            .ToList();

        if (idDaRimuovere.Count > 0)
        {
            var entita = await _db.OccorrenzeCorso
                .Where(o => idDaRimuovere.Contains(o.Id))
                .ToListAsync();
            _db.OccorrenzeCorso.RemoveRange(entita);
        }

        return aggiunte;
    }
}
