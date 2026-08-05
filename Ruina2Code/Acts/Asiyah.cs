using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using Ruina2.Ruina2Code.Encounters.Act1;
using Ruina2.Ruina2Code.Encounters.Act2;

namespace Ruina2.Ruina2Code.Acts;

public class Asiyah() : AbstractRuinaAct(1)
{
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return [
            ModelDb.Encounter<ForsakenMurdererWeak>(),
            ModelDb.Encounter<FragmentWeak>(),
            ModelDb.Encounter<ButterfliesWeak>(),
            ModelDb.Encounter<CrazedEmployeesWeak>(),
            ModelDb.Encounter<ButterfliesNormal>(),
            ModelDb.Encounter<CrazedEmployeesNormal>(),
            ModelDb.Encounter<BygoneEffigyElite>(),
            ModelDb.Encounter<ByrdonisElite>(),
            ModelDb.Encounter<CeremonialBeastBoss>(),
            ModelDb.Encounter<CubexConstructNormal>(),
            ModelDb.Encounter<FlyconidNormal>(),
            ModelDb.Encounter<FogmogNormal>(),
            ModelDb.Encounter<InkletsNormal>(),
            ModelDb.Encounter<MawlerNormal>(),
            ModelDb.Encounter<OvergrowthCrawlers>(),
            ModelDb.Encounter<PhrogParasiteElite>(),
            ModelDb.Encounter<RubyRaidersNormal>(),
            ModelDb.Encounter<SlitheringStranglerNormal>(),
            ModelDb.Encounter<SnappingJaxfruitNormal>(),
            ModelDb.Encounter<TheKinBoss>(),
            ModelDb.Encounter<VantomBoss>(),
            ModelDb.Encounter<VineShamblerNormal>()
        ];
    }

    public override IEnumerable<EventModel> AllEvents => ModelDb.Act<Overgrowth>().AllEvents;
    
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act1";
    
    protected override int NumberOfWeakEncounters => 3;
}