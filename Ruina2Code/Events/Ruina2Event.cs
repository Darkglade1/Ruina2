using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Events;

public abstract class Ruina2Event() : CustomEventModel()
{
    public override string CustomInitialPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".EventImagePath();
    
    public override bool IsAllowed(IRunState runState) => base.IsAllowed(runState) && IsAllowedForAct(runState.Act);

    protected virtual bool IsAllowedForAct(ActModel act)
    {
        return true;
    }
}