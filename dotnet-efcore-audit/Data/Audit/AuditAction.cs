namespace Unctad.eRegulations.Library.Data.Audit;

public enum AuditAction : byte
{
    Init,
    Insert,
    Update,
    Delete,
    Translate,
    MovedToRecycleBin
}
