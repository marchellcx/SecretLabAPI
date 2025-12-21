namespace SecretLabAPI.Elements.Alerts;

/// <summary>
/// A created alert.
/// </summary>
public class AlertInfo
{
    /// <summary>
    /// The title of the alert.
    /// </summary>
    public string Title { get; set; } = "Ｓ E R VＥ R • Z P R Á V A";

    /// <summary>
    /// The content of the alert.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The formatted content of the alert.
    /// </summary>
    public string FormattedContent { get; set; } = string.Empty;

    /// <summary>
    /// The type of the alert.
    /// </summary>
    public AlertType Type { get; set; } = AlertType.Info;

    /// <summary>
    /// The duration of the alert.
    /// </summary>
    public float Duration { get; set; } = 0f;
}