using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Ruina2.Ruina2Code.Cards.EnemyCards;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Bremen;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Melody() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private Dictionary<Creature, List<CardType>> sequences = new();
    private Dictionary<Creature, List<CardType>> currentProgresses = new();
    private Dictionary<Creature, bool> completedSequences = new();
    private Bremen? bremen;
    private CardModel? melodyCard;
    private NGridCardHolder? nCardHolder;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Fragile>()];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner.Monster is Bremen boss)
        {
            bremen = boss;
        }
        if (NCombatRoom.Instance != null && Target != null && Target.Player != null && LocalContext.IsMe(Target))
        {
            NCreature? creatureNode = NCombatRoom.Instance.GetCreatureNode(Owner);
            Marker2D? specialNode = creatureNode?.GetSpecialNode<Marker2D>("%CenterPos");
            if (specialNode != null)
            {
                melodyCard = CombatState.CreateCard<Cards.EnemyCards.Bremen.Melody>(Target.Player);
                EnemyCard.EnemyCardOwner.Set(melodyCard, Owner);
                NCard? nCard = NCard.Create(melodyCard);
                if (nCard != null)
                {
                    nCard.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
                    nCardHolder = NGridCardHolder.Create(nCard);
                    if (nCardHolder != null)
                    {
                        specialNode.AddChildSafely(nCardHolder);
                        nCardHolder.Position += new Vector2(-250.0f, 0.0f);
                        nCardHolder.ReassignToCard(melodyCard, PileType.Hand, null, ModelVisibility.Visible);
                        nCardHolder.Show();
                    }
                }
            }
        }

        if (Target != null)
        {
            sequences[Target] = new List<CardType>();
            currentProgresses[Target] = new List<CardType>();
        }
        GenerateSequence();
        return Task.CompletedTask;
    }

    private void GenerateSequence()
    {
        if (Target != null && Target.Player != null)
        {
            completedSequences[Target] = false;
            sequences[Target].Clear();
            currentProgresses[Target].Clear();
            if (bremen != null)
            {
                int melodyLength = bremen.MelodyLength;
                for (int i = 0; i < melodyLength; i++)
                {
                    if (Target.Player.RunState.Rng.Niche.NextBool())
                    {
                        sequences[Target].Add(CardType.Attack);
                    }
                    else
                    {
                        sequences[Target].Add(CardType.Skill);
                    }
                }
                UpdateMelodyText();
            }
        }
    }

    private void UpdateMelodyText()
    {
        if (melodyCard == null || Target == null)
        {
            return;
        }
        var attackLocString = new LocString("gameplay_ui", "CARD_TYPE.ATTACK");
        var attackString = FormatGold(attackLocString.GetRawText());
        var skillLocString = new LocString("gameplay_ui", "CARD_TYPE.SKILL");
        var skillString = FormatGold(skillLocString.GetRawText());
        var coloredAttackString = FormatGreen(attackLocString.GetRawText());
        var coloredSkillString = FormatGreen(skillLocString.GetRawText());
        var result = "";
        for (int i = 0; i < currentProgresses[Target].Count; i++) {
            CardType type = currentProgresses[Target].ElementAt(i);
            if (type == CardType.Attack) {
                result += coloredAttackString;
            } else if (type == CardType.Skill) {
                result += coloredSkillString;
            }
            result += " ";
        }
        if (currentProgresses[Target].Count < sequences[Target].Count) {
            for (int i = currentProgresses[Target].Count; i < sequences[Target].Count; i++) {
                CardType type = sequences[Target].ElementAt(i);
                if (type == CardType.Attack) {
                    result += attackString;
                } else if (type == CardType.Skill) {
                    result += skillString;
                }
                result += " ";
            }
        }
        if (completedSequences[Target]) {
            var completedString = new LocString("gameplay_ui", "MELODY.COMPLETE");
            result += completedString.GetRawText();
        }
        var dynamicVar = melodyCard.DynamicVars["Melody"];
        if (dynamicVar is StringVar stringVar)
        {
            stringVar.StringValue = result;
        }
        if (nCardHolder != null && nCardHolder.CardNode != null)
        {
            nCardHolder.CardNode.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
        }
    }

    private string FormatGold(string input)
    {
        return "[gold]" + input + "[/gold]";
    }
    
    private string FormatGreen(string input)
    {
        return "[green]" + input + "[/green]";
    }
    
    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Target)
        {
            if (!completedSequences[Target]) {
                currentProgresses[Target].Add(cardPlay.Card.Type);
                bool correct = true;
                for (int i = 0; i < currentProgresses[Target].Count; i++) {
                    CardType sequenceType = sequences[Target].ElementAt(i);
                    CardType progressType = currentProgresses[Target].ElementAt(i);
                    if (sequenceType != progressType) {
                        currentProgresses[Target].Clear();
                        correct = false;
                        break;
                    }
                }
                if (correct && currentProgresses[Target].Count == sequences[Target].Count) {
                    Flash();
                    completedSequences[Target] = true;
                }
                UpdateMelodyText();
            }
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            Flash();
            if (Target != null && !completedSequences[Target])
            {
                await PowerCmd.Apply<Fragile>(new ThrowingPlayerChoiceContext(), Target, Amount, Owner,  null);
            }
            GenerateSequence();
        }
    }
}