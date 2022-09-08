using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// This static class is used to evaluate integer infix expressions
    /// </summary>
    public static class Evaluator
    {

        public delegate int Lookup(String v);

        /// <summary>
        /// This method takes in an integer infix expression and evaluates it correctly, assuming that the original expression is well formulated.
        /// 
        /// Infix expressions can also include variables with the form of any number of letters followed by any number of numbers.
        /// To evaluated variables, it uses a delegate of the form int Lookup(String v);
        /// 
        /// </summary>
        /// <param name="exp">The expression to be evaluated</param>
        /// <param name="variableEvaluator">delegate method</param>
        /// <returns>The integer result of the expression</returns>
        /// <exception cref="ArgumentNullException">If either of the arguments are null</exception>
        /// <exception cref="ArgumentException">If the given expression is not well formulated</exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            //make sure inputs aren't null
            if (exp == null || variableEvaluator == null)
            {
                throw new ArgumentNullException("Arguments cannot be null");
            }
            //Declare stacks to be used to evaluate expression
            Stack<int> valueStack = new Stack<int>();
            Stack<String> operatorStack = new Stack<String>();

            //Trim white space and split the expression to be evaluated into tokens
            exp = String.Concat(exp.Where(c => !Char.IsWhiteSpace(c)));
            string[] tokens = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            //evaluate each token according the assignment algorithm
            foreach(string t in tokens)
            {

                if (Regex.IsMatch(t, "[a-zA-Z]+[0-9]+")) //variable
                {
                    if (operatorStack.isOnTop("*") || operatorStack.isOnTop("/")) 
                    {
                        int result = performOperation(valueStack, variableEvaluator(t), operatorStack);
                        valueStack.Push(result);
                    }
                    else
                        valueStack.Push(variableEvaluator(t));
                }
                else if (int.TryParse(t, out int tokenResult)) //integer
                {
                    if (operatorStack.isOnTop("*") || operatorStack.isOnTop("/")) 
                    {
                        int result = performOperation(valueStack, tokenResult, operatorStack);
                        valueStack.Push(result);
                    }
                    else
                        valueStack.Push(tokenResult);
                }
                else if (t == "+" || t == "-") //operator
                {
                    if ((operatorStack.isOnTop("+") || operatorStack.isOnTop("-")))
                    {
                        int result = performOperation(valueStack, operatorStack);
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
                        int result = performOperation(valueStack, operatorStack);
                        valueStack.Push(result);
                    }

                    //check that there is an opening (
                    if (operatorStack.isOnTop("("))
                        operatorStack.Pop();
                    else
                        throw new ArgumentException("Malformed expression, missing opening parenthesis");

                    if (operatorStack.isOnTop("*") || operatorStack.isOnTop("/")) 
                    {
                        int result = performOperation(valueStack, operatorStack);
                        valueStack.Push(result);
                    }

                }
                //if the token isn't just an empty string, we throw an expection because its something invalid
                else if(t != "")
                {
                    throw new ArgumentException("Unknown token");
                }
            }

            //once we've evaluated the whole expression, either return the final value, or perform the final operation and return
            if (operatorStack.Count == 0)
            {
                if (valueStack.Count == 1)
                    return valueStack.Pop();
                else
                    throw new ArgumentException("Malformed expression");
            }    
            else
            {
                if(operatorStack.Count == 1 && valueStack.Count == 2)
                {
                    int result = performOperation(valueStack, operatorStack);
                    return result;
                } else
                {
                    throw new ArgumentException("Malformed expression");
                }
                
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
        private static int performOperation(Stack<int> valueStack, int val2, Stack<String> opStack)
        {
            if (valueStack.Count < 1)
                throw new ArgumentException("Tried to perform an operation in a malformed expression");

            int val1 = valueStack.Pop();
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
                default:
                    throw new ArgumentException("Tried to perform an operation with an illegal operator");
                   
            }
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
        private static int performOperation(Stack<int> valueStack, Stack<String> opStack)
        {
            if (valueStack.Count < 2)
                throw new ArgumentException("Tried to perform an operation on a malformed expression");

            int val2 = valueStack.Pop();
            int val1 = valueStack.Pop();
            string op = opStack.Pop();
            switch (op)
            {
                case "*":
                    return val1 * val2;
                case "/":
                    if(val2 == 0)
                        throw new ArgumentException("Division by 0");
                    return val1 / val2;
                case "+":
                    return val1 + val2;
                case "-":
                    return val1 - val2;
                default:
                    throw new ArgumentException("Tried to perform an operation with an illegal operator");

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
            
            return stack.Count > 0 && stack.Peek().Equals(target);
        }
    }
}