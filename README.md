# MiPaCo
A Minimal (Monadic) Parser Combinators (MiPaCo) project in C#.
It is very closely modelled on [Monadic Parsing in Haskell](http://www.cs.nott.ac.uk/~pszgmh/pearl.pdf)
by Hutton and Meijer.
In the context of this project *minimal* means hewing as close as possible as possible to the Hutton & Meijer
paper, and making no concessions to efficiency.
For example, the parser for a string is modelled on the haskell 
`string (c:cs) = do {char c; string cs; return (c:cs)}` and is likewise recursive, even though
an iterative version would certainly have been more efficient.
A result of this is the entire library (which is contained in `MiPaCo/Combinators.cs`) is 
just a few dozen lines of code (and most of those lines are helpers; the core lines are 
probably less than a dozen in number).
Thus one way to use the library is to simply copy the relevant lines into whatever
project needs them.

The solution contains two examples of using the library: `CalculatorExample` (drawn directly
from the Hutton & Maijer paper) and `SqlExample` which parses a small subset of SQL 'where'
clauses into an Abstract Syntax Tree.

## Design Notes
The current design models the output of a parser as an `IEnumerable` making it very similar
to the Haskell lists used by Hutton & Meijer.
Other alternatives include an 'Option' class (assuming no more than one parse is generated)
or returning the result of a parse and representing failure by an exception.

The monadic nature of the parsers allows us to implement LINQ extension methods which in turn
allow LINQ query syntax to write parsers.
Alas, LINQ query syntax does not include a `union` keyword which means that choice (i.e.,
the `Or` combinator) requires stepping outside of the query syntax.

## Other Projects
See [Sprache](https://github.com/sprache/Sprache) for another monadic parser combinator library
in C#.
It is probably much more efficient than MiPaCo than better suited for serious work.
