using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using R2API;
using System.Collections.Generic;
using UnityEngine;
using IL.RoR2.Achievements;

namespace BlightBurstBuff
{

    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class BlightBurstBuff : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "OakPrime";
        public const string PluginName = "BlightBurstBuff";
        public const string PluginVersion = "1.0.0";

        private readonly Dictionary<string, string> DefaultLanguage = new Dictionary<string, string>();

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            try
            {
                Log.Init(Logger);

                IL.RoR2.GlobalEventManager.OnHitEnemy += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchLdcI4(5),
                        x => x.MatchLdcR4(out _),
                        x => x.MatchLdarg(out _)
                    );
                    c.Index += 8;
                    c.Emit(OpCodes.Ldarg_1);
                    c.Emit(OpCodes.Ldloc_2);
                    c.EmitDelegate<Action<RoR2.DamageInfo, RoR2.CharacterBody>>((damageInfo, victim) =>
                    {
                        int buffCount = victim.GetBuffCount(RoR2.RoR2Content.Buffs.Blight);
                        //Log.LogDebug("buffCount onhit: " + buffCount);
                        if (buffCount > 2)
                        {
                            //Log.LogDebug("buffCount post clear: " + victim.GetBuffCount(RoR2.RoR2Content.Buffs.Blight));
                            var attackerDamage = damageInfo.attacker.GetComponent<CharacterBody>().damage * 0.6f;

                            DotController dotController = DotController.FindDotController(victim.gameObject);
                            float remainingDamage = 0.0f;
                            //int debugDotCount = 0;
                            if (dotController)
                            {
                                //Log.LogDebug("dotStackListCount: " + dotController.dotStackList.Count);
                                for (int i = dotController.dotStackList.Count - 1; i >= 0; --i)
                                {
                                    if (dotController.dotStackList[i].dotIndex == RoR2.DotController.DotIndex.Blight)
                                    {
                                        DotController.DotStack dotStack = dotController.dotStackList[i];
                                        remainingDamage += dotStack.damage / dotStack.dotDef.interval * dotStack.timer;
                                        //Log.LogDebug("dot stack timer: " + dotController.dotStackList[i].timer);
                                        //Log.LogDebug("damage coeff added: " + dotController.dotStackList[i].timer * 60.0f);
                                        dotController.RemoveDotStackAtServer(i);
                                        //debugDotCount++;
                                    }
                                }
                                //Log.LogDebug("post loop dotStackListCount: " + dotController.dotStackList.Count);

                            }
                            //Log.LogDebug("dotCount: " + debugDotCount);
                            //Log.LogDebug("total damage: " + remainingDamage);
                            //Log.LogDebug("total damage coef: " + (remainingDamage / (attackerDamage / 60)));
                            DamageInfo newDamageInfo = new DamageInfo()
                            {
                                damage = remainingDamage,
                                damageColorIndex = DamageColorIndex.Poison,
                                damageType = DamageType.Generic,
                                attacker = damageInfo.attacker,
                                crit = damageInfo.crit, // consider making unable to crit
                                force = Vector3.zero,
                                inflictor = (GameObject)null,
                                position = damageInfo.position,
                                procChainMask = damageInfo.procChainMask,
                                procCoefficient = 1f
                            };
                            // EffectManager.SimpleImpactEfect here
                            GlobalEventManager.instance.OnHitEnemy(newDamageInfo, victim.gameObject);
                            victim.healthComponent.TakeDamage(newDamageInfo);

                        }
                    });
                    this.ReplaceText();
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            };
        }
        private void ReplaceText()
        {
            this.ReplaceString("CROCO_PASSIVE_ALT_DESCRIPTION", "Attacks that apply <style=cIsHealing>Poison</style> apply stacking <style=cIsDamage>Blight</style> instead,"
                + " dealing <style=cIsDamage>60% damage per second</style>. Reaching <style=cIsDamage>3</style> stacks deals the remaining damage at once.");
        }

        private void ReplaceString(string token, string newText)
        {
            this.DefaultLanguage[token] = Language.GetString(token);
            LanguageAPI.Add(token, newText);
        }
    }
}
