using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace ChatEvaluationKata.Evaluators;

/// <summary>
/// Évaluateur de pertinence qui vérifie si la réponse est pertinente par rapport à la question.
/// </summary>
public class RelevanceEvaluator : IEvaluator
{
    /// <inheritdoc/>
    public IReadOnlyCollection<string> EvaluationMetricNames => new[] { "Relevance" };

    /// <inheritdoc/>
    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        var metrics = new List<EvaluationMetric>();

        // Extraire le dernier message utilisateur et la réponse
        var userMessage = messages.LastOrDefault(m => m.Role == ChatRole.User);
        var responseText = modelResponse.Text ?? string.Empty;

        if (userMessage == null || string.IsNullOrWhiteSpace(userMessage.Text))
        {
            metrics.Add(new BooleanMetric(
                "Relevance",
                false,
                "Aucun message utilisateur trouvé dans la conversation."));
        }
        else if (string.IsNullOrWhiteSpace(responseText))
        {
            metrics.Add(new BooleanMetric(
                "Relevance",
                false,
                "La réponse du modèle est vide."));
        }
        else
        {
            // Logique simple de pertinence : la réponse ne doit pas être générique
            var isRelevant = !IsGenericResponse(responseText, userMessage.Text);
            
            metrics.Add(new BooleanMetric(
                "Relevance",
                isRelevant,
                isRelevant
                    ? "La réponse est pertinente par rapport à la question."
                    : "La réponse est trop générique ou non pertinente."));
        }

        return ValueTask.FromResult(new EvaluationResult(metrics));
    }

    private bool IsGenericResponse(string response, string question)
    {
        var lowerResponse = response.ToLowerInvariant();
        var genericPhrases = new[]
        {
            "je ne suis pas sûr",
            "je ne comprends pas",
            "pourriez-vous reformuler"
        };

        // Si la réponse contient une phrase générique et n'a pas de contenu spécifique
        return genericPhrases.Any(phrase => lowerResponse.Contains(phrase)) &&
               response.Length < 100;
    }
}
