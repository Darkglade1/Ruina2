using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2.redWolf;

namespace Ruina2.Ruina2Code.Encounters.Act2;

public sealed class RedWolfBoss : CustomEncounterModel
{
    public RedWolfBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "LittleRed/Red".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "LittleRed/Red.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "LittleRed/RedOutline.png".MonsterImagePath().SimplifyPath();
    public override bool IsValidForAct(ActModel act) => act is Briah;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<LittleRed>();
            yield return ModelDb.Monster<NightmareWolf>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<LittleRed>().ToMutable(), null),
            (ModelDb.Monster<NightmareWolf>().ToMutable(), null)
        };
    }
}