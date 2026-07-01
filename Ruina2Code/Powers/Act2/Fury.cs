using MegaCrit.Sts2.Core.Entities.Powers;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class Fury() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;
}