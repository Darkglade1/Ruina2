using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Encounters.UninvitedGuests;
using Ruina2.Ruina2Code.Events.Act5;

namespace Ruina2.Ruina2Code.Acts;

public class BlackSilence() : AbstractRuinaAct(5)
{ 
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return [
            ModelDb.Encounter<PhilipEncounter>(),
            ModelDb.Encounter<TanyaEncounter>(),
            ModelDb.Encounter<ArgaliaBoss>()
        ];
    }
    
    public override IEnumerable<EventModel> AllEvents => [ModelDb.Event<TheEnd>()];

    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";
    
    protected override ActMap? CustomCreateMap(RunState runState, bool replaceTreasureWithElites)
    {
        return new BlackSilenceActMap();
    }
    
    public override IEnumerable<AncientEventModel> AllAncients => Array.Empty<AncientEventModel>();
}