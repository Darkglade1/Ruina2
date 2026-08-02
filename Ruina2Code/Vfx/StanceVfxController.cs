using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Ruina2.Ruina2Code.Nodes;

namespace Ruina2.Ruina2Code.Vfx;

public class StanceVfxController(StanceVfxConfig cfg)
{

    private static Color? _originalModulate;
    private Node2D? _vfxInstance;

    public void OnEnter(Creature owner)
    {
        CreateAura(owner);
        ApplyBodyTint(owner);
    }

    public void OnExit(Creature owner)
    {
        RemoveAura();
        ResetBodyTint(owner);
    }

    // ── Aura ──────────────────────────────────────

    private void CreateAura(Creature owner)
    {
        if (cfg.AuraScenePath == null) return;

        var visuals = NCombatRoom.Instance?.GetCreatureNode(owner)?.Visuals;
        if (visuals == null) return;

        var container = visuals.GetNodeOrNull<Node2D>("StanceVfxContainer")
                        ?? CreateContainer(visuals);

        if (_vfxInstance != null && GodotObject.IsInstanceValid(_vfxInstance))
            _vfxInstance.QueueFree();

        _vfxInstance = PreloadManager.Cache.GetScene(cfg.AuraScenePath).Instantiate<Node2D>();
        _vfxInstance.Position = Vector2.Zero;
        _vfxInstance.Scale = Vector2.One;
        container.AddChild(_vfxInstance);

        foreach (var burst in _vfxInstance.GetChildren()
                     .Where(c => c.Name.ToString().Contains("Burst"))
                     .Cast<Node2D>())
        {
            var pos = burst.GlobalPosition;
            burst.Reparent(visuals);
            burst.GlobalPosition = pos;
            visuals.MoveChild(burst, 0);
        }
    }

    private static Node2D CreateContainer(Node visuals)
    {
        var c = new Node2D { Name = "StanceVfxContainer", Position = Vector2.Zero };
        visuals.AddChild(c);
        return c;
    }

    private void RemoveAura()
    {
        if (_vfxInstance == null || !GodotObject.IsInstanceValid(_vfxInstance)) return;

        foreach (var child in _vfxInstance.GetChildren())
            switch (child)
            {
                case WrathGlowSparkSpawner sparks: sparks.StopSpawning(); break;
                case CalmFrostStreakSpawner streaks: streaks.StopSpawning(); break;
                case DivinityEyeSpawner eyes: eyes.StopSpawning(); break;
                case AuraBlobEmitter blob:
                    foreach (var cpu in blob.GetChildren().OfType<CpuParticles2D>())
                        cpu.Emitting = false;
                    var timer = blob.GetTree().CreateTimer(2.5f);
                    timer.Timeout += () =>
                    {
                        if (GodotObject.IsInstanceValid(blob)) blob.QueueFree();
                    };
                    break;
            }

        _vfxInstance = null;
    }

    // ── Body Tint ─────────────────────────────────

    private void ApplyBodyTint(Creature owner)
    {
        if (cfg.BodyTint == null) return;
        var body = NCombatRoom.Instance?.GetCreatureNode(owner)?.Body;
        if (body == null) return;
        _originalModulate ??= body.Modulate;
        body.Modulate = cfg.BodyTint.Value;
    }

    private void ResetBodyTint(Creature owner)
    {
        if (_originalModulate == null) return;
        var body = NCombatRoom.Instance?.GetCreatureNode(owner)?.Body;
        if (body == null) return;
        body.Modulate = _originalModulate.Value;
        _originalModulate = null;
    }
}