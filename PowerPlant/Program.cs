using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // from here
        List<IMyTerminalBlock> iceCargoTerminalBlocks = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> hydroTankTerminalBlocks = new List<IMyTerminalBlock>();

        List<IMyTerminalBlock> generatorTerminalBlocks = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> batteryTerminalBlocks = new List<IMyTerminalBlock>();

        IMyTimerBlock startMining;
        IMyTimerBlock stopMining;

        IMyTextPanel debug;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            IMyBlockGroup iceCargoGroup = GridTerminalSystem.GetBlockGroupWithName("Ice Cargo - Power");
            iceCargoGroup.GetBlocks(iceCargoTerminalBlocks);
            IMyBlockGroup hydroTankGroup = GridTerminalSystem.GetBlockGroupWithName("Hydro Tanks");
            hydroTankGroup.GetBlocks(hydroTankTerminalBlocks);

            IMyBlockGroup generatorGroup = GridTerminalSystem.GetBlockGroupWithName("Hydro Engines - Power");
            generatorGroup.GetBlocks(generatorTerminalBlocks);
            IMyBlockGroup batteryGroup = GridTerminalSystem.GetBlockGroupWithName("Base Batteries");
            batteryGroup.GetBlocks(batteryTerminalBlocks);

            startMining = GridTerminalSystem.GetBlockWithName("Start Mining") as IMyTimerBlock;
            stopMining = GridTerminalSystem.GetBlockWithName("Stop Mining") as IMyTimerBlock;

            debug = GridTerminalSystem.GetBlockWithName("debug") as IMyTextPanel;
        }

        private bool IceCargoHasCapacity(List<IMyTerminalBlock> cargoContainers)
        {
            List<IMyInventory> inventories = new List<IMyInventory>();

            cargoContainers.ForEach(cargo => inventories.Add(cargo.GetInventory()));

            int index = inventories.FindIndex(inventory => inventory.IsFull);

            return index == -1;
        }

        private bool HydroTankHasCaoacity(List<IMyTerminalBlock> hydroContainers)
        {
            List<IMyGasTank> hydroTanks = hydroContainers.Cast<IMyGasTank>().ToList();

            int index = hydroTanks.FindIndex(tank => tank.FilledRatio < 1);

            return index >= 0;
        }

        private float CalcPowerUsagePercentage(List<IMyTerminalBlock> batteryBlocks)
        {
            List<IMyBatteryBlock> batteries = batteryBlocks.Cast<IMyBatteryBlock>().ToList();

            return (batteries.Sum(battery => battery.CurrentOutput) / batteries.Sum(battery => battery.MaxOutput)) * 100;
        }

        private void ToggleGenerators(List<IMyTerminalBlock> generators, bool state)
        {
            foreach(IMyPowerProducer generator in generators)
            {
                generator.Enabled = state;
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (IceCargoHasCapacity(iceCargoTerminalBlocks) && HydroTankHasCaoacity(hydroTankTerminalBlocks))
            {
                startMining.Trigger();
            } else
            {
                stopMining.Trigger();
            }

            ToggleGenerators(generatorTerminalBlocks, CalcPowerUsagePercentage(batteryTerminalBlocks) > 50);

            debug.WriteText("Hello", false);
        }
        // to here
    }
}
