namespace BankApp.Core.Attributes;

/// <summary>Metadane typu konta — czytane przez refleksję do budowy listy typów w GUI.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AccountTypeInfoAttribute : Attribute
{
    public string Key { get; }
    public string DisplayName { get; }
    public string Description { get; }

    public AccountTypeInfoAttribute(string key, string displayName, string description)
    {
        Key = key;
        DisplayName = displayName;
        Description = description;
    }
}
