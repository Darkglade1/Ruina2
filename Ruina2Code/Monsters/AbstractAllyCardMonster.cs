using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractAllyCardMonster : AbstractAllyMonster
{
    public List<CardModel> CardIntents = new();
    public Dictionary<string, CardModel> MoveToCardMap = new();
    public List<NGridCardHolder?> NCardHolders = new([null, null, null]);
    
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
                    if (nCardHolder == null)
                    {
                        NCard? nCard = NCard.Create(card);
                        if (nCard != null)
                        {
                            nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
                            nCardHolder = NGridCardHolder.Create(nCard);
                            if (nCardHolder != null)
                            {
                                NCardHolders[i] = nCardHolder;
                                specialNode.AddChildSafely(nCardHolder);
                                nCardHolder.Position += positionOffsetSetToUse[i];
                                nCardHolder.ReassignToCard(card, PileType.None, null, ModelVisibility.Visible);
                                nCardHolder.Show();
                            }
                        }
                    }
                    else
                    {
                        nCardHolder.ReassignToCard(card, PileType.None, null, ModelVisibility.Visible);
                        nCardHolder.Show();
                    }
                }
            }
        }
    }
}

