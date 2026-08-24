using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Ruina2.Ruina2Code.Monsters;

[GlobalClass]
public abstract partial class NAllyButton : BaseButton
{
    private TextureRect? _icon;
    private IHoverTip? _hoverTip;
    public required AbstractAllyMonster owner;
    public override void _Ready()
    {
        var marginContainer = GetNodeOrNull<MarginContainer>("MarginContainer");
        _icon = GetNodeOrNull<TextureRect>("ButtonVisual"); 

        if (marginContainer == null)
        {
             return;
        }

        marginContainer.MouseFilter = MouseFilterEnum.Ignore;
        if (_icon != null) _icon.MouseFilter = MouseFilterEnum.Ignore;
        
        marginContainer.SetAnchorsPreset(LayoutPreset.FullRect);

        MouseFilter = MouseFilterEnum.Pass;
        Connect(Control.SignalName.MouseEntered, Callable.From(OnHovered));
        Connect(Control.SignalName.MouseExited, Callable.From(OnUnhovered));
        Connect(BaseButton.SignalName.Pressed, Callable.From(OnClick));
    }

    public override void _Process(double delta)
    {
        if (_icon != null)
        {
            if (Disabled)
            {
                _icon.Modulate = Color.Color8(128, 128, 128);
            } 
            else if (IsHovered())
            {
                _icon.Modulate = Color.Color8(218, 175, 38);
            }
            else
            {
                _icon.Modulate = Color.Color8(255, 255, 255);
            }
        }
    }

    protected abstract void OnClick();
    
    protected abstract IHoverTip? GetHoverTip();

    private void OnHovered()
    {
        _hoverTip = GetHoverTip(); 
        if (_hoverTip != null)
        {
            NHoverTipSet? nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
            Vector2 tooltipOffset = new Vector2(60f, -25f);
            if (nHoverTipSet != null)
            {
                nHoverTipSet.GlobalPosition = GlobalPosition + tooltipOffset;
                nHoverTipSet.MouseFilter = MouseFilterEnum.Ignore;
            }
        }
    }

    private void OnUnhovered()
    {
        NHoverTipSet.Remove(this);
    }
}