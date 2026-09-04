using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Cards.EGO.Act1;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class MagicBulletTempStr : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<MagicBullet>();
    
    public string CustomPackedIconPath => "flex.png".PowerImagePath();
    public string CustomBigIconPath => "flex.png".BigPowerImagePath();

    protected override bool IsPositive => true;
}