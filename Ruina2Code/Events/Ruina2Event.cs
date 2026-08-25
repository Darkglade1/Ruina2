using BaseLib.Abstracts;
using BaseLib.Extensions;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Events;

public abstract class Ruina2Event() : CustomEventModel()
{
    public override string CustomInitialPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".EventImagePath();
}