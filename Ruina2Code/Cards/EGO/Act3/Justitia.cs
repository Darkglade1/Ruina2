using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Nodes;

namespace Ruina2.Ruina2Code.Cards.EGO.Act3;

public class Justitia() : EGOCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(11, ValueProp.Move), 
        new("HPThreshold", 30),
        new CalculationBaseVar(0M),
        new CalculationExtraVar(1M),
        new CalculatedVar("CalculatedHP").WithMultiplier((card, target) => target != null ? target.MaxHp * card.GetDynamicVar("HPThreshold").BaseValue / 100M : 0)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            return CombatState != null && CombatState.HittableEnemies.Any(e =>
            {
                return e.CurrentHp <= e.MaxHp * (DynamicVars["HPThreshold"].BaseValue / 100M);
            });
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target != null)
        {
            var enemyBelowHPThreshold = play.Target.CurrentHp <= play.Target.MaxHp * (DynamicVars["HPThreshold"].BaseValue / 100M);
            if (enemyBelowHPThreshold)
            {
                Sfx.JudgementDing.Play();
                await Cmd.Wait(0.25f);
                await JudgementFullScreenAnimation();
                await CreatureCmd.Kill(play.Target);
            }
            else
            {
                await CommonActions.CardAttack(this, play).Execute(choiceContext);
            }
        }
    }
    
    private async Task JudgementFullScreenAnimation()
    {
        Sfx.JudgementHang.Play();
        var fullScreenEffect = FullScreenAnimationEffect.Create("Hang/Hang".VfxImagePath(), 1.4f, 14);
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(fullScreenEffect);
        await Cmd.Wait(1.4f);
    }

    protected override void OnUpgrade()
    {
       RemoveKeyword(CardKeyword.Ethereal);
    }
}