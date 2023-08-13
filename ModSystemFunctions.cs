using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.UI;

namespace MechScope
{
    public class ModSystemFunctions : ModSystem
    {
        public override void PreSaveAndQuit()
        {
            SuspendableWireManager.Active = false;
            MechScope.settingsUI.Visible = false;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(x => x.Name == "Vanilla: Inventory");
            layers.Insert(index + 1, MechScope.UILayer);
        }
    }
}
