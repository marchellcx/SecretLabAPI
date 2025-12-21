using LabExtended.API;
using LabExtended.API.Hints;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace SecretLabAPI.Elements.Alerts;

/// <summary>
/// Extensions targeting alerts.
/// </summary>
public static class AlertExtensions
{
    /// <summary>
    /// The color of the info emoji.
    /// </summary>
    public const string InfoColor = "047a08";
    
    /// <summary>
    /// The color of the warning emoji.
    /// </summary>
    public const string WarnColor = "fc8403";

    /// <summary>
    /// The start of a message line.
    /// </summary>
    public const string TextStart = "»";

    /// <summary>
    /// The end of a message line.
    /// </summary>
    public const string TextEnd = "«";
    
    /// <summary>
    /// The prefix of a title.
    /// </summary>
    public const string TitlePrefix = "<size=30><b>﹝ ";

    /// <summary>
    /// The postfix of a title.
    /// </summary>
    public const string TitlePostfix = " ﹞</b></size>";
    
    /// <summary>
    /// The default title.
    /// </summary>
    public const string DefaultTitle = "server zpráva";
    
    /// <summary>
    /// Sends a new alert to a player.
    /// </summary>
    /// <param name="player">The player receiving the alert.</param>
    /// <param name="type">The type of the alert.</param>
    /// <param name="duration">The duration of the alert (in seconds) - must be at least one.</param>
    /// <param name="content">The content of the alert.</param>
    public static void SendAlert(this ExPlayer player, AlertType type, float duration, string? title, string content)
    {
        if (player?.ReferenceHub == null)
            return;

        if (string.IsNullOrWhiteSpace(content))
            return;

        if (duration < 1f)
            return;

        if (!player.TryGetHintElement<AlertElement>(out var alertElement))
            return;
        
        alertElement.Alerts.Add(new()
        {
            Title = title ?? DefaultTitle,
            
            Type = type,
            Content = content,
            Duration = duration
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="type"></param>
    /// <param name="duration"></param>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="overrideCurrent"></param>
    public static void SendAlert(this ExPlayer player, AlertType type, float duration, string? title, string content, bool overrideCurrent)
    {
        if (player?.ReferenceHub == null)
            return;

        if (string.IsNullOrWhiteSpace(content))
            return;

        if (duration < 1f)
            return;

        if (!player.TryGetHintElement<AlertElement>(out var alertElement))
            return;

        if (overrideCurrent)
        {
            alertElement.Alerts.Clear();

            alertElement.CurrentAlert = new()
            {
                Title = title ?? DefaultTitle,
                Type = type,
                Content = content,
                Duration = duration
            };

            alertElement.AlertTimer.Restart();
        }
        else
        {
            alertElement.Alerts.Add(new()
            {
                Title = title ?? DefaultTitle,
                Type = type,
                Content = content,
                Duration = duration
            });
        }
    }

    /// <summary>
    /// Formats an alert's content.
    /// </summary>
    /// <param name="alert">The alert to format.</param>
    /// <returns>the formatted content of the alert</returns>
    public static string FormatAlert(this AlertInfo alert)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            var content = alert.Content.Replace("\n", $" {TextEnd}\n{TextStart} ");
            var title = ProcessTitle(alert.Title);
            
            x.Append("<color=#");
            x.Append(alert.Type is AlertType.Info ? InfoColor : WarnColor);
            x.Append(">");
            
            x.Append(TitlePrefix);
            x.Append(title);
            x.Append(TitlePostfix);
            
            x.Append("</color>\n<size=25>");
            x.Append(TextStart);
            x.Append(" ");
            x.Append(content);
            x.Append(" ");
            x.Append(TextEnd);
            x.Append("</size>");
        });
    }

    // "word one" to "W O R D • O N E"
    private static string ProcessTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return title;

        var words = title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var letterGroups = new string[words.Length];

        for (var i = 0; i < words.Length; i++) 
            letterGroups[i] = words[i].ToUpperInvariant();
        
        var spacedWords = new string[words.Length];
        
        for (var i = 0; i < words.Length; i++)
            spacedWords[i] = string.Join(" ", letterGroups[i].ToCharArray());
        
        return string.Join(" • ", spacedWords);
    }
}