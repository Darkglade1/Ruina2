using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Cards.EnemyCards;
using Ruina2.Ruina2Code.Monsters.Act2.Knight;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractAllyCardMonster : AbstractAllyMonster
{
    public List<CardModel> CardIntents = new();
    public Dictionary<string, CardModel> MoveToCardMap = new();
    public List<NGridCardHolder?> NCardHolders = new([null, null, null]);
    
    public override bool CanChangeScale => false;
    
    public virtual Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        return new Dictionary<string, CardModel>();
    }

    public void GenerateCardIntentVisuals()
    {
        if (NCombatRoom.Instance != null)
        {
            NCreature? creatureNode = NCombatRoom.Instance.GetCreatureNode(Creature);
            Marker2D? specialNode = creatureNode?.GetSpecialNode<Marker2D>("%IntentPos");
            if (specialNode != null)
            {
                List<Vector2> positionOffsetSet1 = [new(0, AbstractCardMonster.CardIntentY)];
                List<Vector2> positionOffsetSet2 = [new(-60f, AbstractCardMonster.CardIntentY), new(60f, AbstractCardMonster.CardIntentY)];
                List<Vector2> positionOffsetSet3 = [new(-120f, AbstractCardMonster.CardIntentY), new(0, AbstractCardMonster.CardIntentY), new(120f, AbstractCardMonster.CardIntentY)];
                var positionOffsetSetToUse = new List<Vector2>();
                if (CardIntents.Count == 1)
                {
                    positionOffsetSetToUse = positionOffsetSet1;
                } else if (CardIntents.Count == 2)
                {
                    positionOffsetSetToUse = positionOffsetSet2;
                }
                else
                {
                    positionOffsetSetToUse = positionOffsetSet3;
                }

                for (int i = 0; i < CardIntents.Count; i++)
                {
                    var card = CardIntents[i];
                    NGridCardHolder? nCardHolder = NCardHolders[i];
                    if (nCardHolder == null || !GodotObject.IsInstanceValid(nCardHolder))
                    {
                        NCard? nCard = NCard.Create(card);
                        if (nCard != null)
                        {
                            nCard.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
                            nCardHolder = NGridCardHolder.Create(nCard);
                            if (nCardHolder != null)
                            {
                                NCardHolders[i] = nCardHolder;
                                specialNode.AddChildSafely(nCardHolder);
                                nCardHolder.Position += positionOffsetSetToUse[i];
                                nCardHolder.ReassignToCard(card, PileType.Hand, null, ModelVisibility.Visible);
                                nCardHolder.Show();
                            }
                        }
                    }
                    else
                    {
                        nCardHolder.ReassignToCard(card, PileType.Hand, null, ModelVisibility.Visible);
                        nCardHolder.Show();
                    }
                }
            }
        }
    }
    
    public override void SetMoveImmediateMultiIntentMonster(MoveState state, int intentNum)
    {
        base.SetMoveImmediateMultiIntentMonster(state, intentNum);
        CardIntents.Clear();
        for (int i = 0; i < NextMoves.Count; i++)
        {
            var move = NextMoves[i];
            var card = MoveToCardMap[move.Id].CreateClone();
            EnemyCard.EnemyCardOwner.Set(card, Creature);
            card.CurrentTarget = Targets[i];
            CardIntents.Add(card);
        }
        GenerateCardIntentVisuals();
    }

    public void UpdateCardIntentVisuals()
    {
        CardIntents.Clear();
        for (int i = 0; i < NCardHolders.Count; i++)
        {
            var cardHolder = NCardHolders[i];
            if (cardHolder != null && GodotObject.IsInstanceValid(cardHolder))
            {
                cardHolder.Hide();
            }
        }
        for (int i = 0; i < NextMoves.Count; i++)
        {
            var move = NextMoves[i];
            var card = MoveToCardMap[move.Id].CreateClone();
            EnemyCard.EnemyCardOwner.Set(card, Creature);
            card.CurrentTarget = Targets[i];
            CardIntents.Add(card);
        }
        GenerateCardIntentVisuals();
    }
    
    public override async Task OnBossDeath(int phase)
    {
        await base.OnBossDeath(phase);
        foreach (var cardHolder in NCardHolders)
        {
            if (cardHolder != null)
            {
                cardHolder.QueueFree();
            }
        }
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && !wasRemovalPrevented)
        {
            foreach (var cardHolder in NCardHolders)
            {
                if (cardHolder != null)
                {
                    cardHolder.QueueFree();
                }
            }
        }
    }
    
    protected override void AfterCloned()
    {
        base.AfterCloned();
        CardIntents = new();
        NCardHolders = new([null, null, null]);
    }
}

