# ChatEvaluationKata

Un kata .NET qui démontre l'évaluation automatique de la qualité des réponses d'un service de chat IA en utilisant `Microsoft.Extensions.AI.Evaluation`.

## Description

Ce projet implémente un service de chat factice (`FakeChatService`) qui renvoie des réponses simples et prédéfinies. Des tests d'évaluation automatiques vérifient la qualité des réponses selon plusieurs critères :

- **Pertinence** : La réponse est-elle pertinente par rapport à la question ?
- **Sécurité** : La réponse est-elle exempte de violence et de propos haineux ?
- **Ton** : Le ton de la réponse est-il approprié (poli et professionnel) ?
- **Cohérence** : La réponse correspond-elle à la réponse attendue ?

## Structure du projet

```
ChatEvaluationKata/
├── src/
│   └── ChatEvaluationKata/
│       ├── IChatService.cs              # Interface du service de chat
│       ├── FakeChatService.cs           # Implémentation factice
│       └── Evaluators/
│           ├── RelevanceEvaluator.cs    # Évaluation de la pertinence
│           ├── SafetyEvaluator.cs       # Évaluation de la sécurité
│           ├── ToneEvaluator.cs         # Évaluation du ton
│           └── ConsistencyEvaluator.cs  # Évaluation de la cohérence
└── tests/
    └── ChatEvaluationKata.Tests/
        ├── ChatEvaluationTests.cs       # Tests d'évaluation individuels
        ├── QualityGateTests.cs          # Tests de porte de qualité
        └── EvaluationDataset.cs         # Dataset de prompts de test
```

## Prérequis

- .NET 9.0 SDK ou supérieur
- Microsoft.Extensions.AI.Evaluation 9.10.0+

## Installation et exécution

### Cloner le projet

```bash
git clone https://github.com/Thony-Crz/ChatEvaluationKata.git
cd ChatEvaluationKata
```

### Restaurer les dépendances

```bash
dotnet restore
```

### Compiler le projet

```bash
dotnet build
```

### Exécuter les tests

```bash
dotnet test
```

## Évaluateurs implémentés

### 1. RelevanceEvaluator
Vérifie si la réponse est pertinente par rapport à la question posée. Une réponse générique ou non pertinente sera marquée comme échouée.

**Métriques produites :**
- `Relevance` (BooleanMetric) : Indique si la réponse est pertinente

### 2. SafetyEvaluator
Vérifie l'absence de contenu violent et de propos haineux dans les réponses.

**Métriques produites :**
- `Safety` (BooleanMetric) : Sécurité globale de la réponse
- `HasViolence` (BooleanMetric) : Présence de contenu violent
- `HasHateSpeech` (BooleanMetric) : Présence de propos haineux

### 3. ToneEvaluator
Évalue le ton de la réponse pour s'assurer qu'il est poli et professionnel.

**Métriques produites :**
- `Tone` (BooleanMetric) : Ton global approprié
- `IsPolite` (BooleanMetric) : Politesse de la réponse
- `IsProfessional` (BooleanMetric) : Professionnalisme de la réponse

### 4. ConsistencyEvaluator
Compare la réponse obtenue avec une réponse attendue pour vérifier la cohérence.

**Métriques produites :**
- `Consistency` (BooleanMetric) : Cohérence avec la réponse attendue
- `SimilarityScore` (NumericMetric) : Score de similarité (0.0 à 1.0)

## Dataset de test

Le projet inclut un dataset de prompts variés (`EvaluationDataset.cs`) couvrant différents scénarios :

- Salutations (français et anglais)
- Questions techniques (ex: "Qu'est-ce que le C# ?")
- Questions de connaissance générale (ex: capitales)
- Demandes d'aide
- Questions non couvertes par le service (réponses génériques attendues)

## Porte de qualité (Quality Gate)

Les tests dans `QualityGateTests.cs` définissent des seuils de qualité minimaux. Le build échoue si :

- **Pertinence** : Moins de 80% des réponses sont pertinentes
- **Sécurité** : Moins de 100% des réponses sont sûres (tolérance zéro pour le contenu inapproprié)
- **Ton** : Moins de 80% des réponses ont un ton approprié
- **Cohérence** : Les questions avec réponses attendues doivent avoir au moins 50% de similarité

### Exemple de sortie de porte de qualité

```
=== RAPPORT DE QUALITÉ ===
Pertinence: 6/7 (85.7%)
Sécurité: 7/7 (100.0%)
Ton: 7/7 (100.0%)

✓ Tous les seuils de qualité sont respectés
```

## Utilisation avec CompositeEvaluator

Vous pouvez combiner plusieurs évaluateurs en un seul :

```csharp
var compositeEvaluator = new CompositeEvaluator(
    new RelevanceEvaluator(),
    new SafetyEvaluator(),
    new ToneEvaluator()
);

var result = await compositeEvaluator.EvaluateAsync(messages, chatResponse);
```

## Extension du kata

Pour ajouter de nouveaux scénarios de test :

1. Ajoutez de nouveaux cas dans `EvaluationDataset.GetTestCases()`
2. Créez de nouveaux évaluateurs dans le dossier `Evaluators/`
3. Ajustez les seuils de qualité dans `QualityGateTests.cs` si nécessaire

## Licence

Ce projet est un kata éducatif à des fins de démonstration.