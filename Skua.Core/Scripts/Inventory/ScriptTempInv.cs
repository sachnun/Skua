using Skua.Core.Flash;
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;

namespace Skua.Core.Scripts;

#pragma warning disable CS0169 // Field is never used
public partial class ScriptTempInv : IScriptTempInv
{
    private readonly Lazy<IFlashUtil> _lazyFlash;
    private IFlashUtil Flash => _lazyFlash.Value;

    public ScriptTempInv(Lazy<IFlashUtil> flash)
    {
        _lazyFlash = flash;
    }

    [ObjectBinding("world.myAvatar.tempitems", Default = "new()")]
    private List<ItemBase> _items = new();
}