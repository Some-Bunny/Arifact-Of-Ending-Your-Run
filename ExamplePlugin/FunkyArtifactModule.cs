using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;
using MonoMod.Cil;
using System;
using RoR2.Projectile;
using System.Collections.Generic;
using RoR2.Audio;
using EntityStates;
using UnityEngine.Networking;
using System.Collections.ObjectModel;
using HG;
using HarmonyLib;

namespace ArtifactOfEndingYourRun
{


    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(R2API.R2API.PluginGUID)]

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //We will be using 2 modules from R2API: ItemAPI to add our item and LanguageAPI to add our language tokens.
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html


    public class ArtifactOfEndingYourRunModule : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SomeBunny";
        public const string PluginName = "ArtifactOfEndingYourRun";
        public const string PluginVersion = "1.0.0";

        public static AssetBundle MainAssets;

        public static ConfigFile configurationFile;

        public void Awake()
        {
            Log.Init(Logger);
            configurationFile = Config;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArtifactOfEndingYourRun.rorbundle"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }
            ArtifactOfNullification nullification = new ArtifactOfNullification();
            nullification.Init();
            ArtifactOfInfestation infestation = new ArtifactOfInfestation();
            infestation.Init();

            /*
            var harmony = new Harmony("com.author.project"); // rename "author" and "project"
            harmony.PatchAll();
            */
            //Run.onRunStartGlobal += PrintMessageToChat;
            Log.LogInfo(nameof(Awake) + " done.");
        }

        /*
        [HarmonyPatch(typeof(ScriptedCombatEncounter), "Awake")]
        public class ScriptedCombatEncounter_Spawn
        {
            [HarmonyPrefix]
            public static void Prefix(ScriptedCombatEncounter __instance)
            {
                for (int i = 0; i < __instance.spawns.Length; i++)
                {
                    Log.LogInfo(__instance.spawns[i].spawnCard);
                    Log.LogInfo(__instance.spawns[i].spawnCard.name);
                }
                   
            }
        }
        */


        private void Update()
        {
          
          
        }
 
    
        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
           
        }
    }
}
