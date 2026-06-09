# Cartes Epic Spell Wars — Donjon du Poulpe Sanguinaire

## Principe

Chaque entrée JSON est une **carte**. Le champ `texte` reproduit **le texte imprimé sur la carte physique, au mot près** : c'est la **source de vérité textuelle**, pas un modèle exécutable. Tout l'effet est dans `texte` — y compris le coût en Sang (inline `… Payez N 🩸 : …`), les bonus `Donjon : …`, les `Réaction : …`, la cible et le Jet de puissance des Destinations.

**On ne structure PAS l'effet dans le JSON.** La structure exécutable (cible, valeur, coût, tranches, déclencheurs) est **dérivée du texte côté C#**, dans `Carte.Effets` — jamais re-paraphrasée dans les JSON. Le `trigger` des Trésors / Sorciers crevés se **reconnaît dans le texte** (pas de champ JSON ; à défaut on le code en C#).

Seuls les champs **non-textuels** (icônes / chiffres imprimés, ou nécessaires au moteur) sont des clés à part.

## Schéma

### sources.json / qualites.json
```json
{
  "id": "EP2-XXX",
  "nom": "Nom de la carte",
  "type": "Source" | "Qualité",
  "glyphe": "Arcane" | "Ténèbres" | "Élémentaire" | "Illusion" | "Primaire",
  "exemplaires": 2,
  "texte": "Texte verbatim de la carte. Payez N 🩸 : effet conditionnel. Donjon : bonus. Réaction : effet."
}
```

### destinations.json
```json
{
  "id": "EP2-XXX",
  "nom": "Nom de la carte",
  "type": "Destination",
  "glyphe": "...",
  "initiative": 12,
  "exemplaires": 2,
  "texte": "Cible : Adversaire de gauche.\nJet de puissance\n1-4  …\n5-9  …\n10+  … GARDEZ."
}
```
Le `texte` d'une Destination est multiligne : ligne `Cible : …`, en-tête `Jet de puissance`, puis les tranches `1-4` / `5-9` / `10+` (seuils du dé), avec `GARDEZ.` et tout coût (`Payez N 🩸 : GARDEZ.` pour Gracula) tels qu'imprimés.

### tresors.json / sorciers_creves.json / magie_feroce.json
```json
{
  "id": "EP2-XXX",
  "nom": "Nom de la carte",
  "type": "Trésor" | "SorcierCrevé" | "MagieFeroce",
  "exemplaires": 1,
  "texte": "Texte verbatim de la carte."
}
```
Pas de `trigger` : il est déduit du texte (Passif / SurInitiative / Immédiat / MancheSuivante).

## Champs

| Champ | Présent sur | Rôle |
|---|---|---|
| `id` | toutes | identifiant `EP2-XXX` |
| `nom` | toutes | nom imprimé |
| `type` | toutes | `Source` / `Qualité` / `Destination` / `Trésor` / `SorcierCrevé` / `MagieFeroce` |
| `glyphe` | sorts | `Arcane` / `Ténèbres` / `Élémentaire` / `Illusion` / `Primaire` |
| `initiative` | Destinations | ordre de résolution |
| `exemplaires` | toutes | nb d'exemplaires dans la pioche |
| `texte` | toutes | **texte verbatim de la carte** (effet complet) |

## Composition

| Fichier | Cartes uniques | Exemplaires |
|---|---|---|
| sources.json | 20 | 40 |
| qualites.json | 20 | 40 |
| destinations.json | 20 | 40 |
| tresors.json | 25 | 25 |
| sorciers_creves.json | 8 | 25 |
| magie_feroce.json | 1 | 8 |

Pioche principale = 60 sorts (×2) + 8 Magie féroce. Pioche Trésor = 25 (×1). Pioche Sorcier crevé = 8 uniques (multiplicités variées, 25 cartes).
