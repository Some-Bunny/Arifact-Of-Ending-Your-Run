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
using UnityEngine.Networking;
using System.Collections.ObjectModel;
using HG;
using R2API;
using UnityEngine.AddressableAssets;
using MonoMod.RuntimeDetour;
using HarmonyLib;
using System.Collections;
using System.ComponentModel;
using System.Xml.Linq;
using RoR2.ExpansionManagement;
using System.Linq;

namespace ArtifactOfEndingYourRun
{
    public class ArtifactOfInfestation
    {
        private static string ArtifactLangTokenName => "ARTIFACT_OF_INFEST";
        private static string ArtifactName => "Artifact of Infestation";
        private static string ArtifactDescription => "When enabled, causes every interactable to spawn Void Infestors when used.";

        private static ArtifactDef InfestationArtifactDefinition;
        public bool ArtifactEnabled => RunArtifactManager.instance.IsArtifactEnabled(InfestationArtifactDefinition);


        public CharacterSpawnCard InfestorPrefab;

        public WeightedTypeCollection<CharacterSpawnCard> weightedType = new WeightedTypeCollection<CharacterSpawnCard>();
        public void Init()
        {

            LanguageAPI.Add("ARTIFACT_" + ArtifactLangTokenName + "_NAME", ArtifactName);
            LanguageAPI.Add("ARTIFACT_" + ArtifactLangTokenName + "_DESCRIPTION", ArtifactDescription);

            InfestationArtifactDefinition = ScriptableObject.CreateInstance<ArtifactDef>();
            InfestationArtifactDefinition.smallIconDeselectedSprite = ArtifactOfEndingYourRunModule.MainAssets.LoadAsset<Sprite>("infestorDisabled");
            InfestationArtifactDefinition.smallIconSelectedSprite = ArtifactOfEndingYourRunModule.MainAssets.LoadAsset<Sprite>("infestorEnabled");
            InfestationArtifactDefinition.cachedName = "ARTIFACT_" + ArtifactLangTokenName;
            InfestationArtifactDefinition.nameToken = "ARTIFACT_" + ArtifactLangTokenName + "_NAME";
            InfestationArtifactDefinition.descriptionToken = "ARTIFACT_" + ArtifactLangTokenName + "_DESCRIPTION";

            weightedType.elements = new WeightedType<CharacterSpawnCard>[]
            {
                new WeightedType<CharacterSpawnCard>(){ value = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/EliteVoid/cscVoidInfestor.asset").WaitForCompletion(), weight = 1 }
            };



            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;



            ContentAddition.AddArtifactDef(InfestationArtifactDefinition);

            
            On.RoR2.ScrapperController.BeginScrapping += ScrapperController_BeginScrapping;
            On.RoR2.RouletteChestController.EjectPickupServer += RouletteChestController_EjectPickupServer;
            On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;

            On.EntityStates.DeepVoidPortalBattery.Charging.OnEnter += Charging_OnEnter;
            On.EntityStates.Missions.Arena.NullWard.WardOnAndReady.OnEnter += WardOnAndReady_OnEnter;
            On.EntityStates.Missions.Moon.MoonBatteryActive.OnEnter += MoonBatteryActive_OnEnter;

        }


        private void Charging_OnEnter(On.EntityStates.DeepVoidPortalBattery.Charging.orig_OnEnter orig, EntityStates.DeepVoidPortalBattery.Charging self)
        {
            orig(self);
            if (ArtifactEnabled == true)
            {
                if (VoidStageMissionController.instance)
                {
                    VoidStageMissionController.instance.StartCoroutine(this.Delay(null, self.gameObject, VoidStageMissionController.instance, 8, 3, 5));
                }
            }
        }

        private void WardOnAndReady_OnEnter(On.EntityStates.Missions.Arena.NullWard.WardOnAndReady.orig_OnEnter orig, EntityStates.Missions.Arena.NullWard.WardOnAndReady self)
        {
            orig(self);
            if (ArtifactEnabled == true)
            {
                self.purchaseInteraction.StartCoroutine(this.Delay(null, self.gameObject, self.purchaseInteraction, 10, 3, 7.5f));

            }
        }

        private void MoonBatteryActive_OnEnter(On.EntityStates.Missions.Moon.MoonBatteryActive.orig_OnEnter orig, EntityStates.Missions.Moon.MoonBatteryActive self)
        {
            orig(self);
            if (ArtifactEnabled == true)
            {
                self.chargeIndicatorController.StartCoroutine(this.Delay(null, self.gameObject, self.chargeIndicatorController, 5, 5, 6f));

            }
        }

       

        private void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
        {
            if (ArtifactEnabled == true)
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'System.Void RoR2.ShrineChanceBehavior::AddShrineStack(RoR2.Interactor)' called on client");
                    return;
                }
                PickupIndex pickupIndex = PickupIndex.none;
                if (self.dropTable)
                {
                    if (self.rng.nextNormalizedFloat > self.failureChance)
                    {
                        pickupIndex = self.dropTable.GenerateDrop(self.rng);
                    }
                }
                else
                {
                    PickupIndex none = PickupIndex.none;
                    PickupIndex value = self.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
                    PickupIndex value2 = self.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
                    PickupIndex value3 = self.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
                    PickupIndex value4 = self.rng.NextElementUniform<PickupIndex>(Run.instance.availableEquipmentDropList);
                    WeightedSelection<PickupIndex> weightedSelection = new WeightedSelection<PickupIndex>(8);
                    weightedSelection.AddChoice(none, self.failureWeight);
                    weightedSelection.AddChoice(value, self.tier1Weight);
                    weightedSelection.AddChoice(value2, self.tier2Weight);
                    weightedSelection.AddChoice(value3, self.tier3Weight);
                    weightedSelection.AddChoice(value4, self.equipmentWeight);
                    pickupIndex = weightedSelection.Evaluate(self.rng.nextNormalizedFloat);
                }
                string baseToken;
                if (pickupIndex == PickupIndex.none)
                {
                    baseToken = "SHRINE_CHANCE_FAIL_MESSAGE";
                }
                else
                {
                    int fuck = 0;
                    itemCount.TryGetValue(pickupIndex.pickupDef.itemTier, out fuck);

                    if (RoR2.EquipmentCatalog.allEquipment.Contains(pickupIndex.equipmentIndex)
)
                    {
                        fuck += 3;
                    }

                    self.StartCoroutine(this.Delay(null, self.gameObject, self, fuck, 0.25f, 0.125f, 0, 3, 0, self.dropletOrigin));

                    baseToken = "SHRINE_CHANCE_SUCCESS_MESSAGE";
                    self.successfulPurchaseCount++;
                    PickupDropletController.CreatePickupDroplet(pickupIndex, self.dropletOrigin.position, self.dropletOrigin.forward * 20f);
                }
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = activator.GetComponent<CharacterBody>(),
                    baseToken = baseToken
                });
                                
                self.waitingForRefresh = true;
                self.refreshTimer = 2f;
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
                {
                    origin = self.transform.position,
                    rotation = Quaternion.identity,
                    scale = 1f,
                    color = self.shrineColor
                }, true);
                if (self.successfulPurchaseCount >= self.maxPurchaseCount)
                {
                    self.symbolTransform.gameObject.SetActive(false);
                }
            }
            else
            {
                orig(self, activator);
            }
        }


        private void RouletteChestController_EjectPickupServer(On.RoR2.RouletteChestController.orig_EjectPickupServer orig, RouletteChestController self, PickupIndex pickupIndex)
        {
            if (ArtifactEnabled == true)
            {
                foreach (var container in containers)
                {
                    if (self.gameObject.GetComponent(container.daType) != null)
                    {
                        if (UnityEngine.Random.value < container.chance)
                        {
                            if (self)
                            {
                                self.StartCoroutine(this.Delay(container, self.gameObject, self));
                            }
                        }
                    }
                }
            }
            orig(self, pickupIndex);
        }

      

        private void ScrapperController_BeginScrapping(On.RoR2.ScrapperController.orig_BeginScrapping orig, ScrapperController self, int intPickupIndex)
        {
            orig(self, intPickupIndex);
            if (ArtifactEnabled == true)
            {
                PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(intPickupIndex));
                foreach (var container in containers)
                {
                    if (self.gameObject.GetComponent(container.daType) != null)
                    {
                        if (UnityEngine.Random.value < container.chance)
                        {
                            if (self)
                            {
                                for (int i = 0; i < self.itemsEaten; i++)
                                {
                                    int a = 1;
                                    if (pickupDef != null)
                                    {
                                        itemCount.TryGetValue(pickupDef.itemTier, out a);
                                    }
                                    self.StartCoroutine(this.Delay(container, self.gameObject, self.interactor, a));
                                }                
                            }
                        }
                    }
                }
            }
        }

     
      

      

        private void GlobalEventManager_OnInteractionsGlobal(Interactor arg1, IInteractable arg2, GameObject arg3)
        {
            if (ArtifactEnabled == true)
            {
                foreach (var container in containers)
                {
                    if (arg3.GetComponent(container.daType) != null)
                    {
                        if (UnityEngine.Random.value < container.chance)
                        {
                           if (arg3)
                           {
                                arg1.StartCoroutine(this.Delay(container, arg3, arg1));
                           }
                        }
                    }
                }
            }          
        }

        public IEnumerator Delay(VoidInfestorContainer container, GameObject arg3, MonoBehaviour interactor, int OverrideAmount = -1, float overrideInitialDelay = -1, float overrideDelatAfterEachInfestor = -1, float offsetX = 0, float offsetY = 0, float offsetZ = 0, Transform overrtansTrans = null)
        {

            if (Run.instance.IsExpansionEnabled(RoR2.DLC1Content.Items.BearVoid.requiredExpansion) == false) { yield break; }

            float e = 0; float d = overrideInitialDelay > -1 ? overrideInitialDelay : container.InitialDelay;
            if (container != null)
            {
                if (container.modifyDelay != null)
                {
                    d += container.modifyDelay(arg3);
                }
            }
            while (e < d) {
                e += Time.deltaTime;
                if (arg3 == null) { yield break; }
                yield return null;      
            }
            ModelLocator component2 = arg3.GetComponent<ModelLocator>();
            Transform transform2;
            if (component2 == null)
            {
                transform2 = null;
            }
            else
            {
                Transform modelTransform = component2.modelTransform;
                if (modelTransform == null)
                {
                    transform2 = null;
                }
                else
                {
                    ChildLocator component3 = modelTransform.GetComponent<ChildLocator>();
                    transform2 = ((component3 != null) ? component3.FindChild("FireworkOrigin") : null);
                }
            }
            int amount = OverrideAmount > -1 ? OverrideAmount : container.Amount;
            float daei = overrideDelatAfterEachInfestor > -1 ? overrideDelatAfterEachInfestor : container.DelayAfterEachInfestor;
            if (container != null)
            {
                if (OverrideAmount == -1 && container.additionalConditional != null)
                {
                    amount += container.additionalConditional(arg3);
                }
                else if (OverrideAmount > -1)
                {
                    amount = OverrideAmount;
                }
            }

            if (overrtansTrans) { transform2 = overrtansTrans; }

            interactor.StartCoroutine(this.SpawnInfestorAmount(arg3.transform, amount, 
                container != null ? container.Offset.x : offsetX,
                container != null ? container.Offset.y : offsetY, 
                container != null ? container.Offset.z : offsetZ, 
                transform2, daei));
            yield break;
        }



        public static List<VoidInfestorContainer> containers = new List<VoidInfestorContainer>()
        {
            new VoidInfestorContainer(){ daType = typeof(ShrineRestackBehavior), Amount = 4, DelayAfterEachInfestor = 0.25f, InitialDelay = 0.5f},
            new VoidInfestorContainer(){ daType = typeof(ShrineBloodBehavior), Amount = 1, additionalConditional = ShrineBloodBehaviorConditional},
            new VoidInfestorContainer(){ daType = typeof(ShrineBossBehavior), Amount = 3, DelayAfterEachInfestor = 3, InitialDelay = 1},
            //new VoidInfestorContainer(){ daType = typeof(ShrineChanceBehavior), Amount = 0},
            new VoidInfestorContainer(){ daType = typeof(ShrineCleanseBehavior), Amount = 3, DelayAfterEachInfestor = 0.2f},
            new VoidInfestorContainer(){ daType = typeof(ShrineCombatBehavior), Amount = 4, InitialDelay = 0.25f, DelayAfterEachInfestor = 1f},
            new VoidInfestorContainer(){ daType = typeof(ShrineHealingBehavior), Amount = 0, additionalConditional = ShrineHealingBehaviorConditional, DelayAfterEachInfestor = 0.5f},
            new VoidInfestorContainer(){ daType = typeof(ShrinePlaceTotem), Amount = 0},

            new VoidInfestorContainer(){ daType = typeof(TeleporterInteraction), Amount = 8, additionalConditional = TeleporterInteractionConditional, InitialDelay = 3 ,DelayAfterEachInfestor = 2.5f},

            new VoidInfestorContainer(){ daType = typeof(TimedChestController), Amount = 6, InitialDelay = 1, DelayAfterEachInfestor = 0.33f},
            new VoidInfestorContainer(){ daType = typeof(BarrelInteraction), Amount = 1, chance = 0.25f },
            new VoidInfestorContainer(){ daType = typeof(PortalStatueBehavior), Amount = 3, InitialDelay = 0.25f, DelayAfterEachInfestor = 0.25f},
            new VoidInfestorContainer(){ daType = typeof(ChestBehavior), Amount = 0, additionalConditional = ChestBehaviorConditional, DelayAfterEachInfestor = 0.5f, InitialDelay = 0.5f},
            new VoidInfestorContainer(){ daType = typeof(RouletteChestController), Amount = 0, InitialDelay = 0.1f, DelayAfterEachInfestor = 0.25f, additionalConditional = RouletteChestControllerConditional},
            new VoidInfestorContainer(){ daType = typeof(OptionChestBehavior), Amount = 0, additionalConditional = OptionChestBehaviorConditional, DelayAfterEachInfestor = 0.33f},

            new VoidInfestorContainer(){ daType = typeof(ScrapperController), Amount = 0, InitialDelay = 3.25f, DelayAfterEachInfestor = 0.05f, modifyDelay = modifyDelayScrapperController},

            new VoidInfestorContainer(){ daType = typeof(ShopTerminalBehavior), Amount = 0, InitialDelay = 0.1f, DelayAfterEachInfestor = 0.1f, modifyDelay = modifyDelayShopTerminalBehavior, additionalConditional = ShopTerminalBehaviorConditional},

            new VoidInfestorContainer(){ daType = typeof(VendingMachineBehavior), Amount = 1, InitialDelay = 1f, DelayAfterEachInfestor = 0.1f, additionalConditional = VendingMachineBehaviorConditional},

        };

        public static int VendingMachineBehaviorConditional(GameObject obj)
        {
            var b = obj.GetComponent<VendingMachineBehavior>();
            if (b != null)
            {
                if  (UnityEngine.Random.value < (b.purchaseCount / (b.maxPurchases - 4))) 
                {
                    return 1;
                }
            }
            return 0;
        }

        public static int ShopTerminalBehaviorConditional(GameObject obj)
        {
            int i = 0;
            var b = obj.GetComponent<ShopTerminalBehavior>();
            if (b != null)
            {
                int a = 0;
                if (obj.gameObject.name.Contains("Duplicator"))
                {
                    itemCount.TryGetValue(b.pickupDisplay.pickupIndex.pickupDef.itemTier, out a);
                    if (RoR2.EquipmentCatalog.allEquipment.Contains(b.pickupDisplay.pickupIndex.equipmentIndex))
                    {
                        i += 3;
                    }
                }
                else
                {
                    itemCount.TryGetValue(b.dropTable.GenerateDrop(b.rng).pickupDef.itemTier, out a);
                    if (RoR2.EquipmentCatalog.allEquipment.Contains(b.pickupDisplay.pickupIndex.equipmentIndex))
                    {
                        i += 3;
                    }
                }
                i += a;
            }
            return i;
        }

        public static float modifyDelayShopTerminalBehavior(GameObject obj)
        {
            float i = 0;
            if (obj.gameObject.name.Contains("Duplicator")) { i = 2.66f; }
            return i;
        }


        public static float modifyDelayScrapperController(GameObject obj)
        {
            float i = 0;
            var b = obj.GetComponent<ScrapperController>();
            if (b != null) {i = 0.5f * b.itemsEaten; }
            return i;
        }


        public static int RouletteChestControllerConditional(GameObject obj)
        {
            int i = 0;
            var b = obj.GetComponent<RouletteChestController>();
            if (b != null)
            {
                int a = 0;
                itemCount.TryGetValue(b.pickupDisplay.pickupIndex.pickupDef.itemTier, out a);
                if (RoR2.EquipmentCatalog.allEquipment.Contains(b.pickupDisplay.pickupIndex.equipmentIndex))
                {
                    i += 3;
                }
                i += a;
            }
            return i;
        }

        public static int ShrineHealingBehaviorConditional(GameObject obj)
        {
            int i = 0;
            var b = obj.GetComponent<ShrineHealingBehavior>();
            if (b != null)
            {
                return b.purchaseCount;
            }
            return i;
        }

        public static int OptionChestBehaviorConditional(GameObject obj)
        {
            int i = 0;
            var b = obj.GetComponent<OptionChestBehavior>();
            if (b != null)
            {
                int a = 0;
                itemCount.TryGetValue(b.displayTier, out a);
                i += a;
            }
            return i;
        }

        public static int ChestBehaviorConditional(GameObject obj)
        {
            int i = 0;
            var b = obj.GetComponent<ChestBehavior>();
            if (b != null)
            {
                //i += b.dropCount;
                if (b.dropPickup != null)
                {
                    int a = 0;
                    itemCount.TryGetValue(b.dropPickup.pickupDef.itemTier, out a);
                    if (RoR2.EquipmentCatalog.allEquipment.Contains(b.dropPickup.equipmentIndex))
                    {
                        i += 3;
                    }
                    i += a;
                }
                else
                {
                    int a = 0;
                    itemCount.TryGetValue(b.dropTable.GenerateDrop(b.rng).pickupDef.itemTier, out a);
                    if (RoR2.EquipmentCatalog.allEquipment.Contains(b.dropPickup.equipmentIndex))
                    {
                        i += 3;
                    }
                    i += a;
                }
            }
            return i;
        }

        public static int TeleporterInteractionConditional(GameObject obj)
        {
            int i = 0;
            var b = obj.GetComponent<TeleporterInteraction>();
            if (b != null)
            {
                i = Mathf.Min(16, b.shrineBonusStacks * 4);
            }
            return i;
        }

        public static int ShrineBloodBehaviorConditional(GameObject obj)
        {
            var b = obj.GetComponent<ShrineBloodBehavior>();
            if (b != null){
                return b.purchaseCount;
            }
            return 0;
        }

        public class VoidInfestorContainer
        {
            public Type daType = typeof(TeleporterInteraction);

            public int Amount = 6;
            
            public Vector3 Offset = new Vector3(0, 0);

            public float chance = 1;

            public float InitialDelay = 0.1f;
            public float DelayAfterEachInfestor = 0.25f;

            public Func<GameObject, int> additionalConditional;
            public Func<GameObject, float> modifyDelay;
        }


        public static Dictionary<ItemTier, int> itemCount = new Dictionary<ItemTier, int>()
        {
            {ItemTier.Boss, 4},
            {ItemTier.Lunar, 4},
            {ItemTier.NoTier, 0},
            {ItemTier.Tier1, 1},
            {ItemTier.Tier2, 3},
            {ItemTier.Tier3, 8},
            {ItemTier.VoidBoss, 6},
            {ItemTier.VoidTier1, 2},
            {ItemTier.VoidTier2, 4},
            {ItemTier.VoidTier3, 10},
        };

        public IEnumerator SpawnInfestorAmount(Transform transformPosition, int Amount = 1, float x = 0, float y = 0, float z = 0, Transform Th = null, float delay = 0.1f)
        {
            if (ArtifactEnabled == true)
            {
                if (transformPosition.gameObject == null) {yield break; }
                
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    minDistance = 1f,
                    maxDistance = 3f,
                    spawnOnTarget = Th ?? transformPosition.transform,
                    position = Th != null ? Th.position + new Vector3(x, y, z) : transformPosition.position + new Vector3(x, y, z)
                };
                ulong seed = Run.instance.seed ^ (ulong)((long)Run.instance.stageClearCount);
                Xoroshiro128Plus rng = new Xoroshiro128Plus(seed);

                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(weightedType.SelectByWeight(), placementRule, rng);
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                directorSpawnRequest.teamIndexOverride = TeamIndex.None;
                directorSpawnRequest.onSpawnedServer += OnSp;
                for (int i = 0; i < Amount; i++)
                {
                    float e = 0; float d = delay;
                    while (e < d)
                    {
                        e += Time.deltaTime;
                        if (transformPosition.gameObject == null) { yield break; }
                        yield return null;
                    }
                    DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                }
            }
        }

        public void OnSp(SpawnCard.SpawnResult b)
        {
            if (b.spawnedInstance)
            {
                var body = b.spawnedInstance.GetComponent<CharacterBody>();
                if (body) {body.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, 10); }
            }
        } 
     
    } 
}
