using System.ComponentModel;

namespace SysBot.Pokemon.Localization;

/// <summary>
/// Drop-in replacement for <see cref="DisplayNameAttribute"/> backed by <see cref="HubStrings"/>.
/// </summary>
public sealed class HubDisplayNameAttribute : DisplayNameAttribute
{
    private readonly string _key;
    private bool _loaded;

    public HubDisplayNameAttribute(string key) : base(key)
    {
        _key = key;
    }

    public override string DisplayName
    {
        get
        {
            if (!_loaded)
            {
                DisplayNameValue = HubStrings.Get(_key, DisplayNameValue);
                _loaded = true;
            }
            return base.DisplayName;
        }
    }
}
