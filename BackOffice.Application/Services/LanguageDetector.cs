using System.Text.RegularExpressions;

namespace BackOffice.Application.Services;

/// <summary>
/// Lightweight heuristic detector for chat message language.
/// Returns "fr" when French diacritics or common French function words are present,
/// otherwise defaults to "en". This is intentionally simple and dependency-free;
/// it is sufficient for routing localized responses in the chat pipeline.
/// </summary>
public static class LanguageDetector
{
    public const string English = "en";
    public const string French = "fr";

    // French-specific accented characters.
    private static readonly Regex FrenchDiacritics = new(
        @"[àâäçéèêëîïôöùûüÿœæÀÂÄÇÉÈÊËÎÏÔÖÙÛÜŸŒÆ]",
        RegexOptions.Compiled);

    // Common French function words and chat-relevant verbs/nouns.
    // Matched as whole tokens (case-insensitive) to avoid false positives on
    // English words that happen to contain these substrings (e.g. "le" in "select").
    private static readonly string[] FrenchTokens =
    [
        "est-ce", "qu'est", "peut", "peux", "pouvez", "vendre", "vend",
        "concessionnaire", "concessionnaires", "produit", "produits",
        "programme", "programmes", "éligible", "eligible", "éligibilité",
        "activer", "désactiver", "désactivé", "activé",
        "chercher", "cherche", "trouver", "trouve", "rechercher", "recherche",
        "montrer", "montre", "afficher", "affiche", "voir",
        "le", "la", "les", "un", "une", "des", "du",
        "est", "sont", "pour", "avec", "sans", "ou", "et",
        "quel", "quelle", "quels", "quelles", "quoi", "comment",
        "bonjour", "salut", "merci"
    ];

    private static readonly Regex FrenchTokenRegex = new(
        @"(?<![\p{L}])(" + string.Join("|", FrenchTokens) + @")(?![\p{L}])",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Returns "fr" or "en". Defaults to "en" for null/empty input or any
    /// non-French text (including Chinese, Spanish, etc.).
    /// </summary>
    public static string Detect(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return English;

        // Any French-specific diacritic is a strong signal.
        if (FrenchDiacritics.IsMatch(message))
            return French;

        // Otherwise require at least one whole-word French token match.
        return FrenchTokenRegex.IsMatch(message) ? French : English;
    }

    /// <summary>True if the supplied language code is French.</summary>
    public static bool IsFrench(string? language) =>
        string.Equals(language, French, StringComparison.OrdinalIgnoreCase);
}
