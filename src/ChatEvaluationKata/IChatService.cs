namespace ChatEvaluationKata;

/// <summary>
/// Interface pour un service de chat IA.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Obtient une réponse à un prompt donné.
    /// </summary>
    /// <param name="prompt">Le prompt de l'utilisateur.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La réponse générée par le service de chat.</returns>
    Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken = default);
}
