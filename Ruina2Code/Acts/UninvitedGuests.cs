using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Encounters.UninvitedGuests;
using Ruina2.Ruina2Code.Events.Act4;

namespace Ruina2.Ruina2Code.Acts;

public class UninvitedGuests() : AbstractRuinaAct(4)
{
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return [
            ModelDb.Encounter<PhilipEncounter>(),
            ModelDb.Encounter<EileenEncounter>(),
            ModelDb.Encounter<GretaEncounter>(),
            ModelDb.Encounter<BremenEncounter>(),
            ModelDb.Encounter<OswaldEncounter>(),
            ModelDb.Encounter<TanyaEncounter>(),
            ModelDb.Encounter<PuppeteerEncounter>(),
            ModelDb.Encounter<ElenaEncounter>(),
            ModelDb.Encounter<PlutoEncounter>(),
            ModelDb.Encounter<ArgaliaBoss>()
        ];
    }
    
    public override IEnumerable<EventModel> AllEvents => [ModelDb.Event<Ensemble>()];

    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";
    
    protected override ActMap? CustomCreateMap(RunState runState, bool replaceTreasureWithElites)
    {
        return new UninvitedGuestsActMap();
    }
    
    public override IEnumerable<AncientEventModel> AllAncients => Array.Empty<AncientEventModel>();

    internal void ConfigureFixedProgression()
    {
        AssertMutable();
        _rooms.events.Clear();
        _rooms.events.Add(ModelDb.Event<Ensemble>());
        _rooms.eventsVisited = 0;
        _rooms.eliteEncounters.Clear();
        _rooms.eliteEncounters.Add(ModelDb.Encounter<PhilipEncounter>());
        _rooms.eliteEncounters.Add(ModelDb.Encounter<EileenEncounter>());
        _rooms.eliteEncounters.Add(ModelDb.Encounter<GretaEncounter>());
        _rooms.eliteEncounters.Add(ModelDb.Encounter<BremenEncounter>());
        _rooms.eliteEncounters.Add(ModelDb.Encounter<OswaldEncounter>());
        _rooms.eliteEncounters.Add(ModelDb.Encounter<TanyaEncounter>());
        _rooms.eliteEncounters.Add(ModelDb.Encounter<PuppeteerEncounter>());
        _rooms.eliteEncounters.Add(ModelDb.Encounter<ElenaEncounter>());
        _rooms.eliteEncounters.Add(ModelDb.Encounter<PlutoEncounter>());
        _rooms.eliteEncountersVisited = 0;
        _rooms.normalEncounters.Clear();
        _rooms.normalEncountersVisited = 0;
        _rooms.Boss = ModelDb.Encounter<ArgaliaBoss>();
        _rooms.SecondBoss = null;
        _rooms.bossEncountersVisited = 0;
    }
    
    public override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(
        IReadOnlySet<RoomType> roomTypes)
    {
        if (RunManager.Instance.State != null && RunManager.Instance.State.Act is UninvitedGuests)
        {
            return new HashSet<RoomType>
            {
                RoomType.Event
            };
        }
        return roomTypes;
    }
    
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (RunManager.Instance.State != null && RunManager.Instance.State.Act is UninvitedGuests)
        {
            if (room != null && room.RoomType == RoomType.Elite)
            {
                RelicReward? relicReward = null;
                foreach (var reward in rewards)
                {
                    if (reward is RelicReward roomRelicReward)
                    {
                        relicReward = roomRelicReward;
                        break;
                    }
                }
                if (relicReward != null)
                {
                    rewards.Remove(relicReward);
                    return true;
                }
            }
        }
        return false;
    }
}