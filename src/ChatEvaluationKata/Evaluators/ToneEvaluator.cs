using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace ChatEvaluationKata.Evaluators;

/// <summary>
/// Évaluateur de ton qui vérifie si le ton de la réponse est approprié (poli, professionnel).
/// </summary>
public class ToneEvaluator : IEvaluator
{
    /// <inheritdoc/>
    public IReadOnlyCollection<string> EvaluationMetricNames => new[] { "Tone", "IsPolite", "IsProfessional" };

    private readonly HashSet<string> _politeWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "bonjour", "merci", "s'il vous plaît", "svp", "excusez-moi", "pardon",
        "hello", "thank you", "thanks", "please", "excuse me", "sorry"
    };

    private readonly HashSet<string> _rudeWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "idiot", "stupide", "imbécile", "crétin", "nul",
        "stupid", "idiot", "dumb", "moron", "fool"
    };

    /// <inheritdoc/>
    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        var metrics = new List<EvaluationMetric>();
        var responseText = modelResponse.Text ?? string.Empty;

        // Vérifier la politesse
        var hasPoliteWords = ContainsWords(responseText, _politeWords);
        var hasRudeWords = ContainsWords(responseText, _rudeWords);
        
        var isPolite = hasPoliteWords || (!hasRudeWords && responseText.Length > 0);
        metrics.Add(new BooleanMetric(
            "IsPolite",
            isPolite,
            isPolite
                ? "Le ton de la réponse est poli."
                : "Le ton de la réponse n'est pas poli."));

        // Vérifier le professionnalisme (absence de mots grossiers)
        var isProfessional = !hasRudeWords;
        metrics.Add(new BooleanMetric(
            "IsProfessional",
            isProfessional,
            isProfessional
                ? "Le ton de la réponse est professionnel."
                : "Le ton de la réponse n'est pas professionnel."));

        // Métrique de ton globale
        var hasProfessionalTone = isPolite && isProfessional;
        metrics.Add(new BooleanMetric(
            "Tone",
            hasProfessionalTone,
            hasProfessionalTone
                ? "Le ton de la réponse est approprié (poli et professionnel)."
                : "Le ton de la réponse n'est pas approprié."));

        return ValueTask.FromResult(new EvaluationResult(metrics));
    }

    private bool ContainsWords(string text, HashSet<string> words)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        // Séparer le texte en mots et vérifier
        var textWords = text.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        return textWords.Any(word => words.Contains(word));
    }
}
