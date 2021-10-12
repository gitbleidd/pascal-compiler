using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler
{
    public enum TokenType
    {
        // OperationToken
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
        LeftCurlyBracketToken,
        RightCurlyBracketToken,

        // ConstToken
        IntConstToken,
        FloatConstToken,
        StringConstToken,

        // VarToken
        IdentifierToken,
        BadToken,
        CommaToken,
        DotToken,
        CaretToken,

        EndOfFileToken,
        SpaceToken,

        // ReversedWords
        AbsoluteToken = 100,
        AndToken,
        ArrayToken,
        AsmToken,
        BeginToken,
        CaseToken,
        ConstToken,
        ConstructorToken,
        DestructorToken,
        DivToken,
        DoToken,
        DowntoToken,
        ElseToken,
        EndToken,
        FileToken,
        ForToken,
        FunctionToken,
        GotoToken,
        IfToken,
        ImplementationToken,
        InToken,
        InheritedToken,
        InlineToken,
        InterfaceToken,
        LabelToken,
        ModToken,
        NilToken,
        NotToken,
        ObjectToken,
        OfToken,
        OperatorToken,
        OrToken,
        PackedToken,
        ProcedureToken,
        ProgramToken,
        RecordToken,
        ReintroduceToken,
        RepeatToken,
        SelfToken,
        SetToken,
        ShlToken,
        ShrToken,
        StringToken,
        ThenToken,
        ToToken,
        TypeToken,
        UnitToken,
        UntilToken,
        UsesToken,
        VarToken,
        WhileToken,
        WithToken,
        XorToken
    }

}
