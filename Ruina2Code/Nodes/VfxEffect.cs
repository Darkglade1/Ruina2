using Godot;

namespace Ruina2.Ruina2Code.Nodes;

public abstract partial class VfxEffect : Node2D
{
    protected float Duration;
    protected float StartingDuration;
    public float EndDuration;
    protected Color EffectColor = Colors.White;
    public bool IsDone = false;

    protected void Setup()
    {
        ProcessMode = ProcessModeEnum.Always;
        SetProcess(true);
        
        TreeEntered += OnTreeEntered;
    }

    private void OnTreeEntered()
    {
        Initialize();
        
        GetTree().ProcessFrame += OnProcessFrame;
    }

    private void OnProcessFrame()
    {
        if (!IsInsideTree())
        {
            GetTree().ProcessFrame -= OnProcessFrame;
            return;
        }

        Update((float)GetProcessDeltaTime());

        if (IsDone)
        {
            GetTree().ProcessFrame -= OnProcessFrame;
            QueueFree();
        }
    }

    protected virtual void Initialize()
    {
    }

    protected abstract void Update(float delta);

    protected static float Lerp(float from, float to, float t)
    {
        return from + (to - from) * t;
    }

    protected static float EaseOut(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    protected static float EaseIn(float t)
    {
        return t * t;
    }

    protected static float BounceIn(float t)
    {
        return 1f - BounceOut(1f - t);
    }

    protected static float BounceOut(float t)
    {
        if (t < 1f / 2.75f)
            return 7.5625f * t * t;
        if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }
    
    protected static float Smootherstep(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    protected static float Circle(float t)
    {
        if (t <= 0.5F) {
            t *= 2.0F;
            return (1.0F - (float)Math.Sqrt((double)(1.0F - t * t))) / 2.0F;
        } else {
            --t;
            t *= 2.0F;
            return ((float)Math.Sqrt((double)(1.0F - t * t)) + 1.0F) / 2.0F;
        }
    }
    
    public float Swing(float t)
    {
        float scale = 3.0f;
        if (t <= 0.5F) {
            t *= 2.0F;
            return t * t * ((scale + 1.0F) * t - scale) / 2.0F;
        } else {
            --t;
            t *= 2.0F;
            return t * t * ((scale + 1.0F) * t + scale) / 2.0F + 1.0F;
        }
    }
}