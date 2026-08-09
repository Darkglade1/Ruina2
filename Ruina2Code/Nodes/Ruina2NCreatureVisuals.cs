using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Ruina2.Ruina2Code.Nodes;

[GlobalClass]
public partial class Ruina2NCreatureVisuals : NCreatureVisuals
{
    private Node2D? _body2;
    private CreatureAnimator? _spineAnimator2;
    public MegaSprite? SpineBody2 { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        _body2 = GetNodeOrNull<Node2D>((NodePath) "%Visuals2");
        if (_body2 != null)
        {
            SpineBody2 = new MegaSprite((Variant) (GodotObject) _body2);
            if (SpineBody2?.GetSkeleton()?.GetData() == null)
            {
                SpineBody2 = null;
            }
        }
        if (SpineBody2 != null)
        {
            var idle = new AnimState("Idle", true);
            var animator = new CreatureAnimator(idle, SpineBody2);
            animator.AddAnyState("Idle", idle);
            _spineAnimator2 = animator;
            _spineAnimator2.SetTrigger("Idle");
            SetSpine2IdleAnimation();
        }
    }
    
    public void SetSpine2IdleAnimation()
    {
        if (_body2 != null)
        {
            _body.Visible = false;
            _body2.Visible = true;
        }
    }
    
    public void SetSpineIdleAnimation()
    {
        if (_body2 != null)
        {
            _body.Visible = true;
            _body2.Visible = false;
        }
    }
}