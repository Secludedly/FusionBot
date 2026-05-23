using System.ComponentModel;

namespace SysBot.Pokemon.Localization;

/// <summary>
/// Drop-in replacement for <see cref="DescriptionAttribute"/> that resolves its value through
/// <see cref="HubStrings"/>. Named with a Hub prefix to avoid clashing with
/// <c>PKHeX.Core.LocalizedDescriptionAttribute</c> in Settings files that import both namespaces.
/// </summary>
public sealed class HubDescriptionAttribute : DescriptionAttribute
{
    private readonly string _key;
    private bool _loaded;

    public HubDescriptionAttribute(string key) : base(key)
    {
        _key = key;
    }

    public override string Description
    {
        get
        {
            if (!_loaded)
            {
                DescriptionValue = HubStrings.Get(_key, DescriptionValue);
                _loaded = true;
            }
            return base.Description;
        }
    }
}
