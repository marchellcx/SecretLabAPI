using System.Diagnostics;
using System.Reflection;

using LabApi.Events;
using LabApi.Loader;

using LabExtended.Core;
using LabExtended.Extensions;

namespace SecretLabAPI.Utilities
{
    /// <summary>
    /// Contains improvements and fixes for LabAPI events.
    /// </summary>
    public static class LabEvents
    {
        private static readonly MethodInfo invokeMethod = typeof(EventManager).FindMethod(x => x.Name == nameof(EventManager.InvokeEvent) && x.IsGenericMethodDefinition);
        private static readonly MethodInfo replacementMethod = typeof(LabEvents).FindMethod(x => x.Name == nameof(_Prefix));

        /// <summary>
        /// Event triggered before a LabAPI event handler is executed.
        /// It provides the type of the event arguments and the related event data object.
        /// Used to perform actions or modifications before the execution of event handlers.
        /// </summary>
        public static event Action<Type, object> Executing;

        /// <summary>
        /// Event triggered after a LabAPI event handler has been executed.
        /// It provides the type of the event arguments and the related event data object.
        /// Used to perform actions or logging after the execution of event handlers.
        /// </summary>
        public static event Action<Type, object> Executed;

        private static bool _Prefix<TEventArgs>(LabEventHandler<TEventArgs> eventHandler, TEventArgs args) where TEventArgs : EventArgs
        {
            if (eventHandler == null)
                return false;

            var type = typeof(TEventArgs);
            var name = type.Name;

            Executing?.InvokeSafe(type, args);
            
            try
            {
                eventHandler(args);
            }
            catch (Exception ex)
            {
                ApiLog.Error(name, ex);
            }
            
            Executed?.InvokeSafe(type, args);
            return false;
        }

        private static void Initialize()
        {
            var watch = Stopwatch.StartNew();
            var count = 0;
            
            count += InitializeAssembly(typeof(PluginLoader).Assembly);
            
            foreach (var asm in PluginLoader.Plugins)
                count += InitializeAssembly(asm.Value);
            
            watch.Stop();
            
            ApiLog.Info("SecretLabAPI", $"Patched &3{count}&r event types in &3{watch.ElapsedMilliseconds}&rms.");
        }

        private static int InitializeAssembly(Assembly assembly)
        {
            var count = 0;
            
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    try
                    {
                        if (!typeof(EventArgs).IsAssignableFrom(type))
                            continue;

                        if (type.IsGenericTypeDefinition)
                            continue;

                        var target = invokeMethod.MakeGenericMethod(type);
                        var replacement = replacementMethod.MakeGenericMethod(type);

                        ApiPatcher.Harmony.Patch(target, new(replacement));

                        count++;
                    }
                    catch (Exception typeEx)
                    {
                        ApiLog.Error("SecretLabAPI", $"Error while processing event patch for type &3{type}&r:\n{typeEx}");
                    }
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("SecretLabAPI", $"Error while processing event types for assembly &3{assembly}&r:\n{ex}");
            }

            return count;
        }
    }
}