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
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]


    public class ArtifactOfEndingYourRunModule : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SomeBunny";
        public const string PluginName = "ArtifactOfEndingYourRun";
        public const string PluginVersion = "1.0.1";

        public static AssetBundle MainAssets;

        public static ConfigFile configurationFile;


        public void Awake()
        {
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
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
        private void Update(){}

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report){  }
    }
}
