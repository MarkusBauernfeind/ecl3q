# ECL₃^Q — Anwenderhandbuch

Dieses Dokument erklärt, wie man mit dem Framework arbeitet.
Es setzt voraus, dass `dotnet build` und `dotnet test` erfolgreich durchlaufen.

---

## Die wichtigste Frage zuerst: Was ist U?

ECL₃^Q hat drei Wahrheitswerte: **T** (wahr), **F** (falsch), **U** (unbestimmt).

U ist **keine Unwissenheit**. U bedeutet: die Sache ist noch nicht entschieden —
nicht weil wir es nicht wissen, sondern weil die Realität selbst noch nicht festgelegt ist.
Das Modell ist die Quantenmechanik: ein Teilchen hat vor der Messung keine Spin-Richtung,
nicht weil wir sie nicht kennen, sondern weil sie noch nicht existiert.

Jedes Modell das man baut, muss diesen Unterschied respektieren.
Ein Arzt der die Diagnose noch nicht kennt → **nicht** U (das ist Unwissenheit).
Eine rechtliche Situation vor dem Urteil → **U** (das Recht ist noch nicht gesetzt).
Eine Messung vor dem Kollaps → **U** (der Zustand existiert nicht klassisch).

---

## Die fünf Konzepte die man braucht

### 1. Welten und Frames

Ein `World` ist ein möglicher Zustand. Ein `Frame` verbindet Welten durch zwei Relationen:

- **R_obs** (`rObs`): "von hier aus kann man dorthin beobachten"
- **R_τ** (`rTau`): "von hier aus kann ein σ-Kollaps dorthin führen"

```csharp
var wPre  = new World("pre_measurement");   // vor der Messung
var wUp   = new World("spin_up");
var wDown = new World("spin_down");

var frame = new Frame(
    worlds: [wPre, wUp, wDown],
    rObs: [(wPre, wUp), (wPre, wDown)],   // Messapparat sieht beide möglichen Zustände
    rTau: [(wPre, wUp), (wPre, wDown)]);  // Kollaps führt zu einem von beiden
```

**Wichtig:** Man muss nicht alle Welten explizit auflisten. Der Frame-Konstruktor
fügt automatisch alle Welten hinzu, die in den Relationen vorkommen. Folgendes ist äquivalent:

```csharp
var frame = new Frame(
    worlds: [],                                       // leer ist ok
    rObs: [(wPre, wUp), (wPre, wDown)],
    rTau: [(wPre, wUp), (wPre, wDown)]);
// wPre, wUp, wDown werden automatisch aufgenommen
```

### 2. Valuation und World Types

Eine `Valuation` weist jedem Atom an jeder Welt einen Wahrheitswert zu.
**Aktionsvariablen** (action variables) bestimmen den Welttyp:

- Welt ist **W_super** wenn eine Aktionsvariable dort U ist
- Welt ist **W_eigen** wenn alle Aktionsvariablen klassisch (T oder F) sind

```csharp
var val = new Valuation(
    assignments: [
        (("spin", wPre),  TruthValue.Undetermined),  // ← macht wPre zu W_super
        (("spin", wUp),   TruthValue.True),           // ← wUp ist W_eigen
        (("spin", wDown), TruthValue.False),          // ← wDown ist W_eigen
    ],
    actionVariables: ["spin"]);  // "spin" ist Aktionsvariable

var model = new Model(frame, val);
model.GetWorldType(wPre);   // → Super
model.GetWorldType(wUp);    // → Eigen
```

Das ist **Option B**: der Welttyp folgt aus der Valuation, er wird nicht separat deklariert.
SC_PC (jede W_super-Welt hat einen W_eigen-Kollaps-Nachfolger) ist dann ein Theorem, kein Axiom.

### 3. Formeln auswerten

```csharp
using static ECL3Q.Core.Syntax.F;  // für P, Q, R, And(...), Obs(...), etc.

var spin = new Atom("spin");

// Einfach
model.Evaluate(spin, wPre);                    // → Undetermined

// Mit Operatoren
model.Evaluate(And(P, Q), w);                  // → min(V(p,w), V(q,w))
model.Evaluate(Or(P, Not(Q)), w);              // → max(V(p,w), 2-V(q,w))

// Obs: ist das Atom an dieser Welt beobachtbar?
model.Evaluate(Obs(spin), wPre);               // → Undetermined (U ist nie beobachtbar)
model.Evaluate(Obs(spin), wUp);                // → True (spin=T an wUp, keine Nachfolger → direkt beobachtbar)

// Modal: über R_obs
model.Evaluate(Box(spin), wPre);               // → min(T, F) = False (nicht notwendig)
model.Evaluate(Diamond(spin), wPre);           // → max(T, F) = True (möglich)

// Kollaps: über R_τ
model.Evaluate(Collapse(spin), wPre);          // → min(T, F) = False

// SC_PC prüfen
model.VerifySC_PC();                           // → true wenn alle W_super einen W_eigen-Pfad haben
```

### 4. Gültigkeit prüfen

Es gibt drei verschiedene Validity-Begriffe — sie meinen verschiedene Dinge:

```csharp
// K₃-Gültigkeit: φ ist immer T (auch wenn Atome U sein können)
// Vollständig via Wahrheitstabelle. Prüft auf echte drei-wertige Gültigkeit.
ProofSearch.IsK3Tautology(Implies(P, P));         // → false (p=U gibt U→U=U)
ProofSearch.IsK3Tautology(Iff(Not(Not(P)), P));   // → true (¬¬p↔p immer T)

// Klassische Gültigkeit: φ kann niemals F sein
// K₃-Gültig ⟹ klassisch gültig (aber nicht umgekehrt)
ProofSearch.IsClassicalTautology(Or(P, Not(P)));  // → true (LEM)
ProofSearch.IsK3Tautology(Or(P, Not(P)));         // → false (p=U gibt U)

// Modal-Gültigkeit (klassisch, über Kripke-Modelle):
// Prüft ob φ nie in einem Kripke-Modell falsch sein kann.
// ACHTUNG: Das ist klassische Gültigkeit, keine K₃-Gültigkeit.
ModalTableau.IsModallyValid(Box(Implies(P,Q)));   // → false (□(p→q) ist nicht notwendig)
```

**Faustregel:** Für propositionale Fragen → `ProofSearch.IsK3Tautology`.
Für modale/Obs/σ-Fragen → `ModalTableau.IsModallyValid` (klassische modale Gültigkeit).

### 5. Formeln parsen (statt zu bauen)

Für längere Formeln ist der Parser bequemer als die Builder-API:

```csharp
// Unicode
var f1 = FormulaParser.Parse("□(p → q) → (□p → □q)");
var f2 = FormulaParser.Parse("Obs(p) → p");
var f3 = FormulaParser.Parse("σ(spin)");

// ASCII-Fallbacks (für Terminals ohne Unicode)
var f4 = FormulaParser.Parse("[](p -> q) -> ([]p -> []q)");
var f5 = FormulaParser.Parse("(p <-> q)");

// Fehlerbehandlung
var f6 = FormulaParser.TryParse("@ungültig", out var error);
// f6 == null, error = "Unexpected character '@' at position 0"
```

Atom-Namen dürfen `a-z`, `0-9`, `_`, `τ` enthalten.
**Nicht:** `sigma` als Atom-Name — der Parser interpretiert `sigma(` als σ-Operator.
`sigmap` ist ok (kein Klammer-Zeichen folgt).

---

## Die Operatoren — was sie bedeuten

### Propositionale Operatoren (Strong Kleene)

Die Grundoperatoren folgen Strong Kleene-Semantik: Konjunktion ist das Minimum, Disjunktion das Maximum in der Ordnung F < U < T.

| Formel | Sprich | Bedeutung | Besonderheit gegenüber klassisch |
|--------|--------|-----------|----------------------------------|
| `¬A` | nicht A | Negation | ¬U = U (Unbestimmtes bleibt unbestimmt) |
| `A ∧ B` | A und B | min(A,B) | U ∧ T = U (Unbestimmtes "infiziert") |
| `A ∨ B` | A oder B | max(A,B) | U ∨ T = T (Wahres dominiert) |
| `A → B` | A impliziert B | max(¬A, B) | U → U = U (nicht T wie klassisch!) |
| `A ↔ B` | A genau dann wenn B | min(A→B, B→A) | T ↔ U = U |

Das wichtigste Nicht-Ergebnis: **p → p = U wenn p = U**. Implikation ist kein K₃-Tautologie. Das ist kein Bug — es ist der Kern der Drei-Wertigkeit. Klassische Logik ist in ECL₃^Q als Fragment enthalten (wenn U ausgeschlossen wird), aber nicht umgekehrt.

### Obs(A) — Beobachtbarkeits-Operator

`Obs(A)` fragt: *Kann der Wahrheitswert von A an dieser Welt beobachtet werden?*

```
Obs(A) = T   wenn A einen klassischen Wert hat und alle R_obs-Nachfolger einig sind
Obs(A) = F   wenn die Nachfolger uneinig sind über den Wert von A
Obs(A) = U   wenn A selbst U ist — Unbestimmtes ist grundsätzlich nicht beobachtbar
```

Wichtige Eigenschaften:
- `Obs(A) → A` ist gültig: was beobachtbar ist, ist wahr
- `Obs(A) = T` bedeutet nicht nur "A ist wahr", sondern "A ist einheitlich und zugänglich"
- `obs_w(F-Formel) ∈ {F, U}` — ein falsches Atom kann als falsch beobachtbar sein (= F), oder die Falschheit ist nicht zugänglich (= U), aber nie T
- OC (`obs(A∧B) = min(obs(A), obs(B))`) gilt **nicht** für nicht-Standard Obs-Semantik — Beobachtbarkeit einer Konjunktion ist nicht auf die Einzelbeobachtbarkeiten reduzierbar (Theorem A4)

**Wann Obs(A) = F?** Wenn die R_obs-Nachfolger verschiedene Werte für A liefern. Beispiel: von Welt w aus ist v₁ (A=T) und v₂ (A=F) zugänglich — A ist von w aus nicht einheitlich beobachtbar → Obs(A) = U.

### □A und ◇A — Modale Operatoren

`□A` und `◇A` quantifizieren über die **Beobachtungsrelation R_obs**:

```
□A at w = min { V(A, v) | v ∈ R_obs(w) }   "A gilt an allen beobachtbaren Welten"
◇A at w = max { V(A, v) | v ∈ R_obs(w) }   "A gilt an mindestens einer beobachtbaren Welt"
```

Drei-wertig: wenn ein Nachfolger U hat, kann □A = U ergeben (nicht F, nicht T).
Ohne R_obs-Nachfolger: □A = T (vacuously), ◇A = F (vacuously).

Die K-Axiom-Analogie gilt: `□(A→B) → (□A → □B)` ist gültig.
Aber T-Axiom (`□A → A`) gilt nur in reflexiven Frames (RF1).

**R_obs vs R_τ:** Das ist der zentrale Unterschied zu σ und [do τ]:
- R_obs = Beobachtungsrelation: was kann von hier aus gesehen werden?
- R_τ = Kollapsrelation: wohin kann ein σ-Ereignis führen?

Beide Relationen sind unabhängig. Ein Modell kann haben: w beobachtet v (R_obs), aber kollabiert zu u (R_τ). Das sind verschiedene Fragen.

### σ(A) — Kollaps-Operator

`σ(A)` fragt: *Was ist der Wert von A nach einem σ-Kollaps?*

```
σ(A) at w = min { V(A, v) | v ∈ R_τ(w) }   (analog zu □ über R_τ)
```

Der σ-Kollaps ist der Übergang von W_super (ontologisch unbestimmt) zu W_eigen (klassisch bestimmt). Er ist:
- **irreversibel**: W_eigen → W_super ist verboten (σ-Inversion)
- **nicht-deterministisch**: mehrere R_τ-Ziele sind möglich
- **AO1_eigen**: nach dem Kollaps sind alle Aktionsvariablen klassisch (T oder F)

Wenn w keine R_τ-Nachfolger hat: `σ(A) = U` — das signalisiert einen SC_PC-Fehler.

### [do τ]A — Dynamischer Operator

`[do τ]A` ist semantisch identisch mit `σ(A)` — beide nutzen R_τ und haben dieselbe min-Semantik. Der Unterschied ist konzeptuell:

- `σ(A)` betont den physikalischen Kollaps-Charakter (Quantenmechanik, Messvorgang)
- `[do τ]A` betont die Handlungs-Perspektive (τ ist eine Aktion die jemand ausführt)

Für das τ-Framework (Paper IV): `[do τ]` modelliert agentische Interventionen die einen σ-Kollaps auslösen.

### Obs vs □ vs σ — der entscheidende Unterschied

Das wird am häufigsten verwechselt:

```
□A at w:    A gilt überall wo ich hinschauen kann (R_obs-Nachfolger)
Obs(A) at w: A ist beobachtbar — einheitlich und zugänglich
σ(A) at w:  A gilt nach dem Kollaps (R_τ-Nachfolger)
```

Beispiel: Rechtslage vor Urteil
- `□schuldig` an w_dispute: in allen beobachtbaren Welten ist der Angeklagte schuldig → F (weil v_freigesprochen existiert)
- `Obs(schuldig)` an w_dispute: ist Schuld beobachtbar? → U (Nachfolger uneinig)
- `σ(schuldig)` an w_dispute: nach dem Urteil gilt Schuld → F (min über mögliche Urteile)

---



### Workflow 1: Ein Szenario modellieren und analysieren

```csharp
// Schritt 1: Welten benennen
var wOffen   = new World("fall_offen");    // Rechtslage ungeklärt
var wVerurteil = new World("verurteilt");
var wFreigespr = new World("freigesprochen");

// Schritt 2: Frame mit Relationen
var frame = new Frame(
    worlds: [],  // werden aus Relationen abgeleitet
    rObs: [(wOffen, wVerurteil), (wOffen, wFreigespr)],
    rTau: [(wOffen, wVerurteil), (wOffen, wFreigespr)]);

// Schritt 3: Valuation — Aktionsvariable = was kollabiert
var val = new Valuation([
    (("schuldig", wOffen),     TruthValue.Undetermined),
    (("schuldig", wVerurteil), TruthValue.True),
    (("schuldig", wFreigespr), TruthValue.False),
], actionVariables: ["schuldig"]);

var model = new Model(frame, val);

// Schritt 4: Abfragen
var schuldig = new Atom("schuldig");
Console.WriteLine(model.GetWorldType(wOffen));          // Super
Console.WriteLine(model.Evaluate(schuldig, wOffen));    // Undetermined
Console.WriteLine(model.Evaluate(Obs(schuldig), wOffen)); // Undetermined
Console.WriteLine(model.VerifySC_PC());                 // true
```

### Workflow 2: σ-Konflikte erkennen

```csharp
// Agenten definieren
var gericht = new Agent("Bundesgericht", isSigmaCarrier: true);

var maFrame = new MultiAgentFrame(frame, [gericht]);
var detektor = new SigmaConflictDetector(maFrame, model);

foreach (var konflikt in detektor.DetectAll())
    Console.WriteLine(konflikt);
// → Keine Konflikte (well-formed)

// Vakuum-Szenario: kein σ-Träger
var maFrameOhne = new MultiAgentFrame(frame, []);
var detektorOhne = new SigmaConflictDetector(maFrameOhne, model);
detektorOhne.DetectAll();
// → [Vakuum] No σ-carrier exists for superposition world fall_offen
```

**Die 8 σ-Konflikt-Kategorien:**

| Kategorie | Wann | Typisches Beispiel |
|-----------|------|-------------------|
| Vakuum | Kein σ-Träger vorhanden | Rechtslücke, keine zuständige Behörde |
| Inversion | Kollaps in falsche Richtung (W_eigen→W_super) | Rechtskräftiges Urteil wird wieder aufgerollt |
| Unterwanderung | Träger vorhanden, handelt aber nicht | Zuständiges Gremium tagt nie |
| VorabKollaps | Kollaps vor berechtigter Beobachtung | Urteil vor Beweisaufnahme |
| Delegation | Träger gibt Autorität an Nicht-Autorisierten weiter | Gericht delegiert an privaten Schlichter |
| HierarchieKonflikt | Zwei Träger mit widersprüchlichen R_obs | Zwei Gerichte beanspruchen Zuständigkeit |
| Kaskade | Kollaps-Fehler propagiert | Ungültiger Beschluss löst weitere aus |
| Diskontinuität | Kollaps-Kette bricht ab | Verfahren wird nie abgeschlossen |

### Workflow 3: Formale Gültigkeit prüfen

```csharp
// Ist "Obs(p)→p" gültig? (Beobachtbarkeit impliziert Wahrheit)
ModalTableau.IsModallyValid(FormulaParser.Parse("Obs(p) -> p"));  // true

// Ist "□(p→q) → (□p→□q)" gültig? (K-Axiom)
ModalTableau.IsModallyValid(FormulaParser.Parse("[](p->q) -> ([]p->[]q)"));  // true

// Ist "σ(p→q) → (σ(p)→σ(q))" gültig? (K-Analogon für σ)
ModalTableau.IsModallyValid(FormulaParser.Parse("sigma(p->q) -> (sigma(p)->sigma(q))"));  // true

// Gilt LEM in K₃?
ProofSearch.IsK3Tautology(FormulaParser.Parse("p \\/ ~p"));  // false — das ist der Kern
```

### Workflow 4: Interaktiv erkunden

```bash
dotnet run --project ECL3Q.CLI
```

```
ECL3Q> valid (p -> p)        # klassisch gültig, nicht K₃-gültig
ECL3Q> classical (p -> p)    # klassisch gültig
ECL3Q> parse Obs((p /\ q))   # Formel-Struktur anzeigen
ECL3Q> truth-tables          # alle Wahrheitstabellen
ECL3Q> oc-test               # OC-Falsifikation demonstrieren
```

---

## Häufige Fehler und wie man sie vermeidet

### "Mein Modell hat keinen U-Wert obwohl es sollte"

Wahrscheinliche Ursache: die Aktionsvariable ist nicht in `actionVariables` registriert.

```csharp
// Falsch:
var val = new Valuation([
    (("schuldig", wOffen), TruthValue.Undetermined),
]);
// actionVariables fehlt → Closed-World-Default: F überall → alle Welten W_eigen

// Richtig:
var val = new Valuation([
    (("schuldig", wOffen), TruthValue.Undetermined),
], actionVariables: ["schuldig"]);
```

### "SC_PC schlägt fehl obwohl mein Modell korrekt aussieht"

SC_PC verlangt: jede W_super-Welt hat eine R_τ-Kante zu einer W_eigen-Welt.
Typischer Fehler: R_τ und R_obs zeigen auf verschiedene Welten.

```csharp
// Bug: R_τ führt zu einer Welt ohne Aktionsvariable-Eintrag → Closed-World → F → W_eigen ✓
// Aber: wenn diese Welt in val gar nicht vorkommt, ist sie nicht explizit W_eigen.
// Prüfe: model.GetWorldType(zielWelt) == WorldType.Eigen
```

### "IsModallyValid gibt true für etwas das nicht gültig sein sollte"

`IsModallyValid` prüft **klassische** modale Gültigkeit, nicht K₃-Gültigkeit.
LEM (`p∨¬p`) ist klassisch gültig → `IsModallyValid` gibt true.
Für K₃-Fragen: `ProofSearch.IsK3Tautology`.

### "Der Parser wirft eine Ausnahme für meinen Atom-Namen"

Atom-Namen müssen mit Kleinbuchstaben beginnen. Großbuchstaben, Sonderzeichen,
und `sigma` direkt vor `(` sind nicht erlaubt.

```
Erlaubt:  p, q, spin, tau1, ist_schuldig, τ
Verboten: Schuldig, ist-schuldig, sigma(  ← "sigma(" wird als σ-Operator interpretiert
```

---

## Die 119 Domänen

ECL₃^Q ist auf 119 Domänen anwendbar (SR401-rev3). Zugriff:

```csharp
using ECL3Q.Core.Semantics;

// Alle Domänen
foreach (var d in DomainClassification.All)
    Console.WriteLine($"{d.Id}: {d.Name} — σ-Träger: {d.SigmaCarrier}");

// Nach Gruppe filtern
var rechtsDomänen = DomainClassification.InGroup(
    DomainClassification.DomainGroup.LawAndGovernance);

// Einzelne Domäne
var qm = DomainClassification.ById(76);
// → Domain(76, "Quantum Measurement", NaturalAndSocialSciences, ...)
```

---

## Was ist gesichert, was ist offen

**Gesichert (rigoros bewiesen):**
- K₃-Tautologien werden korrekt erkannt (Wahrheitstabelle, vollständig)
- Klassische modale Gültigkeit (ModalTableau, vollständig für K + alle Konnektive inkl. Obs, σ, [do τ])
- OC ist falsifiziert (für nicht-Standard Obs-Semantik, Theorem A4)
- OC-weak gilt (für Standard EvaluateObs)
- RF1, RF3 sind frame-definierbar; RF2 nicht
- σ-Taxonomie: 8 Kategorien disjunkt und vollständig (Ax1–Ax7)
- SC_PC ist ein Theorem unter Option B
- U:Obs vollständig: Zwei-Branch-Regel deckt Source 1 (φ=U) und Source 2 (uneinige Nachfolger) ab

**Offen (ehrlich dokumentiert):**
- **F4**: Lineare Darstellung von Obs in größerem algebraischen Raum
- **Vollständigkeitssatz**: für vollen ECL₃^Q (SC_PC + AO1_eigen Frame-Klasse) noch nicht bewiesen. Ziel: Paper I §4.3.

---

## Für Reviewer und Logiker

Das Framework ist als ausführbare Verifikation der formalen Resultate gedacht.
Jedes gesicherte Resultat (SR-Serie) hat einen entsprechenden Unit-Test.

```bash
dotnet test --verbosity normal
# Zeigt alle 110 Tests mit Namen
```

Kritische Tests für Paper I:
- `AlgebraTests.OC_Countermodel_ShowsOCDoesNotHold` — OC-Falsifikation
- `SemanticTests.RF2_*` — RF2-Nicht-Frame-Definierbarkeit
- `SemanticTests.SCPC_*` — SC_PC als Theorem
- `FrameAndTaxonomyTests.SigmaTaxonomy_Ax4_*` — Disjunktheit der Taxonomie
- `InferenceTests.Tableau_IsValid_*` — Korrektheitsregression

---

*Markus Bauernfeind | Juni 2026*
*Implementierung: Claude (Anthropic) als KI-Assistent. Alle Konzepte, Beweise und formalen Inhalte: Markus Bauernfeind.*
