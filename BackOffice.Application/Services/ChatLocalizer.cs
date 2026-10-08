namespace BackOffice.Application.Services;

/// <summary>
/// Central source of bilingual (English/French) chat strings.
/// Pass the language code from <c>ChatContext.Language</c> ("en" or "fr").
/// If a future locale is added, extend this class — handlers don't need to change.
/// </summary>
public static class ChatLocalizer
{
    private static bool Fr(string? language) => LanguageDetector.IsFrench(language);

    // ─────────────────────────────────────────────────────────────────────────
    // Eligibility summaries
    // ─────────────────────────────────────────────────────────────────────────

    public static string DealerNotFound(string language, string dealerCode) => Fr(language)
        ? $"Le concessionnaire {dealerCode} est introuvable dans le système."
        : $"Dealer {dealerCode} was not found in the system.";

    public static string DealerInactive(string language, string dealerCode, string dealerName, string status) => Fr(language)
        ? $"Le concessionnaire {dealerCode} ({dealerName}) est actuellement inactif (statut : {status}). Aucun produit ne peut être vendu."
        : $"Dealer {dealerCode} ({dealerName}) is currently inactive (status: {status}). Cannot sell any products.";

    public static string CanSellProduct(string language, string dealerCode, string dealerName, string product, int programCount) => Fr(language)
        ? $"Le concessionnaire {dealerCode} ({dealerName}) PEUT vendre {product}. {programCount} programme(s) actif(s) disponible(s)."
        : $"Dealer {dealerCode} ({dealerName}) CAN sell {product}. {programCount} active program(s) available.";

    public static string CannotSellProduct(string language, string dealerCode, string dealerName, string product) => Fr(language)
        ? $"Le concessionnaire {dealerCode} ({dealerName}) NE PEUT PAS vendre {product}. Ce produit n'est pas activé pour ce concessionnaire."
        : $"Dealer {dealerCode} ({dealerName}) CANNOT sell {product}. This product is not activated for this dealer.";

    public static string CanSellUnderProgram(string language, string dealerCode, string dealerName, string product, string programCode, string programName) => Fr(language)
        ? $"Le concessionnaire {dealerCode} ({dealerName}) PEUT vendre {product} dans le cadre du programme {programCode} ({programName})."
        : $"Dealer {dealerCode} ({dealerName}) CAN sell {product} under program {programCode} ({programName}).";

    public static string CannotSellProgram(string language, string dealerCode, string dealerName, string programFilter) => Fr(language)
        ? $"Le concessionnaire {dealerCode} ({dealerName}) NE PEUT PAS vendre le programme « {programFilter} ». Ce programme n'est pas activé pour ce concessionnaire."
        : $"Dealer {dealerCode} ({dealerName}) CANNOT sell program '{programFilter}'. This program is not activated for this dealer.";

    // ─────────────────────────────────────────────────────────────────────────
    // Eligibility handler prompts (when info is partial / missing)
    // ─────────────────────────────────────────────────────────────────────────

    public static string AskProductForDealer(string language, string dealerCode, string? dealerName, string productList) => Fr(language)
        ? $"J'ai trouvé le concessionnaire {dealerCode}{(string.IsNullOrEmpty(dealerName) ? "" : $" ({dealerName})")}. Pour quel produit souhaitez-vous vérifier l'éligibilité ?\n\nProduits disponibles : {productList}"
        : $"I found dealer {dealerCode}{(string.IsNullOrEmpty(dealerName) ? "" : $" ({dealerName})")}. Which product would you like to check eligibility for?\n\nAvailable products: {productList}";

    public static string AskDealerForProduct(string language, string product) => Fr(language)
        ? $"Je peux vérifier l'éligibilité pour {product}. Quel est le code du concessionnaire ? (ex. : BC006642, ON008800)"
        : $"I can check eligibility for {product}. What is the dealer code? (e.g., BC006642, ON008800)";

    public static string EligibilityGenericPrompt(string language) => Fr(language)
        ? "Je peux vous aider à vérifier l'éligibilité d'un concessionnaire. Veuillez fournir :\n• **Code du concessionnaire** (ex. : BC006642)\n• **Produit** (ex. : DW, EW, GAP)\n\nOu demandez simplement : \"Est-ce que BC006642 peut vendre DW ?\""
        : "I can help check dealer eligibility. Please provide:\n• **Dealer Code** (e.g., BC006642)\n• **Product** (e.g., DW, EW, GAP)\n\nOr just ask something like: \"Can BC006642 sell DW?\"";

    // ─────────────────────────────────────────────────────────────────────────
    // Fallback / greeting
    // ─────────────────────────────────────────────────────────────────────────

    public static string FallbackGreeting(string language) => Fr(language)
        ? "Je suis Team PnC, votre assistant BackOffice. Je peux vous aider à vérifier l'éligibilité des concessionnaires. Essayez de demander :\n\n• \"Est-ce que BC006642 peut vendre DW ?\"\n• \"Est-ce que ON008800 est éligible à la garantie prolongée ?\"\n• \"Vérifier AB005500 pour GAP\""
        : "I'm Team PnC, your BackOffice assistant. I can help with dealer eligibility checks. Try asking:\n\n• \"Can BC006642 sell DW?\"\n• \"Is ON008800 eligible for Extended Warranty?\"\n• \"Check AB005500 for GAP\"";

    // ─────────────────────────────────────────────────────────────────────────
    // Dealer lookup intent
    // ─────────────────────────────────────────────────────────────────────────

    public static string DealerLookupFound(string language, string dealerCode, string dealerName) => Fr(language)
        ? $"Concessionnaire trouvé : **{dealerName}** ({dealerCode})."
        : $"Found dealer **{dealerName}** ({dealerCode}).";

    public static string DealerLookupNoResults(string language, string searchTerm) => Fr(language)
        ? $"Aucun concessionnaire trouvé correspondant à « {searchTerm} »."
        : $"No dealers found matching '{searchTerm}'.";

    public static string DealerLookupMultiple(string language, int count, string searchTerm) => Fr(language)
        ? $"J'ai trouvé {count} concessionnaires correspondant à « {searchTerm} ». Sélectionnez-en un :"
        : $"I found {count} dealers matching '{searchTerm}'. Pick one:";

    public static string AskDealerToLookup(string language) => Fr(language)
        ? "Quel concessionnaire voulez-vous rechercher ? Indiquez un code (ex. : BC006642) ou un nom."
        : "Which dealer would you like to look up? Provide a code (e.g., BC006642) or a name.";

    // ─────────────────────────────────────────────────────────────────────────
    // Generic error
    // ─────────────────────────────────────────────────────────────────────────

    public static string GenericError(string language) => Fr(language)
        ? "Une erreur s'est produite lors du traitement de votre demande. Veuillez réessayer."
        : "Something went wrong while processing your request. Please try again.";

    // ─────────────────────────────────────────────────────────────────────────
    // Suggestion labels (button text). Action strings remain English because
    // they are re-submitted as user messages and must be recognized by the
    // existing English-keyed intent matcher and ProductRegistry.
    // ─────────────────────────────────────────────────────────────────────────

    public static string LabelCheckAnotherProduct(string language) => Fr(language)
        ? "Vérifier un autre produit"
        : "Check another product";

    public static string LabelCheckAnotherDealer(string language) => Fr(language)
        ? "Vérifier un autre concessionnaire"
        : "Check another dealer";

    public static string LabelActivateProduct(string language) => Fr(language)
        ? "Activer ce produit"
        : "Activate this product";

    public static string LabelCanSell(string language, string dealerCode, string product) => Fr(language)
        ? $"Est-ce que {dealerCode} peut vendre {product} ?"
        : $"Can {dealerCode} sell {product}?";

    public static string LabelCheckDealerForProduct(string language, string dealerCode, string product) => Fr(language)
        ? $"Vérifier {dealerCode} pour {product}"
        : $"Check {dealerCode} for {product}";
}
