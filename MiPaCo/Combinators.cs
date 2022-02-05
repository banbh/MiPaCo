using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MiPaCo
{
    public interface IResult<out T> // Needed because tuples (or classes or structs can't be covariant)
    {
        T Value { get; }
        string Rest { get; }
    }

    public class Result<T> : IResult<T>
    {
        private readonly T value;
        private readonly string rest;
        public Result(T value, string rest)
        {
            this.value = value;
            this.rest = rest;
        }

        public T Value => value;

        public string Rest => rest;

        public override bool Equals(object obj) =>
            obj is Result<T> result &&
            EqualityComparer<T>.Default.Equals(value, result.value) &&
            rest == result.rest;

        public override int GetHashCode() => HashCode.Combine(value, rest);

        public override string ToString() => value + (rest == "" ? "" : $" (rest is \"{rest}\")");
    }

    public delegate IEnumerable<IResult<T>> Parser<out T>(string s);

    public static class Combinators
    {
        #region Helper extension methods
        /// <summary>Returns the first sequence unless it is empty, in which case it returns the second.</summary>
        public static IEnumerable<T> DefaultIfEmpty<T>(this IEnumerable<T> tt, IEnumerable<T> tt2)
        {
            bool empty = true;
            foreach (T t in tt)
            {
                yield return t;
                empty = false;
            }
            if (empty)
            {
                foreach (T t in tt2)
                {
                    yield return t;
                }
            }
        }

        public static Result<T> Result<T>(this T t, string s) => new(t, s);
        #endregion

        #region Core monadic extension methods
        public static Parser<T> Return<T>(T t) => s => Enumerable.Repeat(t.Result(s), 1);

        public static Parser<T> Fail<T>() => _ => Enumerable.Empty<Result<T>>();

        public static Parser<T2> Bind<T1, T2>(this Parser<T1> p1, Func<T1, Parser<T2>> fp2) => s => p1(s).SelectMany(t1s => fp2(t1s.Value)(t1s.Rest));
        #endregion

        #region Helper parser methods (built from core monadic methods)

        public static Parser<T2> SelectMany<T1, T2>(this Parser<T1> p1, Func<T1, Parser<T2>> fp2) => p1.Bind(fp2);

        public static Parser<T> SelectMany<T1, T2, T>(this Parser<T1> p1, Func<T1, Parser<T2>> fp2, Func<T1, T2, T> f) => p1.Bind(x => fp2(x).Bind(y => Return(f(x, y))));

        public static Parser<T2> Select<T1, T2>(this Parser<T1> p, Func<T1, T2> f) => p.Bind(t1 => Return(f(t1)));

        public static Parser<T> Where<T>(this Parser<T> p, Predicate<T> predicate) => p.Bind(t => predicate(t) ? Return(t) : Fail<T>());

        public static Parser<T> Or<T>(this Parser<T> p1, Parser<T> p2) => s => p1(s).Concat(p2(s));

        public static Parser<T> OrElse<T>(this Parser<T> p1, Parser<T> p2) => s => p1(s).DefaultIfEmpty(p2(s));

        public static Parser<ImmutableList<T>> Many1<T>(this Parser<T> p) => p.Bind(t => p.Many().Select(tt => tt.Insert(0, t)));

        public static Parser<ImmutableList<T>> Many<T>(this Parser<T> p) => p.Many1().OrElse(Return(ImmutableList<T>.Empty)); // greedy

        public static Parser<ImmutableList<T>> N<T>(this Parser<T> p, int n) => n <= 0 ? Return(ImmutableList<T>.Empty) : p.Bind(t => p.N(n - 1).Select(tt => tt.Insert(0, t)));

        public static Parser<T> ChainL1<T>(this Parser<T> p, Parser<Func<T, T, T>> op)
        {
            Parser<T> rest(T t) => grab(t).OrElse(Return(t)); // greedy
            Parser<T> grab(T t1) => from f in op
                                    from t2 in p
                                    from x in rest(f(t1, t2))
                                    select x;
            return p.Bind(rest);
        }

        public static readonly Parser<char> AnyChar = s => s == "" ? Fail<char>()(s) : Return(s[0])(s.Remove(0, 1));

        public static Parser<char> Char(Predicate<char> predicate) => AnyChar.Where(predicate);

        public static Parser<char> Char(char c0) => Char(c => c == c0);

        public static readonly Parser<string> Space = Char(char.IsWhiteSpace).Many().Select(string.Concat);

        public static Parser<string> Str(string s0) => s0 == "" ? Return("") : Char(s0[0]).Bind(c => Str(s0.Remove(0, 1)).Select(s => c + s));

        public static Parser<T> Token<T>(this Parser<T> p) => p.Bind(t => Space.Select(_ => t));

        public static Parser<string> Symb(string cs) => Str(cs).Token();
        #endregion
    }
}
