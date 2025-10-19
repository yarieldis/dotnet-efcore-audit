namespace Data.Audit.Configuration;

/// <summary>
/// Configuration settings for the audit system.
/// </summary>
public class AuditConfiguration
{
    /// <summary>
    /// Gets or sets whether sensitive data should be masked in audit logs.
    /// Default is true.
    /// </summary>
    public bool EnableSensitiveDataMasking { get; set; } = true;

    /// <summary>
    /// Gets or sets whether field-level auditing is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableFieldLevelAuditing { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to continue processing other fields when a field processing error occurs.
    /// Default is true.
    /// </summary>
    public bool ContinueOnFieldProcessingError { get; set; } = true;

    /// <summary>
    /// Gets or sets the default value used to mask sensitive data.
    /// Default is "***MASKED***".
    /// </summary>
    public string DefaultMaskValue { get; set; } = "***MASKED***";

    /// <summary>
    /// Gets or sets the default user name when no user is specified.
    /// Default is "System".
    /// </summary>
    public string DefaultSystemUser { get; set; } = "System";

    /// <summary>
    /// Gets or sets whether to enable detailed logging for audit operations.
    /// Default is false.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}