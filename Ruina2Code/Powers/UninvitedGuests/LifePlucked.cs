using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Ruina2.Ruina2Code.Cards;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;
public class LifePlucked() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            Flash();
            await PowerCmd.Apply<Vibration>(choiceContext,player.Creature, Amount, null, null);
            await CardPileCmd.AddToCombatAndPreview<ControlledVibration>(Owner, PileType.Draw, DynamicVars.Cards.IntValue, null, CardPilePosition.Random);
        }
    }
}