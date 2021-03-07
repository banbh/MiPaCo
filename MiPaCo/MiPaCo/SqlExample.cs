using System;
using System.Collections.Immutable;
using static MiPaCo.Combinators;

namespace MiPaCo
{
    public class SqlExample
    {
        #region Simple AST for SQL
        public interface IExpr { }

        public record IdentifierExpr(string Name) : IExpr;

        public interface IPredicate { }

        public enum Operator
        {
            Gt, Gte, Lt, Lte, Eq, Neq
        }

        public record ComparisonPred(IExpr Lhs, Operator Op, IExpr Rhs) : IPredicate;

        public record NumericExpr(double Value) : IExpr;

        public record StringExpr(string Value) : IExpr;

        public record InPred(IExpr Lhs, ImmutableList<IExpr> Rhs) : IPredicate;

        public record AndPred(IPredicate Lhs, IPredicate Rhs) : IPredicate;

        public record OrPred(IPredicate Lhs, IPredicate Rhs) : IPredicate;
        #endregion

        #region Parsers for a simple SQL grammar
        // First some helpers to help us ignore case
        static bool EqualIgnoreCase(char lhs, char rhs) => char.ToUpperInvariant(lhs) == char.ToUpperInvariant(rhs);
        static Parser<string> Stri(string s0) => s0 == "" ? Return("") : Char(c => EqualIgnoreCase(c, s0[0])).Bind(c => Stri(s0.Remove(0, 1)).Select(s => c + s));
        public static Parser<string> Symbi(string cs) => Stri(cs).Token();

        // Now build the grammar
        static readonly Parser<char> QuoteP = Char('\'');

        static readonly Parser<IExpr> StringP = from _1 in QuoteP
                                                from s in Char(c => c != '\'').Or(from q in QuoteP from _ in QuoteP select q).Many()
                                                from _2 in QuoteP
                                                select new StringExpr(string.Concat(s));

        static readonly Parser<IExpr> IdentifierP = from c1 in Char(char.IsLetter)
                                                    from s2 in Char(char.IsLetterOrDigit).Many()
                                                    select new IdentifierExpr(c1 + string.Concat(s2));

        static readonly Parser<IExpr> NumberP = (from cc in Char(char.IsDigit).Many1()
                                                 select new NumericExpr(double.Parse(string.Concat(cc)))).Token();

        static readonly Parser<Operator> OperatorP = Symb("<=").Select(_ => Operator.Lte)
            .OrElse(Symb(">=").Select(_ => Operator.Gte))
            .OrElse(Symb("!=").Select(_ => Operator.Neq))
            .OrElse(Symb("<>").Select(_ => Operator.Neq))
            .OrElse(Symb(">").Select(_ => Operator.Gt))
            .OrElse(Symb("<").Select(_ => Operator.Lt))
            .OrElse(Symb("=").Select(_ => Operator.Eq));

        static readonly Parser<IExpr> ExprP = StringP.OrElse(IdentifierP).OrElse(NumberP).Token();

        static readonly Parser<IPredicate> ComparisonP = from lhs in ExprP
                                                         from op in OperatorP
                                                         from rhs in ExprP
                                                         select new ComparisonPred(lhs, op, rhs);

        static readonly Parser<ImmutableList<IExpr>> ListP = from _1 in Symb("(")
                                                             from x in ExprP
                                                             from xx in (from _ in Symb(",")
                                                                         from y in ExprP
                                                                         select y).Many()
                                                             from _2 in Symb(")")
                                                             select xx.Insert(0, x);

        static readonly Parser<IPredicate> InParser = from e in ExprP
                                                      from _ in Symbi("in")
                                                      from lst in ListP
                                                      select new InPred(e, lst);

        static readonly Func<IPredicate, IPredicate, IPredicate> And = (lhs, rhs) => new AndPred(lhs, rhs), Or = (lhs, rhs) => new OrPred(lhs, rhs);

        static readonly Parser<Func<IPredicate, IPredicate, IPredicate>> OrP = Symbi("or").Select(_ => Or), AndP = Symbi("and").Select(_ => And);

        public static Parser<IPredicate> DisjunctionsP() => ConjunctionsP.ChainL1(OrP);
        static readonly Parser<IPredicate> ParenP = from b1 in Symb("(")
                                                    from pred in DisjunctionsP()
                                                    from b2 in Symb(")")
                                                    select pred;
        static readonly Parser<IPredicate> PredicateP = ComparisonP.OrElse(InParser).OrElse(ParenP);
        static readonly Parser<IPredicate> ConjunctionsP = PredicateP.ChainL1(AndP);
        #endregion

        public static void Main()
        {
            Console.WriteLine();
            Console.WriteLine($"=== {typeof(SqlExample)} ===");

            StringP.ParseAndPrint("'abc'xyz", $"{nameof(StringP)}");
            StringP.ParseAndPrint("'abc''xyz'", $"{nameof(StringP)}");
            var ch = Char(c => c != '\'').Or(from q in QuoteP from q2 in QuoteP select q);
            ch.ParseAndPrint("abc");
            ch.ParseAndPrint("''bc");
            ch.ParseAndPrint("'bc");

            ComparisonP.ParseAndPrint("a1 <> 'xx''yy'   ", $"{nameof(ComparisonP)}");
            InParser.ParseAndPrint("abc in (  1 , 2 , 3 )  ", $"{nameof(InParser)}");

            DisjunctionsP().ParseAndPrint("a=1 and b=2", "DisjunctionP()");
            DisjunctionsP().ParseAndPrint("a = 1 OR b=2 AND c = 3 OR d IN ( 'x' , 'y' , 'z' ) ", "DisjunctionP()");
        }
    }
}
