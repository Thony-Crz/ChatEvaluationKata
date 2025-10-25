namespace ChatEvaluationKata;

/// <summary>
/// Implémentation factice d'un service de chat qui renvoie des réponses simples et prédéfinies.
/// </summary>
public class FakeChatService : IChatService
{
    private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase)
    {
        // Réponses de base
        { "Bonjour", "Bonjour ! Comment puis-je vous aider aujourd'hui ?" },
        { "Hello", "Hello! How can I help you today?" },
        { "Comment ça va ?", "Je vais bien, merci ! Et vous ?" },
        { "How are you?", "I'm doing well, thank you! How are you?" },
        
        // Questions techniques
        { "Qu'est-ce que le C# ?", "Le C# est un langage de programmation orienté objet développé par Microsoft." },
        { "What is C#?", "C# is an object-oriented programming language developed by Microsoft." },
        
        // Questions de capital
        { "Quelle est la capitale de la France ?", "La capitale de la France est Paris." },
        { "What is the capital of France?", "The capital of France is Paris." },
        
        // Questions d'aide
        { "Aide", "Je suis un assistant virtuel. Posez-moi des questions et je ferai de mon mieux pour y répondre." },
        { "Help", "I'm a virtual assistant. Ask me questions and I'll do my best to answer them." },
        
        // Météo
        { "Quel temps fait-il ?", "Je ne peux pas vérifier la météo en temps réel, mais vous pouvez consulter un service météo en ligne." },
        { "What's the weather?", "I can't check the weather in real-time, but you can consult an online weather service." }
    };

    /// <inheritdoc/>
    public Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Le prompt ne peut pas être vide.", nameof(prompt));
        }

        // Chercher une correspondance exacte
        if (_responses.TryGetValue(prompt.Trim(), out var response))
        {
            return Task.FromResult(response);
        }

        // Chercher une correspondance partielle
        foreach (var kvp in _responses)
        {
            if (prompt.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(kvp.Value);
            }
        }

        // Réponse par défaut si aucune correspondance
        return Task.FromResult("Je ne suis pas sûr de comprendre votre question. Pourriez-vous la reformuler ?");
    }
}
