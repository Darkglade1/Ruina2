using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Monsters.Act2.redWolf;

namespace Ruina2.Ruina2Code.Encounters.Act2;

public sealed class RedWolfEncounterBoss : CustomEncounterModel
{
    public RedWolfEncounterBoss() : base(RoomType.Boss)
    {
    }
    
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
    
    public override string? CustomRunHistoryIconOutlinePath => ImageHelper.GetImagePath($"ui/run_history/{ModelDb.Encounter<QueenBoss>().Id.Entry.ToLowerInvariant()}.png");
    public override string? CustomRunHistoryIconPath => ImageHelper.GetImagePath($"ui/run_history/{ModelDb.Encounter<QueenBoss>().Id.Entry.ToLowerInvariant()}_outline.png");
}