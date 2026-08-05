using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act1;

namespace Ruina2.Ruina2Code.Encounters.Act1;

public sealed class CrazedEmployeesWeak : CustomEncounterModel
{
    public CrazedEmployeesWeak() : base(RoomType.Monster)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["singing_bg.tscn".BackgroundImagePath()], 
            "singing_bg.tscn".BackgroundImagePath());
    }
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override bool IsWeak => true;
    public override IEnumerable<EncounterTag> Tags => [RuinaEncounterTags.Employees];
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<CrazedEmployee>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<CrazedEmployee>().ToMutable(), null),
            (ModelDb.Monster<CrazedEmployee>().ToMutable(), null)
        };
    }
}