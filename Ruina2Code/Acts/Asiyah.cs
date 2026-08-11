using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using Ruina2.Ruina2Code.Encounters.Act1;

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
            ModelDb.Encounter<ScorchedGirlNormal>(),
            ModelDb.Encounter<TeddyBearNormal>(),
            ModelDb.Encounter<PorccubusNormal>(),
            ModelDb.Encounter<QueenBeeNormal>(),
            ModelDb.Encounter<RedShoesNormal>(),
            ModelDb.Encounter<GalaxyFriendNormal>(),
            ModelDb.Encounter<ShyLookNormal>(),
            // ModelDb.Encounter<SlitheringStranglerNormal>(),
            // ModelDb.Encounter<SnappingJaxfruitNormal>(),
            // ModelDb.Encounter<VineShamblerNormal>(),
            ModelDb.Encounter<AlriuneElite>(),
            ModelDb.Encounter<HelpersElite>(),
            ModelDb.Encounter<LaetitiaElite>(),
            //ModelDb.Encounter<FairyBoss>(),
            //ModelDb.Encounter<NothingDerBoss>(),
            ModelDb.Encounter<BlackSwanBoss>(),
            //ModelDb.Encounter<OrchestraBoss>()
        ];
    }

    public override IEnumerable<EncounterModel> BossDiscoveryOrder => [ModelDb.Encounter<FairyBoss>(), ModelDb.Encounter<NothingDerBoss>(), ModelDb.Encounter<OrchestraBoss>()];

    public override IEnumerable<EventModel> AllEvents => ModelDb.Act<Overgrowth>().AllEvents;
    
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act1";
    
    protected override int NumberOfWeakEncounters => 3;
}