namespace Data.Audit;

/// <summary>
/// Marker interface for entities that are child collection items owned by an IAuditable parent.
/// These entities do not carry ModifiedDate/ModifiedUser; audit metadata is sourced from the parent.
/// Audit records are created through the parent entity's audit processing.
/// </summary>
public interface IAuditableCollection
{
    int Id { get; set; }
}
