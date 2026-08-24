using BaseLib.Abstracts;
using BaseLib.Extensions;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Enchantments;

public abstract class Ruina2Enchantment : CustomEnchantmentModel
{
    protected override string CustomIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".EnchantmentImagePath();
}