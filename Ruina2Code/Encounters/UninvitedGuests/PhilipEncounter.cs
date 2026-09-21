using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Philip;

namespace Ruina2.Ruina2Code.Encounters.UninvitedGuests;

public sealed class PhilipEncounter : CustomEncounterModel
{
    public PhilipEncounter() : base(RoomType.Elite)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["malkuth_bg.tscn".BackgroundImagePath()], 
            "malkuth_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "philip_encounter.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Malkuth>();
            yield return ModelDb.Monster<CryingChild>();
            yield return ModelDb.Monster<CryingChild2>();
            yield return ModelDb.Monster<Philip>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Malkuth>().ToMutable(), "ally"),
            (ModelDb.Monster<CryingChild>().ToMutable(), "minion1"),
            (ModelDb.Monster<CryingChild2>().ToMutable(), "minion2"),
            (ModelDb.Monster<Philip>().ToMutable(), "boss")
        };
    }
}