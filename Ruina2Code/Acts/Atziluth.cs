using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Ruina2.Ruina2Code.Encounters.Act3;
using Ruina2.Ruina2Code.Events.Act3;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Acts;

public class Atziluth() : AbstractRuinaAct(3)
{
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return [
            ModelDb.Encounter<BloodbathWeak>(),
            ModelDb.Encounter<HeartOfAspirationWeak>(),
            ModelDb.Encounter<BirdsWeak>(),
            ModelDb.Encounter<BirdsNormal>(),
            ModelDb.Encounter<BurrowingHeavenNormal>(),
            ModelDb.Encounter<PinocchioNormal>(),
            ModelDb.Encounter<PunishingBirdNormal>(),
            ModelDb.Encounter<JudgementBirdNormal>(),
            ModelDb.Encounter<PriceOfSilenceNormal>(),
            ModelDb.Encounter<ApostlesNormal>(),
            // ModelDb.Encounter<SlimedBerserkerNormal>(),
            // ModelDb.Encounter<TheLostAndForgottenNormal>(),
            ModelDb.Encounter<BigBirdElite>(),
            ModelDb.Encounter<BlueStarElite>(),
            ModelDb.Encounter<SnowQueenElite>(),
            ModelDb.Encounter<TwilightBoss>(),
            ModelDb.Encounter<WhiteNightBoss>(),
            ModelDb.Encounter<SilentGirlBoss>()
        ];
    }
    
    public override IEnumerable<EncounterModel> BossDiscoveryOrder => [ModelDb.Encounter<TwilightBoss>(), ModelDb.Encounter<WhiteNightBoss>(), ModelDb.Encounter<SilentGirlBoss>()];

    public override IEnumerable<EventModel> AllEvents {
        get
        {
            return [
                ModelDb.Event<Lowell>(),
                ModelDb.Event<HanaAssociation>(),
                ModelDb.Event<DistortedYan>(),
                ModelDb.Event<Reflections>(),
                ModelDb.Event<RoundTeaParty>(),
                ModelDb.Event<Trial>(),
                ModelDb.Event<TinkerTime>()
            ];
        }
    }
    
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";
    
    protected override int NumberOfWeakEncounters => 2;
}