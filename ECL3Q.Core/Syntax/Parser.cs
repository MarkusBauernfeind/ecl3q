namespace ECL3Q.Core.Syntax;

/// <summary>
/// Parser for ECL₃^Q formula strings.
///
/// Thread safety: <see cref="Parse"/> and <see cref="TryParse"/> are thread-safe —
/// each call creates a new parser instance. Do not share instances across threads.
///
/// Grammar:
///   formula  ::= atom | '¬'formula | '('formula op formula')' | modal | obs | collapse | do
///   op       ::= '∧' | '∨' | '→' | '↔'
///   modal    ::= '□'formula | '◇'formula
///   obs      ::= 'Obs''('formula')'
///   collapse ::= 'σ''('formula')'
///   do       ::= '['do' action']'formula
///   atom     ::= [a-z][a-z0-9_τ]*
///
/// ASCII fallbacks (for terminals without Unicode):
///   ¬  → !  or  ~
///   ∧  → &amp;  or  /\
///   ∨  → |  or  \/
///   →  → ->
///   ↔  → &lt;->
///   □  → []
///   ◇  → &lt;&gt;
///   σ  → sigma
/// </summary>
public class FormulaParser
{
    private string _input = "";
    private int _pos;

    public static Formula Parse(string input)
    {
        var parser = new FormulaParser();
        return parser.ParseInternal(Normalize(input));
    }

    /// <summary>
    /// Try parse; returns null on failure instead of throwing.
    /// </summary>
    public static Formula? TryParse(string input, out string? error)
    {
        try
        {
            error = null;
            return Parse(input);
        }
        catch (FormulaParseException ex)
        {
            error = ex.Message;
            return null;
        }
    }

    private Formula ParseInternal(string input)
    {
        _input = input;
        _pos = 0;
        var result = ParseFormula();
        SkipWhitespace();
        if (_pos < _input.Length)
            throw new FormulaParseException(
                $"Unexpected character '{_input[_pos]}' at position {_pos}");
        return result;
    }

    private Formula ParseFormula()
    {
        SkipWhitespace();

        // Negation
        if (Peek() is '¬' or '!' or '~')
        {
            Consume();
            return new Negation(ParseFormula());
        }

        // Parenthesized binary formula
        if (Peek() == '(')
        {
            Consume(); // '('
            var left = ParseFormula();
            SkipWhitespace();
            var op = ParseBinaryOp();
            var right = ParseFormula();
            SkipWhitespace();
            Expect(')');
            return op switch
            {
                "∧" => new Conjunction(left, right),
                "∨" => new Disjunction(left, right),
                "→" => new Implication(left, right),
                "↔" => new Biconditional(left, right),
                _ => throw new FormulaParseException($"Unknown operator: {op}")
            };
        }

        // Modal necessity □
        if (Peek() == '□' || PeekStr("[]"))
        {
            if (Peek() == '□') Consume();
            else { Consume(); Consume(); }
            return new ModalBox(ParseFormula());
        }

        // Modal possibility ◇
        if (Peek() == '◇' || PeekStr("<>"))
        {
            if (Peek() == '◇') Consume();
            else { Consume(); Consume(); }
            return new ModalDiamond(ParseFormula());
        }

        // Obs(φ)
        if (PeekStr("Obs("))
        {
            ConsumeStr("Obs(");
            var sub = ParseFormula();
            Expect(')');
            return new ObsFormula(sub);
        }

        // σ(φ) or sigma(φ)
        if (PeekStr("σ(") || PeekStr("sigma("))
        {
            if (PeekStr("σ(")) ConsumeStr("σ(");
            else ConsumeStr("sigma(");
            var sub = ParseFormula();
            Expect(')');
            return new CollapseFormula(sub);
        }

        // [do action]φ
        if (PeekStr("[do "))
        {
            ConsumeStr("[do ");
            var action = ParseIdentifier();
            Expect(']');
            var sub = ParseFormula();
            return new DoOperator(action, sub);
        }

        // Atom
        if (char.IsLower(Peek()) || Peek() == 'τ')
        {
            var name = ParseIdentifier();
            return new Atom(name);
        }

        throw new FormulaParseException(
            $"Unexpected character '{Peek()}' at position {_pos} in: {_input}");
    }

    private string ParseBinaryOp()
    {
        SkipWhitespace();
        if (Peek() == '∧') { Consume(); return "∧"; }
        if (Peek() == '∨') { Consume(); return "∨"; }
        if (Peek() == '→') { Consume(); return "→"; }
        if (Peek() == '↔') { Consume(); return "↔"; }
        if (PeekStr("/\\")) { ConsumeStr("/\\"); return "∧"; }
        if (PeekStr("\\/")) { ConsumeStr("\\/"); return "∨"; }
        if (PeekStr("<->")) { ConsumeStr("<->"); return "↔"; }
        if (PeekStr("->")) { ConsumeStr("->"); return "→"; }
        if (PeekStr("&")) { Consume(); return "∧"; }
        if (PeekStr("|")) { Consume(); return "∨"; }
        throw new FormulaParseException($"Expected binary operator at position {_pos}");
    }

    private string ParseIdentifier()
    {
        var start = _pos;
        while (_pos < _input.Length &&
               (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_'))
            _pos++;
        if (_pos == start)
            throw new FormulaParseException($"Expected identifier at position {_pos}");
        return _input[start.._pos];
    }

    private char Peek() => _pos < _input.Length ? _input[_pos] : '\0';

    private bool PeekStr(string s) =>
        _pos + s.Length <= _input.Length &&
        _input[_pos..(_pos + s.Length)] == s;

    private void Consume() => _pos++;

    private void ConsumeStr(string s)
    {
        if (!PeekStr(s))
            throw new FormulaParseException($"Expected '{s}' at position {_pos}");
        _pos += s.Length;
    }

    private void Expect(char c)
    {
        SkipWhitespace();
        if (Peek() != c)
            throw new FormulaParseException(
                $"Expected '{c}' at position {_pos}, found '{Peek()}'");
        Consume();
    }

    private void SkipWhitespace()
    {
        while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
            _pos++;
    }

    /// <summary>Normalize ASCII fallbacks to Unicode operators.</summary>
    private static string Normalize(string s) => s
        .Replace("<->", "↔")
        .Replace("->",  "→")
        .Replace("/\\", "∧")
        .Replace("\\/", "∨")
        .Replace("[]",  "□")
        .Replace("<>",  "◇")
        .Replace("!",   "¬")
        .Replace("~",   "¬")
        .Replace("sigma(", "σ(");  // only normalize "sigma(" to avoid corrupting atom names like "sigmap"
}

public class FormulaParseException(string message) : Exception(message);
