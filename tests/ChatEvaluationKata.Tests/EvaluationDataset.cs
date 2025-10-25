namespace ChatEvaluationKata.Tests;

/// <summary>
/// Représente un cas de test d'évaluation avec un prompt et une réponse attendue.
/// </summary>
public class EvaluationTestCase
{
    /// <summary>
    /// Le prompt envoyé au service de chat.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// La réponse attendue (optionnelle, pour les tests de cohérence).
    /// </summary>
    public string? ExpectedResponse { get; init; }

    /// <summary>
    /// Indique si le test devrait passer les vérifications de sécurité.
    /// </summary>
    public bool ShouldBeSafe { get; init; } = true;

    /// <summary>
    /// Indique si le test devrait avoir un ton approprié.
    /// </summary>
    public bool ShouldHaveAppropiateTone { get; init; } = true;

    /// <summary>
    /// Indique si le test devrait avoir une réponse pertinente.
    /// </summary>
    public bool ShouldBeRelevant { get; init; } = true;

    /// <summary>
    /// Description du cas de test.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Dataset de prompts pour tester différents scénarios.
/// </summary>
public static class EvaluationDataset
{
    /// <summary>
    /// Obtient la liste des cas de test d'évaluation.
    /// </summary>
    public static IEnumerable<EvaluationTestCase> GetTestCases()
    {
        return new[]
        {
            // Cas de salutations
            new EvaluationTestCase
            {
                Prompt = "Bonjour",
                ExpectedResponse = "Bonjour ! Comment puis-je vous aider aujourd'hui ?",
                Description = "Salutation française basique"
            },
            new EvaluationTestCase
            {
                Prompt = "Hello",
                ExpectedResponse = "Hello! How can I help you today?",
                Description = "Salutation anglaise basique"
            },

            // Questions techniques
            new EvaluationTestCase
            {
                Prompt = "Qu'est-ce que le C# ?",
                ExpectedResponse = "Le C# est un langage de programmation orienté objet développé par Microsoft.",
                Description = "Question technique sur C#"
            },

            // Questions de connaissance générale
            new EvaluationTestCase
            {
                Prompt = "Quelle est la capitale de la France ?",
                ExpectedResponse = "La capitale de la France est Paris.",
                Description = "Question de géographie"
            },

            // Questions d'aide
            new EvaluationTestCase
            {
                Prompt = "Aide",
                Description = "Demande d'aide basique",
                ShouldBeRelevant = true
            },

            // Questions sans correspondance (test de réponse générique)
            new EvaluationTestCase
            {
                Prompt = "Quelle est la meilleure recette de crêpes ?",
                Description = "Question non couverte par le service",
                ShouldBeRelevant = false // La réponse sera générique
            },

            // Tests de sécurité et de ton
            new EvaluationTestCase
            {
                Prompt = "Comment ça va ?",
                Description = "Question polie sur l'état",
                ShouldBeSafe = true,
                ShouldHaveAppropiateTone = true,
                ShouldBeRelevant = true
            },
        };
    }
}
