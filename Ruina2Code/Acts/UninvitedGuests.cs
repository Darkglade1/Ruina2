using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
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
}