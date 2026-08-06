using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act1;
using Ruina2.Ruina2Code.Monsters.Act1.Laetitia;

namespace Ruina2.Ruina2Code.Encounters.Act1;

public sealed class LaetitiaElite : CustomEncounterModel
{
    public LaetitiaElite() : base(RoomType.Elite)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["laetitia_bg.tscn".BackgroundImagePath()], 
            "laetitia_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "laetitia_elite.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => [];
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<WitchFriend>();
            yield return ModelDb.Monster<GiftFriend>();
            yield return ModelDb.Monster<Laetitia>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<GiftFriend>().ToMutable(), "minion1"),
            (ModelDb.Monster<GiftFriend>().ToMutable(), "minion2"),
            (ModelDb.Monster<Laetitia>().ToMutable(), "laetitia")
        };
    }
}