using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModLoader;
using UnityEngine;
using HarmonyLib;

namespace GlidingHeatShields
{
    public class Main : Mod
    {
        const string C_STR_MOD_ID = "GLIDING_HEAT_SHIELDS";
        const string C_STR_MOD_NAME = "Gliding Heat Shields";
        const string C_STR_AUTHOR = "Altaïr";
        const string C_STR_MODLOADER_VERSION = "0.5.7";
        const string C_STR_MOD_VERSION = "V1.1";
        const string C_STR_MOD_DESCRIPTION = "This mod adds some gliding capabilities to heat shields, giving the player some control over his reentry trajectory.";


        public Main() : base(C_STR_MOD_ID, C_STR_MOD_NAME, C_STR_AUTHOR, C_STR_MODLOADER_VERSION, C_STR_MOD_VERSION, C_STR_MOD_DESCRIPTION)
        {

        }

        // This initializes the patcher. This is required if you use any Harmony patches
        public static Harmony patcher;

        public override void Load()
        {
            // Tells the loader what to run when your mod is loaded
        }

        public override void Early_Load()
        {
            // This method run s before anything from the game is loaded. This is where you should apply your patches, as shown below.

            // The patcher uses an ID formatted like a web domain
            Main.patcher = new Harmony($"{C_STR_MOD_ID}.{C_STR_MOD_NAME}.{C_STR_AUTHOR}");

            // This pulls your Harmony patches from everywhere in the namespace and applies them.
            Main.patcher.PatchAll();
            
            //base.early_load();
        }

        public override void Unload()
        {
            // This method runs if your mod gets unloaded. This shouldn't happen, so it throws an error.
            throw new NotImplementedException();
        }
    }
}
