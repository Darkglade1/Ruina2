using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act3.Apostles;

namespace Ruina2.Ruina2Code.Encounters.Act3;

public sealed class ApostlesNormal : CustomEncounterModel
{
    public ApostlesNormal() : base(RoomType.Monster)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["paradise_bg.tscn".BackgroundImagePath()], 
            "paradise_bg.tscn".BackgroundImagePath());
    }
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => [];
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<ScytheApostle>();
            yield return ModelDb.Monster<SpearApostle>();
            yield return ModelDb.Monster<StaffApostle>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<ScytheApostle>().ToMutable(), null),
            (ModelDb.Monster<SpearApostle>().ToMutable(), null),
            (ModelDb.Monster<StaffApostle>().ToMutable(), null)
        };
    }
}