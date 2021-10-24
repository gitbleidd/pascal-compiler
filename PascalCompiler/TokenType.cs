using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler
{
    public enum TokenType
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

        // Const tokens
        IntConstToken,
        FloatConstToken,
        StringConstToken,

        IdentifierToken, // Identifier token

        // Other tokens
        EndOfFileToken,
        SpaceToken,
        BadToken,

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
        InToken = 134
    }

}
