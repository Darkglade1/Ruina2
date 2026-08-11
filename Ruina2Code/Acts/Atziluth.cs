using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using Ruina2.Ruina2Code.Encounters.Act3;

namespace Ruina2.Ruina2Code.Acts;

public class Atziluth() : AbstractRuinaAct(3)
{
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return [
            ModelDb.Encounter<AxebotsNormal>(),
            ModelDb.Encounter<ConstructMenagerieNormal>(),
            ModelDb.Encounter<DevotedSculptorWeak>(),
            ModelDb.Encounter<FabricatorNormal>(),
            ModelDb.Encounter<FrogKnightNormal>(),
            ModelDb.Encounter<GlobeHeadNormal>(),
            ModelDb.Encounter<MechaKnightElite>(),
            ModelDb.Encounter<OwlMagistrateNormal>(),
            ModelDb.Encounter<ScrollsOfBitingNormal>(),
            ModelDb.Encounter<ScrollsOfBitingWeak>(),
            ModelDb.Encounter<SlimedBerserkerNormal>(),
            ModelDb.Encounter<SoulNexusElite>(),
            ModelDb.Encounter<TheLostAndForgottenNormal>(),
            ModelDb.Encounter<TurretOperatorWeak>(),
            ModelDb.Encounter<BigBirdElite>(),
            //ModelDb.Encounter<TwilightBoss>(),
            ModelDb.Encounter<WhiteNightBoss>(),
            //ModelDb.Encounter<SilentGirlBoss>()
        ];
    }
    
    public override IEnumerable<EncounterModel> BossDiscoveryOrder => [ModelDb.Encounter<WhiteNightBoss>()];

    public override IEnumerable<EventModel> AllEvents => ModelDb.Act<Glory>().AllEvents;
    
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";
    
    protected override int NumberOfWeakEncounters => 2;
}