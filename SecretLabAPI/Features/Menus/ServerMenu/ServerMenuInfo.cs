using System.ComponentModel;
using System.Diagnostics;

namespace SecretLabAPI.Features.Menus.ServerMenu
{
    /// <summary>
    /// Represents the configuration information for a server, including its port number, alias, and description.
    /// </summary>
    public class ServerMenuInfo
    {
        internal bool isRunning;
        internal bool isChecked;

        internal Stopwatch watch = new();

        /// <summary>
        /// Gets or sets the port number used by the server.
        /// </summary>
        [Description("Sets the port of the server - must be on the same IP as this server!")]
        public ushort Port { get; set; } = 0;

        /// <summary>
        /// Gets or sets the alias name used to identify the server.
        /// </summary>
        [Description("Sets the name alias of the server.")]
        public string Alias { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the description of the server.
        /// </summary>
        [Description("Sets the description of the server.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the server's screen session.
        /// </summary>
        [Description("Sets the name of the server's screen session.")]
        public string Screen { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command used to start the server.
        /// </summary>
        [Description("Sets the command used to start the server.")]
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the permission required to start the server.
        /// </summary>
        [Description("Sets the permission required to start the server.")]
        public string? Permission { get; set; } = null;
    }
}