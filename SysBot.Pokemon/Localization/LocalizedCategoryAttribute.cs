using System.ComponentModel;

namespace SysBot.Pokemon.Localization;

/// <summary>
/// Resolves the PropertyGrid category header through <see cref="HubStrings"/> at first access.
/// Pass the original English category name (e.g. "Operation"); the lookup key becomes
/// "Category_Operation". Missing keys fall back to the supplied value, so categories that
/// haven't been translated yet still render in English.
/// </summary>
public sealed class HubCategoryAttribute : CategoryAttribute
{
    public HubCategoryAttribute(string englishName) : base(englishName) { }

    protected override string GetLocalizedString(string value) =>
        HubStrings.Get("Category_" + value, value);
}
