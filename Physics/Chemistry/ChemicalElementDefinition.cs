namespace TheSingularityWorkshop.Physics.Chemistry;

/// <summary>
/// Source-attributed identity for a chemical element. This is not an isotope record,
/// standard atomic-weight value, material definition, or reaction model.
/// </summary>
/// <param name="AtomicNumber">Proton count, from 1 through 118 for currently recognized elements.</param>
/// <param name="Symbol">Canonical element symbol, including capitalization.</param>
/// <param name="Name">Element name in the declared language/source convention.</param>
/// <param name="SourceId">Stable identifier for the dataset or source.</param>
/// <param name="SourceVersion">Dataset revision or version used for this record.</param>
/// <param name="SourceUrl">Reference URL or stable citation for this record.</param>
/// <param name="LicenseId">License or usage terms identifier for the source data.</param>
public sealed record ChemicalElementDefinition(
    int AtomicNumber,
    string Symbol,
    string Name,
    string SourceId,
    string SourceVersion,
    string SourceUrl,
    string LicenseId)
{
    /// <summary>Returns whether identity and source-attribution fields are present and admissible.</summary>
    public bool IsValid =>
        AtomicNumber is >= 1 and <= 118 &&
        IsValidSymbol(Symbol) &&
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(SourceId) &&
        !string.IsNullOrWhiteSpace(SourceVersion) &&
        !string.IsNullOrWhiteSpace(SourceUrl) &&
        !string.IsNullOrWhiteSpace(LicenseId);

    /// <summary>Throws when the element identity or provenance is invalid.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException(
                "Element identity and source attribution must be valid.",
                nameof(ChemicalElementDefinition));
    }

    private static bool IsValidSymbol(string? symbol) =>
        symbol is { Length: >= 1 and <= 2 } &&
        symbol[0] is >= 'A' and <= 'Z' &&
        (symbol.Length == 1 || symbol[1] is >= 'a' and <= 'z');
}
