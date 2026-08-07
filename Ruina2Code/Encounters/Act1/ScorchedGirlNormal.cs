using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act1.ScorchedGirl;

namespace Ruina2.Ruina2Code.Encounters.Act1;

public sealed class ScorchedGirlNormal : CustomEncounterModel
{
    public ScorchedGirlNormal() : base(RoomType.Monster)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["scorched_bg.tscn".BackgroundImagePath()], 
            "scorched_bg.tscn".BackgroundImagePath());
    }
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => [];
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<MatchFlame>();
            yield return ModelDb.Monster<ScorchedGirl>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<MatchFlame>().ToMutable(), null),
            (ModelDb.Monster<MatchFlame>().ToMutable(), null),
            (ModelDb.Monster<ScorchedGirl>().ToMutable(), null)
        };
    }
}