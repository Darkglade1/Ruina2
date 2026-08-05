using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using Ruina2.Ruina2Code.Encounters.Act2;

namespace Ruina2.Ruina2Code.Acts;

public class Briah() : AbstractRuinaAct(2)
{
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return [
            ModelDb.Encounter<BatsWeak>(),
            ModelDb.Encounter<NosferatuWeak>(),
            ModelDb.Encounter<OzmaWeak>(),
            ModelDb.Encounter<BadWolfWeak>(),
            ModelDb.Encounter<ScarecrowsWeak>(),
            ModelDb.Encounter<NosferatuAndBatNormal>(),
            ModelDb.Encounter<OzmaAndJackNormal>(),
            ModelDb.Encounter<WolfPackNormal>(),
            ModelDb.Encounter<ScarecrowsNormal>(),
            ModelDb.Encounter<QueenNormal>(),
            ModelDb.Encounter<KnightNormal>(),
            ModelDb.Encounter<GreedNormal>(),
            ModelDb.Encounter<WoodsmanNormal>(),
            ModelDb.Encounter<MountainElite>(),
            ModelDb.Encounter<WrathElite>(),
            ModelDb.Encounter<RoadHomeElite>(),
            ModelDb.Encounter<RedWolfBoss>(),
            ModelDb.Encounter<JesterBoss>(),
        ];
    }

    public override IEnumerable<EventModel> AllEvents => ModelDb.Act<Hive>().AllEvents;
    
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act2";
    
    protected override int NumberOfWeakEncounters => 2;
}