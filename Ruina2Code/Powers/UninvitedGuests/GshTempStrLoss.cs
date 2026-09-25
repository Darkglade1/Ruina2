using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Cards.EnemyCards.Gebura;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class GshTempStrLoss : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<GreaterSplitHorizontal>();
    
    public string CustomPackedIconPath => "shackle.png".PowerImagePath();
    public string CustomBigIconPath => "shackle.png".BigPowerImagePath();

    protected override bool IsPositive => false;
}