﻿using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace MechScope.Patches
{
    [HarmonyPatch(typeof(Wiring),"TripWire")]
    [HarmonyPriority(Priority.First)]
    static class TripWirePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(int left, int top, int width, int height)
        {
            if (!SuspendableWireManager.Active || Thread.CurrentThread.Name == SuspendableWireManager.wireThreadName)
            {
                VisualizerWorld.AddStart(new Point16(left, top));
                return true;
            }

            SuspendableWireManager.BeginTripWire(left, top, width, height);
            return false;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injectPostWire = false;
            int postWireCount = 0;

            Stack<Label> prevJumps = new Stack<Label>();

            foreach (var item in instructions)
            {
                if (injectPostWire)
                {
                    injectPostWire = false;
                    postWireCount++;

                    CodeInstruction[] inst = SuspendableWireManager.MakeSuspendSnippet(generator, SuspendableWireManager.SuspendMode.perWire);

                    //We want to be inside of the Wiring._wireList.Count > 0 branch, but outside of the XferWater branch
                    Label XferWaterLabel1 = prevJumps.Pop();
                    Label XferWaterLabel2 = prevJumps.Pop();
                    item.labels.Remove(XferWaterLabel1);
                    item.labels.Remove(XferWaterLabel2);
                    inst[0].labels.Add(XferWaterLabel1);
                    inst[0].labels.Add(XferWaterLabel2);

                    foreach (var item2 in inst)
                    {
                        //ErrorLogger.Log(item2);
                        yield return item2;
                    }
                }

                if (postWireCount == 4)
                {
                    postWireCount = 0;

                    CodeInstruction[] inst = SuspendableWireManager.MakeSuspendSnippet(generator, SuspendableWireManager.SuspendMode.perSource);

                    //The end of a branch is in front, but we want to be outside if it.
                    inst[0].labels = new List<Label>(item.labels);
                    item.labels.Clear();

                    foreach (var item2 in inst)
                    {
                        //ErrorLogger.Log(item2);
                        yield return item2;
                    }
                }




                //Inject code after call to XferWater at IL_011A, IL_022B, IL_033C and IL_044D
                if (item.opcode == OpCodes.Call)
                {
                    MethodInfo calledMethod = item.operand as MethodInfo;
                    if (calledMethod != null && calledMethod.Name == "XferWater")
                    {
                        injectPostWire = true;
                    }
                }

                if (item.opcode == OpCodes.Ble || item.opcode == OpCodes.Ble_S)
                {
                    prevJumps.Push((Label)item.operand);
                }

                //ErrorLogger.Log(item);
                yield return item;
            }
        }
    }
}
