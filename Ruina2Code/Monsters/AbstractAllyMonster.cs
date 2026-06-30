using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractAllyMonster : AbstractMultiIntentMonster
{
    public bool IsAlly = true;
    public bool IsTargetableByPlayers = false;

    public AbstractAllyMonster()
    {
        ShouldClearBlockAtStartOfOwnTurn = false;
    }
    
    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        SetToSide(CombatSide.Player);
        FlipHorizontal();
        SetUpAllyButton("res://Ruina2/images/ui/ally_block_button.tscn", "res://Ruina2/images/ui/BlockIcon.png", 0);
    }
    
    protected void SetToSide(CombatSide side)
    {
        FieldInfo? backingField = typeof(Creature).GetField("<Side>k__BackingField", 
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField != null)
        {
            backingField.SetValue(Creature, side); 
        }
    }
    
    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && IsAlly)
        {
            Creature.Block = 0;
        }
        return Task.CompletedTask;
    }

    public override bool ShouldAllowHitting(Creature creature)
    {
        if (creature.Monster == this)
        {
            if (IsAlly && IsTargetableByPlayers)
            {
                return true;
            }
            else
            {
                return creature.CombatState?.CurrentSide == CombatSide.Enemy;
            }
        }
        return true;
    }
    
    protected void SetUpAllyButton(string scene, string path, int positionIndex)
    {
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(Creature);
        Marker2D? specialNode = creatureNode?.GetSpecialNode<Marker2D>("%IntentPos");
        if (specialNode != null)
        {
            var buttonScene = GD.Load<PackedScene>(scene);
            if (buttonScene != null)
            {
                var button = buttonScene.Instantiate<NAllyButton>();
                if (button != null)
                {
                    TextureRect? textureNode = button.GetNodeOrNull<TextureRect>("%ButtonVisual");
                    if (textureNode != null)
                    {
                        textureNode.Texture = GD.Load<Texture2D>(path);
                    }
                    button.owner = this;
                    specialNode.AddChildSafely(button);
                    button.Position += new Vector2(-125f, 50f - (75f * positionIndex));
                }
            }
        }
    }
}