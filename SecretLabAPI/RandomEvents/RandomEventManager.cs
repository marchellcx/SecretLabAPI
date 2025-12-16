using LabExtended.API;
using LabExtended.API.Custom.Gamemodes;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Utilities;

using SecretLabAPI.Actions;
using SecretLabAPI.Actions.API;

using SecretLabAPI.RandomEvents.Features.ItsRainingCoins;
using SecretLabAPI.RandomEvents.Features.RandomScale;

namespace SecretLabAPI.RandomEvents
{
    /// <summary>
    /// Manages the loading and execution of random events.
    /// </summary>
    public static class RandomEventManager
    {
        private static CustomGamemode? previousEvent = null;
        
        private static Dictionary<string, Type> baseEvents = new ()
        {
            { "its_raining_coins", typeof(ItsRainingCoinsEvent) },
            { "random_scale", typeof(RandomScaleEvent) },
        };
        
        public static RandomEventConfig Config { get; private set; }

        private static void OnRoundStart()
        {
            if (Config.EventWeight <= 0f)
                return;
            
            if (Config.EventWeight < 100f && !WeightUtils.GetBool(Config.EventWeight))
                return;

            var randomEvent = CustomGamemode.RegisteredObjects.GetRandomWeighted(y =>
            {
                if (y.Value is RandomEventBase x)
                {
                    if (x.Weight <= 0f)
                        return 0f;

                    if (x.MinPlayers != null && ExPlayer.Count < x.MinPlayers)
                        return 0f;

                    if (x.MaxPlayers != null && ExPlayer.Count > x.MaxPlayers)
                        return 0f;

                    if (x.Weight >= 100f)
                        return 100f;

                    return x.Weight;
                }

                if (Config.Weights.TryGetValue(y.Value.Id, out var weight))
                {
                    if (weight <= 0f)
                        return 0f;

                    if (weight >= 100f)
                        return 100f;

                    return weight;
                }

                return 0f;
            });

            if (randomEvent.Value == null)
                return;

            CustomGamemode.Enable(randomEvent.Value, true);
        }

        internal static void Initialize()
        {
            var path = Path.Combine(SecretLab.RootDirectory, "random_events");
            var example = Path.Combine(path, "example.txt");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

#region ExampleFile
            File.WriteAllText(example, 
                $"# Random Events are formatted exactly the same way as normal actions, the only difference is that\n" +
                $"# they must contain two actions (one named 'Start' and the other 'Stop').\n" +
                $"# Start is called once the random event is selected\n" +
                $"# Stop is called once the random event is cancelled, either via commands or round restarting.\n" +
                $"# Random Events functions are always prefixed with the name of the event's file, for example if you name\n" +
                "# a random event file 'Blackout.txt' then all the functions in that file will be loaded as 'Blackout{Function}'\n" +
                "# for example a function named Start would become 'BlackoutStart' and you must refer to them as such in your code.\n" +
                "# Below is an example event.\n\n" +
                ";Start" +
                "GlobalBroadcast \"Blackout starts now!\"\n" +
                "FlickerLights -1\n\n" +
                "# And then the stop function (not required)." +
                ";Stop\n" +
                "FlickerLights 1");
            #endregion

            if (FileUtils.TryLoadYamlFile<RandomEventConfig>(SecretLab.RootDirectory, "random_events.yml",
                    out var config))
            {
                Config = config;
            }
            else
            {
                FileUtils.TrySaveYamlFile(SecretLab.RootDirectory, "random_events.yml", Config = new());
            }

            foreach (var pair in baseEvents)
            {
                var eventPath = Path.Combine(path, pair.Key + ".yml");

                try
                {
                    if (FileUtils.TryLoadYamlFile<RandomEventBase>(eventPath, pair.Value, out var randomEventBase))
                    {
                        randomEventBase.Register();
                    }
                    else
                    {
                        if ((randomEventBase = Activator.CreateInstance(pair.Value) as RandomEventBase) != null)
                        {
                            FileUtils.TrySaveYamlFile(eventPath, randomEventBase);

                            randomEventBase.Register();
                        }
                        else
                        {
                            ApiLog.Error("RandomEvents",
                                $"Could not construct default instance of event &3{pair.Value}&r");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApiLog.Error("RandomEvents", ex);
                }
            }

            foreach (var file in Directory.GetFiles(path, "*.txt"))
            {
                if (file == example)
                    continue;

                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var lines = File.ReadAllLines(file);
                    var actions = ActionManager.ReadFromLines(lines, null);
                    var list = new List<CompiledAction>();

                    CompiledAction? startAction = null;
                    CompiledAction? stopAction = null;

                    actions.ForEach(act =>
                    {
                        var compiled = new CompiledAction(act, Array.Empty<CompiledParameter>(), null);

                        if (string.Equals(act.Id, "Start") && startAction == null)
                            startAction = compiled;

                        if (string.Equals(act.Id, "Stop") && stopAction == null)
                            stopAction = compiled;

                        act.Id = string.Concat(name, act.Id);

                        list.Add(compiled);
                        
                        ActionManager.Actions[act.Id] = act;
                    });

                    if (startAction == null)
                    {
                        ApiLog.Warn("RandomEvents",
                            $"Event &3{name}&r could not be loaded - missing &1Start&r action!");
                        continue;
                    }

                    var gamemode = new CustomRandomEvent(name, startAction, stopAction, list);

                    gamemode.Register();

                    ApiLog.Info("RandomEvents", $"Loaded custom event &3{name}&r!");
                }
                catch (Exception ex)
                {
                    ApiLog.Error("RandomEvents", ex);
                }
            }

            ExRoundEvents.Started += OnRoundStart;
        }
    }
}