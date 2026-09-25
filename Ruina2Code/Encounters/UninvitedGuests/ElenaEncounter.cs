using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Elena;

namespace Ruina2.Ruina2Code.Encounters.UninvitedGuests;

public sealed class ElenaEncounter : CustomEncounterModel
{
    public ElenaEncounter() : base(RoomType.Elite)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["binah_bg.tscn".BackgroundImagePath()], 
            "binah_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "elena_encounter.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Binah>();
            yield return ModelDb.Monster<Vermilion>();
            yield return ModelDb.Monster<Elena>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Binah>().ToMutable(), "ally"),
            (ModelDb.Monster<Vermilion>().ToMutable(), "boss1"),
            (ModelDb.Monster<Elena>().ToMutable(), "boss2")
        };
    }
}