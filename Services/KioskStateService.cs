namespace PilatesStudio.Services;

/// <summary>
/// Singleton che mantiene lo stato del kiosk iPad.
/// Contiene l'Id del cliente il cui contratto è in attesa di firma.
/// </summary>
public class KioskStateService
{
    private int? _pendingClienteId;
    private int? _lastSignedClienteId;
    private readonly object _lock = new();

    public int? PendingClienteId
    {
        get { lock (_lock) return _pendingClienteId; }
    }

    /// <summary>Id dell'ultimo cliente che ha firmato il contratto sul kiosk.</summary>
    public int? LastSignedClienteId
    {
        get { lock (_lock) return _lastSignedClienteId; }
    }

    public void SetCliente(int clienteId)
    {
        lock (_lock) _pendingClienteId = clienteId;
    }

    /// <summary>Registra che il cliente ha completato la firma.</summary>
    public void SetFirmato(int clienteId)
    {
        lock (_lock) _lastSignedClienteId = clienteId;
    }

    public void Clear()
    {
        lock (_lock) _pendingClienteId = null;
    }
}
