using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2;

namespace Ruina2.Ruina2Code.Encounters.Act2;

public sealed class BatsWeak : CustomEncounterModel
{
    public BatsWeak() : base(RoomType.Monster)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["blood_castle_bg.tscn".BackgroundImagePath()], 
            "blood_castle_bg.tscn".BackgroundImagePath());
    }
    public override bool IsValidForAct(ActModel act) => false;
    public override bool IsWeak => true;
    public override IEnumerable<EncounterTag> Tags => [RuinaEncounterTags.Bats];
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<SanguineBat>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<SanguineBat>().ToMutable(), null),
            (ModelDb.Monster<SanguineBat>().ToMutable(), null),
            (ModelDb.Monster<SanguineBat>().ToMutable(), null)
        };
    }
}