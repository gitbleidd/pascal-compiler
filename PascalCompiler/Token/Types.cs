using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Token
{
    public enum SpecialSymbolType
    {
        // Special-symbol tokens
        LeftRoundBracketToken,
        RightRoundBracketToken,
        PlusToken,
        MinusToken,
        MultToken,
        DivisionToken,
        GreaterToken,
        LessToken,
        GreaterOrEqualToken,
        LessOrEqualToken,
        EqualToken,
        NotEqualToken,
        SemicolonToken,
        ColonToken,
        AssignmentToken,
        LeftSquareBracketToken,
        RightSquareBracketToken,
        CommaToken,
        DotToken,
        DoubleDotToken,

        // Word-symbol
        NilToken = 100,
        NotToken,
        AndToken,
        DivToken,
        PackedToken,
        ArrayToken,
        OfToken,
        FileToken,
        SetToken,
        RecordToken,
        EndToken,
        CaseToken,
        OrToken,
        FunctionToken,
        VarToken,
        ProcedureToken,
        BeginToken,
        IfToken,
        ThenToken,
        ElseToken,
        WhileToken,
        DoToken,
        RepeatToken,
        UntilToken,
        ForToken,
        ToToken,
        DowntoToken,
        WithToken,
        GotoToken,
        LabelToken,
        ConstToken,
        TypeToken,
        ProgramToken,
        ModToken,
        InToken = 134,
    }

    public enum TriviaTokenType
    {
        EndOfFileToken,
        BadToken,
        UnknownSymbol,
    }
}
