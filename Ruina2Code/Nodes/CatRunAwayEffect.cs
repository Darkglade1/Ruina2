using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Ruina2.Ruina2Code.Nodes;

public partial class CatRunAway : VfxEffect
{
    private float _startingX;
    private float _startingY;
    private float _targetX;
    private NCreature? creatureNode;

    public static CatRunAway Create(Creature creature)
    {
        var effect = new CatRunAway();
        
        effect.creatureNode = creature.GetCreatureNode();
        if (effect.creatureNode != null)
        {
            effect._startingX = effect.creatureNode.GlobalPosition.X;
            effect._startingY = effect.creatureNode.GlobalPosition.Y;
            effect._targetX = effect._startingX + 800.0f;
            effect.creatureNode.Body.Scale *= new Vector2(-1f, 1f);
        }

        effect.Setup();
        return effect;
    }

    protected override void Initialize()
    {
        Duration = 0f;
        EndDuration = 3.0f;
    }

    protected override void Update(float delta)
    {
        if (creatureNode == null)
        {
            IsDone = true;
            return;
        }
        Duration += delta;
        var t = Duration / EndDuration;
        if (t > 1.0f)
        {
            t = 1.0f;
        }
        var currX = Lerp(_startingX, _targetX, t);
        creatureNode.GlobalPosition = new Vector2(currX, _startingY);
        if (Duration >= EndDuration)
        {
            IsDone = true;
        }
    }
}