// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private string formula;
        private string formulaAsString;
        private Func<string, string> normalize;
        private List<string> variables = new();
        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        { }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            this.formula = formula;
            this.normalize = normalize;
            int oParenthesisCount = 0;
            int cParenthesisCount = 0;

            string[] tokens = GetTokens(formula).ToArray();

            //check non empty
            if (tokens.Length == 0)
            {
                throw new FormulaFormatException("Formula cannot be empty");
            }

            //check starting token rule
            if (isOperator(tokens[0]) || tokens[0] == ")")
            {
                throw new FormulaFormatException("Formula must start with a number, a variable, or an opening parenthesis.");
            }

            //check ending token rule
            if (isOperator(tokens[tokens.Length - 1]) || tokens[tokens.Length - 1] == "(")
            {
                throw new FormulaFormatException("Formula must end with a number, a variable, or a closing parenthesis.");
            }

            //parse the whole formula checking for correctness
            for (int i = 0; i < tokens.Length; i++)
            {
                if (Regex.IsMatch(tokens[i], @"[a-zA-Z_](?: [a-zA-Z_]|\d)*")) //variable
                {
                    if (isValid(normalize(tokens[i])))
                    {
                        if (!variables.Contains(normalize(tokens[i])))
                        {
                            variables.Add(normalize(tokens[i]));
                        }

                        if (i != tokens.Length - 1 && (tokens[i + 1] != ")" && !isOperator(tokens[i + 1])))
                        {
                            throw new FormulaFormatException("What follows a variable must be either an operator or a closing parenthesis.");
                        }
                    }
                    else
                        throw new FormulaFormatException(tokens[i] + " is an invalid vaariable name");
                }
                else if (tokens[i] == "(") //open parenthesis
                {
                    if (i != tokens.Length - 1 && (tokens[i + 1] == ")" || isOperator(tokens[i + 1])))
                    {
                        throw new FormulaFormatException("What follows an opening parenthesis must be either a number, a variable, or an opening parenthesis");
                    }
                    oParenthesisCount++;
                }
                else if (tokens[i] == ")") //closing parenthesis
                {
                    if (i != tokens.Length - 1 && (tokens[i + 1] != ")" && !isOperator(tokens[i + 1])))
                    {
                        throw new FormulaFormatException("What follows a closing parenthesis must be either an operator or a closing parenthesis.");
                    }
                    cParenthesisCount++;
                    if (cParenthesisCount > oParenthesisCount)
                    {
                        throw new FormulaFormatException("More closing than opening parentheses");
                    }
                }
                else if (isOperator(tokens[i])) //operator
                {
                    if (i != tokens.Length - 1 && (tokens[i + 1] == ")" || isOperator(tokens[i + 1])))
                    {
                        throw new FormulaFormatException("What follows an operator must be either a number, a variable, or an opening parenthesis");
                    }
                }
                else if (Double.TryParse(tokens[i], out double d)) //double
                {

                    if (i != tokens.Length - 1 && (tokens[i + 1] != ")" && !isOperator(tokens[i + 1])))
                    {
                        throw new FormulaFormatException("What follows a number must be either an operator or a closing parenthesis.");
                    }
                }
                else
                { //its some unknown token
                    throw new FormulaFormatException(tokens[i] + " is not valid");
                }
            }

            //check for balanced parenthesis
            if (oParenthesisCount != cParenthesisCount)
            {
                throw new FormulaFormatException("Opening parenthesis do not match closing parenthesis");
            }

            //now that everything is confirmed syntactically correct, save the string version
            formulaAsString = this.ToString();
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            //Declare stacks to be used to evaluate expression
            Stack<double> valueStack = new Stack<double>();
            Stack<String> operatorStack = new Stack<String>();

            //split the expression to be evaluated into tokens        
            List<string> tokens = GetTokens(formula).ToList();

            //evaluate each token according the assignment algorithm, making sure to catch any argument exceptions that result from dividing by zero
            try
            {
                foreach (string t in tokens)
                {
                    if (Double.TryParse(t, out double tokenResult)) //number
                    {
                        if (operatorStack.isOnTop("*") || operatorStack.isOnTop("/"))
                        {
                            double result = performOperation(valueStack, tokenResult, operatorStack);
                            valueStack.Push(result);
                        }
                        else
                            valueStack.Push(tokenResult);
                        
                    }
                    else if (Regex.IsMatch(t, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*")) //variable
                    {
                        try
                        {
                            double varValue = lookup(normalize(t));
                            if (operatorStack.isOnTop("*") || operatorStack.isOnTop("/"))
                            {
                                double result = performOperation(valueStack, varValue, operatorStack);
                                valueStack.Push(result);
                            }
                            else
                                valueStack.Push(varValue);
                        }
                        catch (ArgumentException)
                        {
                            return new FormulaError("Variable " + t + " not found");
                        }
                    }
                    else if (t == "+" || t == "-") //operator
                    {
                        if ((operatorStack.isOnTop("+") || operatorStack.isOnTop("-")))
                        {
                            double result = performOperation(valueStack, operatorStack);
                            valueStack.Push(result);
                        }
                        operatorStack.Push(t);
                    }
                    else if (t == "*" || t == "/")//operator
                    {
                        operatorStack.Push(t);
                    }
                    else if (t == "(")//open parenthesis
                    {
                        operatorStack.Push(t);
                    }
                    else if (t == ")") //close parenthesis
                    {
                        if (operatorStack.isOnTop("+") || operatorStack.isOnTop("-"))
                        {
                            double result = performOperation(valueStack, operatorStack);
                            valueStack.Push(result);
                        }

                        //check that there is an opening (
                        if (operatorStack.isOnTop("("))
                            operatorStack.Pop();                   

                        if (operatorStack.isOnTop("*") || operatorStack.isOnTop("/"))
                        {
                            double result = performOperation(valueStack, operatorStack);
                            valueStack.Push(result);
                        }

                    }
                }

                //once we've evaluated the whole expression, either return the final value, or perform the final operation and return
                if (operatorStack.Count == 0)
                {
                    return valueStack.Pop();
                }
                else
                {
                    double result = performOperation(valueStack, operatorStack);
                    return result;
                }
            }
            catch (ArgumentException)
            {

                return new FormulaError("Can't divide by 0");
            }
        }
        /// <summary>
        /// Helper method for Evaluate that uses 2 values and operator and returns the result of applying the operator to the 2 numbers
        /// 
        /// In this instance, one of the values comes from the current infix stack of values while the second value is the one currently being read
        /// </summary>
        /// <param name="valueStack"> the stack of values to pull from</param>
        /// <param name="val2">the value currently being read</param>
        /// <param name="opStack">the stack of operators</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="ArgumentException"> Throws an exception when there aren't enough values to operate on (i.e the expression was malformed)</exception>
        private static double performOperation(Stack<double> valueStack, double val2, Stack<String> opStack)
        {

            double val1 = valueStack.Pop();
            string op = opStack.Pop();

            switch (op)
            {
                case "*":
                    return val1 * val2;
                case "/":
                    if (val2 == 0)
                        throw new ArgumentException("Division by 0");
                    return val1 / val2;
                case "+":
                    return val1 + val2;
                case "-":
                    return val1 - val2;
            }

            //THIS LINE WILL NEVER RUN BUT THE COMPILER GETS MAD WITHOUT IT
            throw new ArgumentException("Malformed error slipped past the constructor");
        }

        /// <summary>
        /// Helper method for Evaluate that uses 2 values and operator and returns the result of applying the operator to the 2 numbers
        /// 
        /// In this instance, BOTH of the values come from the current infix stack of values 
        /// </summary>
        /// <param name="valueStack"> the stack of values to pull from</param>
        /// <param name="opStack">the stack of operators</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="ArgumentException"> Throws an exception when there aren't enough values to operate on (i.e the expression was malformed)</exception>
        private static double performOperation(Stack<double> valueStack, Stack<String> opStack)
        {
            double val2 = valueStack.Pop();
            double val1 = valueStack.Pop();
            string op = opStack.Pop();
            switch (op)
            {
                case "*":
                    return val1 * val2;
                case "/":
                    if (val2 == 0)
                        throw new ArgumentException("Division by 0");
                    return val1 / val2;
                case "+":
                    return val1 + val2;
                case "-":
                    return val1 - val2;
            }

            //THIS LINE WILL NEVER RUN BUT THE COMPILER GETS MAD WITHOUT IT
            throw new ArgumentException("Malformed error slipped past the constructor");
        }

        /// <summary>
        /// Helper method that determines if a given string is one of 4 legal mathematical operators, those being
        /// * / + or -
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns>If the string is one of those 4</returns>
        private static Boolean isOperator(string s)
        {
            return s == "+" || s == "-" || s == "*"
                || s == "/";
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return variables;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            StringBuilder result = new();
            List<string> formulaTokens = GetTokens(formula).ToList();
            foreach (string s in formulaTokens)
            {
                if (Double.TryParse(s, out double val))
                {
                    result.Append(val);
                    
                }
                else if (Regex.IsMatch(s, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*"))
                {
                    result.Append(normalize(s));
                }
                else
                {
                    result.Append(s);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Formula))
            {
                return false;
            }

            return formulaAsString.Equals(obj.ToString());
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return !f1.Equals(f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return formulaAsString.GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }
    }
    /// <summary>
    /// Provides additional extensions for the stack class
    /// </summary>
    public static class StackExtensions
    {
        /// <summary>
        /// Checks if a given target is at the top of a stack
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="stack">stack to be checked</param>
        /// <param name="target">search target</param>
        /// <returns></returns>
        public static bool isOnTop<T>(this Stack<T> stack, T target)
        {
            //silly extra local variable to get rid of a null dereference warning
            if (stack.Count == 0)
                return false;
            T onTop = stack.Peek();
            if (onTop != null)
                return onTop.Equals(target);
            else
                //THIS LINE WILL NEVER RUN BUT THE COMPILER GETS MAD WITHOUT IT
                return false;
        }
    }
    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}

