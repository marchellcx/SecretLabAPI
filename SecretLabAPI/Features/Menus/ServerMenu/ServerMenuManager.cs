using LabExtended.API;
using LabExtended.API.Settings;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Utilities;
using LabExtended.Utilities.Update;

using SecretLabAPI.Extensions;
using SecretLabAPI.Utilities;

using System.Diagnostics;

namespace SecretLabAPI.Features.Menus.ServerMenu
{
    /// <summary>
    /// Manages server-list menus.
    /// </summary>
    public static class ServerMenuManager
    {
        /// <summary>
        /// Represents the interval, in seconds, at which availability checks are performed.
        /// </summary>
        public const float CheckInterval = 3f;

        /// <summary>
        /// Represents the delay, in seconds, between each update call.
        /// </summary>
        public const float UpdateInterval = 1.5f;

        private static PlayerUpdateComponent update = PlayerUpdateComponent.Create();
        private static Stopwatch watch = new();

        /// <summary>
        /// Gets a list of all valid configured servers.
        /// </summary>
        public static List<ServerMenuInfo> Servers { get; } = new();

        /// <summary>
        /// Gets a list of all servers keyed by their alias.
        /// </summary>
        public static Dictionary<string, ServerMenuInfo> ServersByAlias { get; } = new();

        /// <summary>
        /// Stops the specified server if it is currently running.
        /// </summary>
        /// <remarks>The method checks whether the specified server is part of the known servers and is
        /// currently running before attempting to stop it. If the server is not found or is not running, the method
        /// returns false and no action is taken.</remarks>
        /// <param name="server">The server to stop. This parameter must not be null.</param>
        /// <returns>true if the server was successfully stopped; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the server parameter is null.</exception>
        public static bool StopServer(ServerMenuInfo server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            if (!Servers.Contains(server))
                return false;

            if (!server.isRunning)
                return false;

            LinuxUtils.Execute($"screen -S {server.Screen} -X quit");
            return true;
        }

        /// <summary>
        /// Starts the specified server if it is not already running and is registered.
        /// </summary>
        /// <remarks>The method does not start the server if it is already running or if it is not part of
        /// the registered servers.</remarks>
        /// <param name="server">The server to start. Must not be null and must be included in the registered servers list.</param>
        /// <returns>true if the server was successfully started; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the server parameter is null.</exception>
        public static bool StartServer(ServerMenuInfo server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            if (!Servers.Contains(server))
                return false;

            if (server.isRunning)
                return false;

            LinuxUtils.Execute(server.Command);
            return true;
        }

        private static void CheckServer(ServerMenuInfo server)
        {
            LinuxUtils.Execute("screen -ls", output =>
            {
                var running = output.Contains(server.Screen);

                if (server.isChecked && running == server.isRunning)
                    return;

                server.isRunning = running;
                server.isChecked = true;

                if (running)
                {
                    ApiLog.Info($"Updated status of server &3{server.Alias}&r to &2ONLINE&r");
                }
                else
                {
                    ApiLog.Warn($"Updated status of server &3{server.Alias}&r to &1OFFLINE&r");
                }
            });
        }

        private static void OnUpdate()
        {
            if (watch.Elapsed.TotalSeconds < UpdateInterval)
                return;

            watch.Restart();

            foreach (var server in Servers)
            {
                if (server.watch.Elapsed.TotalSeconds < CheckInterval)
                    continue;

                server.watch.Restart();

                CheckServer(server);
            }

            foreach (var player in ExPlayer.Players)
            {
                if (!player.IsValidPlayer())
                    continue;
                
                if (!player.TryGetMenu<ServerMenuInstance>(out var menu))
                    continue;

                menu.SyncMenu();
            }
        }

        private static void OnVerified(ExPlayer player)
        {
            TimingUtils.AfterSeconds(() =>
            {
                player.AddMenu(new ServerMenuInstance());
            }, 2f);
        }

        internal static void Initialize()
        {
            var servers = FileUtils.LoadYamlFileOrDefault<List<ServerMenuInfo>>(SecretLab.RootDirectory, "server_menu.yml", new() 
            { 
                new(),
                new() 
            }, true);

            foreach (var server in servers)
            {
                if (server.Port == 0)
                    continue;

                if (string.IsNullOrEmpty(server.Alias))
                    continue;

                if (string.IsNullOrEmpty(server.Description))
                    continue;

                if (string.IsNullOrEmpty(server.Screen))
                    continue;

                if (string.IsNullOrEmpty(server.Command))
                    continue;

                Servers.Add(server);
                ServersByAlias.Add(server.Alias, server);

                server.watch.Restart();
            }

            if (Servers.Count == 0)
                return;

            watch.Restart();

            update.OnLateUpdate += OnUpdate;

            ExPlayerEvents.Verified += OnVerified;
        }
    }
}