using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act3;

public class Twilight() : EGOCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public static readonly SpireField<DarkOrb, bool> IsTwilightDarkOrb = new(() => false);
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Channeling), HoverTipFactory.FromOrb<DarkOrb>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        List<OrbModel> darkOrbs = new List<OrbModel>();
        foreach (var orb in Owner.PlayerCombatState!.OrbQueue.Orbs)
        {
            if (orb is DarkOrb)
            {
                darkOrbs.Add(orb);
            }
        }
        foreach (var orb in darkOrbs)
        {
            await OrbCmd.Evoke(choiceContext, Owner, orb,false);
            await Cmd.CustomScaledWait(0.1f, 0.25f);
            await OrbCmd.Evoke(choiceContext, Owner, orb);
        }
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var darkOrb = (DarkOrb)ModelDb.Orb<DarkOrb>().ToMutable();
        IsTwilightDarkOrb.Set(darkOrb, true);
        await OrbCmd.Channel(choiceContext, darkOrb, Owner);
        if (IsUpgraded)
        {
            IEnumerable<OrbModel> orbModels = Owner.PlayerCombatState.OrbQueue.Orbs.Where(orb => orb is DarkOrb);
            foreach (OrbModel darknessOrb in orbModels)
            {
                await OrbCmd.Passive(choiceContext, darknessOrb, null);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
    
    [HarmonyPatch(typeof (DarkOrb), "PassiveVal", MethodType.Getter)]
    private static class IconPatch
    {
        private static void Postfix(DarkOrb __instance, ref Decimal __result)
        {
            var isTwilightDarkOrb = IsTwilightDarkOrb.Get(__instance);
            if (isTwilightDarkOrb)
            {
                __result = (int)(__result * 1.5M);
            }
        }
    }
    
    [HarmonyPatch(typeof (OrbModel), "Title", MethodType.Getter)]
    private static class TwilightDarkTitlePatch
    {
        private static bool Prefix(OrbModel __instance, ref LocString __result)
        {
            if (__instance is DarkOrb dark)
            {
                var isTwilightDarkOrb = IsTwilightDarkOrb.Get(dark);
                if (isTwilightDarkOrb)
                {
                    __result = new LocString("orbs","RUINA2-TWILIGHT_DARK_ORB.title");
                    return false;
                }
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof (OrbModel), "SmartDescription", MethodType.Getter)]
    private static class TwilightDarkDescriptionPatch
    {
        private static bool Prefix(OrbModel __instance, ref LocString __result)
        {
            if (__instance is DarkOrb dark)
            {
                var isTwilightDarkOrb = IsTwilightDarkOrb.Get(dark);
                if (isTwilightDarkOrb)
                {
                    __result = new LocString("orbs","RUINA2-TWILIGHT_DARK_ORB.smartDescription");
                    return false;
                }
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof (DarkOrb), nameof(DarkOrb.Evoke))]
    private static class TwilightDarkEvokePatch
    {
        private static bool Prefix(DarkOrb __instance, PlayerChoiceContext playerChoiceContext, ref Task<IEnumerable<Creature>> __result)
        {
            var isTwilightDarkOrb = IsTwilightDarkOrb.Get(__instance);
            if (isTwilightDarkOrb)
            {
                __result = Wrap(__instance, playerChoiceContext);
                return false;
            }
            return true;
        }
        
        private static async Task<IEnumerable<Creature>> Wrap(DarkOrb darkOrb, PlayerChoiceContext playerChoiceContext)
        {
            List<Creature> enemies = darkOrb.CombatState.HittableEnemies.Where(e => e.IsHittable).ToList();
            if (darkOrb.EvokeVal <= 0M)
                return Array.Empty<Creature>();
            darkOrb.ActivateEvoke(enemies.ToArray());
            await CreatureCmd.Damage(playerChoiceContext, enemies, darkOrb.EvokeVal, ValueProp.Unpowered, darkOrb.Owner.Creature);
            return enemies;
        }
    }
}