using LabExtended.API.Custom.Items;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Extensions;

using SecretLabAPI.Items.Weapons;
using SecretLabAPI.Items.Weapons.ItemLauncher;

namespace SecretLabAPI.Items
{
    /// <summary>
    /// Provides static methods for registering and managing custom items.
    /// </summary>
    public static class CustomItemsHandler
    {
        private static List<Type> items = new();

        /// <summary>
        /// Adds a custom item of type <typeparamref name="T"/> to the item registry if it is not already present.
        /// </summary>
        /// <remarks>If an item of the specified type does not exist, a new instance is created and saved
        /// to a YAML file before registration. If the item already exists, no action is taken. This method is typically
        /// used to ensure that custom items are loaded and registered at application startup.</remarks>
        /// <typeparam name="T">The type of custom item to add. Must inherit from <see cref="CustomItem"/>.</typeparam>
        public static void AddItem<T>() where T : CustomItem
        {
            if (!items.AddUnique(typeof(T)))
                return;

            var name = typeof(T).Name.CamelCase();
            var path = FileUtils.CreatePath(SecretLab.RootDirectory, "items", name + ".yml");

            if (!FileUtils.TryLoadYamlFile<T>(path, out var item))
            {
                if ((item = Activator.CreateInstance<T>()) == null)
                {
                    ApiLog.Error("SecretLabAPI", $"Could not create an instance of custom item &3{typeof(T).FullName}&r");
                    return;
                }

                FileUtils.TrySaveYamlFile(path, item);
            }

            item.Register();
        }

        internal static void Initialize()
        {
            AddItem<AirsoftGun>();
            AddItem<SniperRifle>();
            AddItem<ReplicatingScp018>();

            ItemLauncher.Initialize();
        }
    }
}
