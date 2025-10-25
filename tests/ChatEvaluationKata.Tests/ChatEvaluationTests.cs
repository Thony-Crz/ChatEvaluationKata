using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using ChatEvaluationKata.Evaluators;
using Xunit;

namespace ChatEvaluationKata.Tests;

/// <summary>
/// Tests d'évaluation de la qualité des réponses du service de chat.
/// </summary>
public class ChatEvaluationTests
{
    private readonly IChatService _chatService;

    public ChatEvaluationTests()
    {
        _chatService = new FakeChatService();
    }

    [Fact]
    public async Task GetResponseAsync_WithValidPrompt_ReturnsResponse()
    {
        // Arrange
        var prompt = "Bonjour";

        // Act
        var response = await _chatService.GetResponseAsync(prompt);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response);
    }

    [Fact]
    public async Task GetResponseAsync_WithEmptyPrompt_ThrowsException()
    {
        // Arrange
        var prompt = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _chatService.GetResponseAsync(prompt));
    }

    [Theory]
    [MemberData(nameof(GetEvaluationTestCases))]
    public async Task EvaluateResponse_WithDataset_MeetsQualityStandards(EvaluationTestCase testCase)
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, testCase.Prompt)
        };

        var response = await _chatService.GetResponseAsync(testCase.Prompt);
        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, response)]);

        // Créer les évaluateurs
        var relevanceEvaluator = new RelevanceEvaluator();
        var safetyEvaluator = new SafetyEvaluator();
        var toneEvaluator = new ToneEvaluator();
        var consistencyEvaluator = new ConsistencyEvaluator(testCase.ExpectedResponse);

        // Act - Évaluer la pertinence
        var relevanceResult = await relevanceEvaluator.EvaluateAsync(messages, chatResponse);
        var relevanceMetric = relevanceResult.Get<BooleanMetric>("Relevance");

        // Act - Évaluer la sécurité
        var safetyResult = await safetyEvaluator.EvaluateAsync(messages, chatResponse);
        var safetyMetric = safetyResult.Get<BooleanMetric>("Safety");

        // Act - Évaluer le ton
        var toneResult = await toneEvaluator.EvaluateAsync(messages, chatResponse);
        var toneMetric = toneResult.Get<BooleanMetric>("Tone");

        // Act - Évaluer la cohérence si une réponse attendue est fournie
        EvaluationResult? consistencyResult = null;
        BooleanMetric? consistencyMetric = null;
        if (!string.IsNullOrWhiteSpace(testCase.ExpectedResponse))
        {
            consistencyResult = await consistencyEvaluator.EvaluateAsync(messages, chatResponse);
            consistencyMetric = consistencyResult.Get<BooleanMetric>("Consistency");
        }

        // Assert - Vérifier les métriques selon les attentes du test
        Assert.NotNull(relevanceMetric);
        if (testCase.ShouldBeRelevant)
        {
            Assert.True(relevanceMetric.Value,
                $"[{testCase.Description}] La réponse devrait être pertinente. Raison: {relevanceMetric.Reason}");
        }

        Assert.NotNull(safetyMetric);
        if (testCase.ShouldBeSafe)
        {
            Assert.True(safetyMetric.Value,
                $"[{testCase.Description}] La réponse devrait être sûre. Raison: {safetyMetric.Reason}");
        }

        Assert.NotNull(toneMetric);
        if (testCase.ShouldHaveAppropiateTone)
        {
            Assert.True(toneMetric.Value,
                $"[{testCase.Description}] Le ton devrait être approprié. Raison: {toneMetric.Reason}");
        }

        if (consistencyMetric != null)
        {
            Assert.True(consistencyMetric.Value,
                $"[{testCase.Description}] La réponse devrait être cohérente avec la réponse attendue. Raison: {consistencyMetric.Reason}");
        }
    }

    [Fact]
    public async Task EvaluateResponse_WithCompositeEvaluator_AllMetricsReturned()
    {
        // Arrange
        var prompt = "Bonjour";
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, prompt)
        };

        var response = await _chatService.GetResponseAsync(prompt);
        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, response)]);

        var compositeEvaluator = new CompositeEvaluator(
            new RelevanceEvaluator(),
            new SafetyEvaluator(),
            new ToneEvaluator()
        );

        // Act
        var result = await compositeEvaluator.EvaluateAsync(messages, chatResponse);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Metrics);

        // Vérifier que toutes les métriques attendues sont présentes
        Assert.True(result.TryGet<BooleanMetric>("Relevance", out _));
        Assert.True(result.TryGet<BooleanMetric>("Safety", out _));
        Assert.True(result.TryGet<BooleanMetric>("HasViolence", out _));
        Assert.True(result.TryGet<BooleanMetric>("HasHateSpeech", out _));
        Assert.True(result.TryGet<BooleanMetric>("Tone", out _));
        Assert.True(result.TryGet<BooleanMetric>("IsPolite", out _));
        Assert.True(result.TryGet<BooleanMetric>("IsProfessional", out _));
    }

    public static IEnumerable<object[]> GetEvaluationTestCases()
    {
        return EvaluationDataset.GetTestCases().Select(tc => new object[] { tc });
    }
}
