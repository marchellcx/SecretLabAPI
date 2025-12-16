using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters.Parsers;

using LabExtended.Utilities;

using UnityEngine;

namespace SecretLabAPI.Textures.Commands
{
    [Command("texture", "Base command for texture utilities", "tex")]
    public class TextureCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("Lists all available textures", null)]
        public void List()
        {
            if (TextureManager.IsReloading)
            {
                Fail("TextureManager is currently reloading textures. Please wait...");
                return;
            }

            if (TextureManager.LoadedTextures.Count(x => x.Value?.Texture?.Texture != null) == 0)
            {
                Fail("No textures loaded.");
                return;
            }

            var displayed = new List<string>();

            Ok(x =>
            {
                x.AppendLine();

                foreach (var pair in TextureManager.LoadedTextures)
                {
                    if (pair.Value?.Texture?.Texture == null)
                        continue;

                    var name = Path.GetFileNameWithoutExtension(pair.Key);

                    if (displayed.Contains(name))
                        continue;

                    displayed.Add(name);

                    x.AppendLine();

                    if (pair.Value.Frames.Count > 1)
                    {
                        x.AppendLine($"[ANIMATED] {Path.GetFileName(pair.Key)}");
                        x.AppendLine($" - Resolution: {pair.Value.Texture.Texture.width} x {pair.Value.Texture.Texture.height}");
                        x.AppendLine($" - Frames: {pair.Value.Frames.Count}");
                        x.AppendLine($" - Delay: {pair.Value.Delay}s");
                        x.AppendLine($" - Loop: {pair.Value.Loop}");
                        x.AppendLine($" - Audio: {pair.Value.AnimatedSettings.AudioClip ?? "None"}");
                        x.AppendLine($" - Instances: {TextureManager.SpawnedTextures.Count(x => x.Value.Texture == pair.Value)}");
                    }
                    else
                    {
                        x.AppendLine($"[STATIC] {Path.GetFileName(pair.Key)}");
                        x.AppendLine($" - Resolution: {pair.Value.Texture.Texture.width} x {pair.Value.Texture.Texture.height}");
                        x.AppendLine($" - Instances: {TextureManager.SpawnedTextures.Count(x => x.Value.Texture == pair.Value)}");
                    }
                }

                displayed.Clear();
            });
        }

        [CommandOverload("active", "Lists all active instances.", null)]
        public void Active()
        {
            if (TextureManager.IsReloading)
            {
                Fail("TextureManager is currently reloading textures. Please wait...");
                return;
            }

            if (TextureManager.SpawnedTextures.Count == 0)
            {
                Fail("No active texture instances.");
                return;
            }

            Ok(x =>
            {
                x.AppendLine();

                foreach (var instance in TextureManager.SpawnedTextures)
                {
                    x.AppendLine($"[ID: {instance.Key}] '{Path.GetFileName(instance.Value.Texture.Path)}' at {instance.Value.Position} with {instance.Value.Toys.Count} toy(s).");
                }
            });
        }

        [CommandOverload("reload", "Reloads all textures and settings", null)]
        public void Reload()
        {
            if (TextureManager.IsReloading)
            {
                Fail("TextureManager is currently reloading textures. Please wait...");
                return;
            }

            TextureManager.ReloadSettings();
            TextureManager.ReloadTextures();

            Ok($"Started reloading textures, please wait ..");

            TimingUtils.OnTrue(() => Write("Textures reloaded!"), () => !TextureManager.IsReloading);
        }

        [CommandOverload("spawn", "Spawns a texture at a given position", null)]
        public void Spawn(
            [CommandParameter("Name", "Name of the texture to spawn.")] string textureName,

            [CommandParameter("Position", "The position to spawn the toy at.")]
            [CommandParameter(ParserType = typeof(PlayerParameterParser), ParserProperty = "CameraTransform.position")]
            Vector3 position)
        {
            if (TextureManager.IsReloading)
            {
                Fail("TextureManager is currently reloading textures. Please wait...");
                return;
            }

            if (!TextureManager.TrySpawnTexture(textureName, false, out var instance))
            {
                Fail($"Texture '{textureName}' not found.");
                return;
            }

            instance.Position = position;

            Ok($"Spawned texture '{textureName}' (ID: {instance.Id}) with {instance.Toys.Count} toy(s).");
        }

        [CommandOverload("destroy", "Destroys a texture instance by ID", null)]
        public void Destroy(
            [CommandParameter("ID", "The ID of the instance (set to -1 to destroy all instances).")] int id)
        {
            if (TextureManager.IsReloading)
            {
                Fail("TextureManager is currently reloading textures. Please wait...");
                return;
            }

            if (id == -1)
            {
                TextureManager.DestroyInstances();

                Ok($"Destroyed all instances.");
                return;
            }

            if (!TextureManager.TryDestroyInstance(id))
            {
                Fail($"Texture instance with ID '{id}' not found.");
                return;
            }

            Ok($"Texture instance with ID '{id}' has been destroyed.");
        }
    }
}