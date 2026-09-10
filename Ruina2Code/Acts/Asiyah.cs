using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Encounters.Act1;
using Ruina2.Ruina2Code.Events;
using Ruina2.Ruina2Code.Events.Act1;

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
            ModelDb.Encounter<SpiderBudNormal>(),
            ModelDb.Encounter<FuneralNormal>(),
            // ModelDb.Encounter<VineShamblerNormal>(),
            ModelDb.Encounter<AlriuneElite>(),
            ModelDb.Encounter<HelpersElite>(),
            ModelDb.Encounter<LaetitiaElite>(),
            ModelDb.Encounter<FairyBoss>(),
            ModelDb.Encounter<NothingDerBoss>(),
            ModelDb.Encounter<BlackSwanBoss>(),
            ModelDb.Encounter<OrchestraBoss>()
        ];
    }

    public override IEnumerable<EncounterModel> BossDiscoveryOrder => [ModelDb.Encounter<FairyBoss>(), ModelDb.Encounter<BlackSwanBoss>(), ModelDb.Encounter<OrchestraBoss>(), ModelDb.Encounter<NothingDerBoss>()];

    public override IEnumerable<EventModel> AllEvents {
        get
        {
            return [
                ModelDb.Event<GalaxyChild>(),
                ModelDb.Event<Funeral>(),
                ModelDb.Event<SnowWhiteApple>(),
                ModelDb.Event<DerFreischutz>(),
                ModelDb.Event<WarpTrain>(),
                ModelDb.Event<ShiAssociation>(),
                ModelDb.Event<NightInTheBackstreets>(),
                ModelDb.Event<Art>(),
                ModelDb.Event<YourBook>(),
                ModelDb.Event<SingingMachine>(),
                //ModelDb.Event<Wellspring>(),
                //ModelDb.Event<WhisperingHollow>(),
                //ModelDb.Event<WoodCarvings>()
            ];
        }
    }
    
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act1";
    
    protected override int NumberOfWeakEncounters => 3;
    
    public override IEnumerable<AncientEventModel> AllAncients
    {
        get
        {
            if ((RunManager.Instance.State != null && RunManager.Instance.State.Modifiers.Count > 0) || !Config.NeowAngelaAppears)
            {
                return [ModelDb.AncientEvent<Neow>()];
            }

            return  [ModelDb.AncientEvent<NeowAngela>()];
        }
    }
}