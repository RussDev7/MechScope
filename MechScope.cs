using HarmonyLib;
using MechScope.UI;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MechScope
{
    public class MechScope : Mod
    {
        public static ModKeybind keyToggle;
        public static ModKeybind keyStep;
        public static ModKeybind keyAutoStep;
        public static ModKeybind keySettings;
        public static SettingsUI settingsUI;

        private static Harmony harmonyInstance;
        private static UserInterface userInterface;
        public static LegacyGameInterfaceLayer UILayer;

        public MechScope()
        {
            ContentAutoloadingEnabled = true;
            GoreAutoloadingEnabled = true;
            MusicAutoloadingEnabled = true;
            BackgroundAutoloadingEnabled = true;
        }

        public override void Load()
        {
            if (harmonyInstance == null)
                harmonyInstance = new Harmony(Name);

            harmonyInstance.PatchAll();

            keyToggle = KeybindLoader.RegisterKeybind(this, "Toggle", "NumPad1");
            keyStep = KeybindLoader.RegisterKeybind(this, "Step", "NumPad2");
            keyAutoStep = KeybindLoader.RegisterKeybind(this, "Auto step", "NumPad3");
            keySettings = KeybindLoader.RegisterKeybind(this, "Settings", "NumPad5");

            if (!Main.dedServ)
            {
                settingsUI = new SettingsUI();
                userInterface = new UserInterface();
                userInterface.SetState(settingsUI);
                settingsUI.Activate();
                UILayer = new LegacyGameInterfaceLayer("MechScope: Settings menu",
                    delegate
                    {
                        if (settingsUI.Visible)
                        {
                            settingsUI.Draw(Main.spriteBatch);
                            userInterface.Update(Main._drawInterfaceGameTime);
                        }
                        return true;
                    }
                );
            }
        }

        public override void Unload()
        {
            harmonyInstance.UnpatchAll();

            keyToggle = null;
            keyStep = null;
            keyAutoStep = null;
            keySettings = null;
            settingsUI = null;
            userInterface = null;
            UILayer = null;
        }
    }

    public class MechScopeLoader : ModSystem
    {
        public override void PreSaveAndQuit()/* tModPorter Note: Removed. Use ModSystem.PreSaveAndQuit */
        {
            SuspendableWireManager.Active = false;
            MechScope.settingsUI.Visible = false;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)/* tModPorter Note: Removed. Use ModSystem.ModifyInterfaceLayers */
        {
            int index = layers.FindIndex(x => x.Name == "Vanilla: Inventory");
            layers.Insert(index + 1, MechScope.UILayer);
        }
    }
}