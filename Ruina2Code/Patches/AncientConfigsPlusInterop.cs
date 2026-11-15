using BaseLib.Abstracts;
using BaseLib.Utils.ModInterop;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Patches;

[ModInterop("AncientConfigsPlus", "AncientConfigsPlus.AncientConfigsPlusCode.ExceptionAncientsExtension")]
public static class AncientConfigsPlusInterop 
{
    public static void AddActSpecificAncientList(this CustomAncientModel ancientModel, ModelId[] acts) 
    {
    }
}