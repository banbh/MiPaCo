﻿using System;
using System.Linq;
using static MiPaCo.Combinators;

namespace MiPaCo
{
    public abstract record Expr { }
    public record Lam(string Name, Expr Body) : Expr { }
    public record App(Expr Fun, Expr Arg) : Expr { }
    public record Var(string Name) : Expr { }

    /// <summary>Contains a grammar for the λ-calculus essentially identical to that in 
    /// "Monadic Parser Combinators" by Hutton and Meijer, section 6.2.</summary>
    static class LambdaCalculusExample
    {
        static readonly Func<Expr, Expr, Expr> app = (fun, arg) => new App(fun, arg);
        static Parser<Expr> Expression => Atom.ChainL1(Return(app)); // make into property to deal with circular dependency
        static readonly Parser<Expr> Paren = from _ in Symb("(")
                                             from x in Expression
                                             from __ in Symb(")")
                                             select x;
        static readonly Parser<string> Name = Char(char.IsLetter).Many1().Select(string.Concat).Token();
        static readonly Parser<Expr> Var = from name in Name select new Var(name);
        static readonly Parser<Expr> Lam = from _ in Symb("\\")
                                           from name in Name
                                           from __ in Symb("->")
                                           from body in Expression
                                           select new Lam(name, body);
        static readonly Parser<Expr> Atom = Lam.Dor(Var).Dor(Paren);

        public static void Main()
        {
            Console.WriteLine();
            Console.WriteLine($"=== {typeof(LambdaCalculusExample)} ===");

            Name.ParseAndPrint(@"abc", nameof(Name));
            Var.ParseAndPrint(@"abc", nameof(Var));
            Lam.ParseAndPrint(@"\x -> x  ", nameof(Lam));
            Paren.ParseAndPrint(@"( x ) ", nameof(Paren));
            Expression.ParseAndPrint(@"x y z", nameof(Expression));
            Expression.ParseAndPrint(@"\x -> x", nameof(Expression));
            // Use example of ambiguity from Andrej Bauer
            var ambiguousStr = @"\x -> x \y -> y  ";
            var ambiguousExpr = Expression(ambiguousStr).Single().Value; // the grammar can generate this in two ways
            // our parser only parses it one way though
            var standardParseStr = @"\x -> (x \y -> y)";
            Console.WriteLine($"{ambiguousStr}={standardParseStr}? {ambiguousExpr == Expression(standardParseStr).Single().Value}");
            var alternativeParseStr = @"(\x -> x)(\y -> y)";
            Console.WriteLine($"{ambiguousStr}={alternativeParseStr}? {ambiguousExpr == Expression(alternativeParseStr).Single().Value}");
        }
    }


    /// <summary>A similar grammar as in <see cref="LambdaCalculusExample"/> except using lazy combinators so
    /// that all parses are returned which makes ambiguous grammars more obvious.</summary>
    static class LazyLambdaCalculusExample
    {
        /// <summary>Lazy or non-deterministic version <see cref="Combinators.ChainL1"/> which returns 
        /// all parses including partial ones.</summary>
        /// <remarks>This is a simplified version of the previous grammar that is just enough to
        /// illustrate some ambiguous parses.</remarks>
        static Parser<T> LazyChainL1<T>(this Parser<T> p, Parser<Func<T, T, T>> op)
        {
            Parser<T> rest(T t) => grab(t).Or(Return(t)); // Use Or insead of Dor to be lazy
            Parser<T> grab(T t1) => from f in op
                                    from t2 in p
                                    from x in rest(f(t1, t2))
                                    select x;
            return p.Bind(rest);
        }

        static readonly Func<Expr, Expr, Expr> app = (fun, arg) => new App(fun, arg);
        static Parser<Expr> Expression => Atom.LazyChainL1(Return(app)); // make into property to deal with circular dependency
        static readonly Parser<string> Name = Char(char.IsLetter).Many1().Select(string.Concat).Token();
        static readonly Parser<Expr> Var = from name in Name select new Var(name);
        static readonly Parser<Expr> Lam = from _ in Symb("\\")
                                           from name in Name
                                           from __ in Symb("->")
                                           from body in Expression
                                           select new Lam(name, body);
        static readonly Parser<Expr> Atom = Lam.Or(Var);

        public static void Main()
        {
            Console.WriteLine();
            Console.WriteLine($"=== {typeof(LazyLambdaCalculusExample)} ===");

            // In next example we see multiple parses simply because it stops early
            Expression.ParseAndPrint(@"x y z", nameof(Expression), "\n\t-> ");
            // We can force it to go to the end by appending the ".ToEnd()" combinator
            Expression.ToEnd().ParseAndPrint(@"x y z", nameof(Expression) + ".ToEnd", "\n\t-> ");
            // In next example we parse an ambiguous string (provided by Andrej Bauer)
            // which given our lazy grammar gives us two parses
            Expression.ToEnd().ParseAndPrint(@"\x -> x \y -> y  ", nameof(Expression), "\n\t-> ");
        }
    }
}
