using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

namespace Ruina2.Ruina2Code.Encounters.UninvitedGuests;

public sealed class OswaldEncounter : CustomEncounterModel
{
    public OswaldEncounter() : base(RoomType.Monster)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["wrath_bg.tscn".BackgroundImagePath()], 
            "wrath_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "wrath_elite.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Tiph>();
            yield return ModelDb.Monster<Oswald>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Tiph>().ToMutable(), "wrath"),
            (ModelDb.Monster<Oswald>().ToMutable(), "hermit")
        };
    }
}