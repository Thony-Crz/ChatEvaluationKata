using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace ChatEvaluationKata.Evaluators;

/// <summary>
/// Évaluateur de cohérence qui vérifie si la réponse correspond à une réponse attendue.
/// </summary>
public class ConsistencyEvaluator : IEvaluator
{
    private readonly string? _expectedResponse;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ConsistencyEvaluator"/>.
    /// </summary>
    /// <param name="expectedResponse">La réponse attendue (optionnelle).</param>
    public ConsistencyEvaluator(string? expectedResponse = null)
    {
        _expectedResponse = expectedResponse;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<string> EvaluationMetricNames => new[] { "Consistency", "SimilarityScore" };

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

        if (string.IsNullOrWhiteSpace(_expectedResponse))
        {
            // Pas de réponse attendue, on considère comme cohérent
            metrics.Add(new BooleanMetric(
                "Consistency",
                true,
                "Aucune réponse attendue fournie, vérification ignorée."));

            metrics.Add(new NumericMetric(
                "SimilarityScore",
                1.0,
                "Score de similarité non applicable sans réponse attendue."));
        }
        else
        {
            // Calculer la similarité
            var similarity = CalculateSimilarity(responseText, _expectedResponse);
            
            metrics.Add(new NumericMetric(
                "SimilarityScore",
                similarity,
                $"Score de similarité avec la réponse attendue : {similarity:F2}"));

            var isConsistent = similarity >= 0.5; // Seuil de cohérence à 50%
            metrics.Add(new BooleanMetric(
                "Consistency",
                isConsistent,
                isConsistent
                    ? "La réponse est cohérente avec la réponse attendue."
                    : "La réponse n'est pas cohérente avec la réponse attendue."));
        }

        return ValueTask.FromResult(new EvaluationResult(metrics));
    }

    /// <summary>
    /// Calcule un score de similarité simple entre deux textes (0.0 à 1.0).
    /// </summary>
    private double CalculateSimilarity(string text1, string text2)
    {
        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
        {
            return 0.0;
        }

        // Normaliser les textes
        var normalized1 = text1.ToLowerInvariant().Trim();
        var normalized2 = text2.ToLowerInvariant().Trim();

        // Similarité exacte
        if (normalized1 == normalized2)
        {
            return 1.0;
        }

        // Similarité basée sur les mots communs (Jaccard simplifiée)
        var words1 = normalized1.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var words2 = normalized2.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        if (words1.Count == 0 && words2.Count == 0)
        {
            return 1.0;
        }

        if (words1.Count == 0 || words2.Count == 0)
        {
            return 0.0;
        }

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union;
    }
}
