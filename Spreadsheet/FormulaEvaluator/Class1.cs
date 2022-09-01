using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// 
    /// </summary>
    public static class Evaluator
    {
        // TODO: Follow the PS1 instructions

        public delegate int Lookup(String v);
        
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            //Declare stacks to be used to evaluate expression
            Stack<int> valueStack = new Stack<int>();
            Stack<String> operatorStack = new Stack<String>();

            //Trim white space and split the expression to be evaluated into tokens
            exp = exp.Trim();
            string[] tokens = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            //evaluate each token according the assignment algorithm
            foreach(string t in tokens)
            {
                
                if (Regex.IsMatch(t, "[a-zA-Z]+[0-9]+"))
                {
                    if (operatorStack.Count > 0 && (operatorStack.Peek() == "*" || operatorStack.Peek() == "/"))
                    {
                        int result = performOperation(valueStack, variableEvaluator(t), operatorStack);
                        valueStack.Push(result);
                    }
                    else
                        valueStack.Push(variableEvaluator(t));
                }
                else if (int.TryParse(t, out int tokenResult))
                {
                    if (operatorStack.Count > 0 && (operatorStack.Peek() == "*" || operatorStack.Peek() == "/"))
                    {
                        int result = performOperation(valueStack, tokenResult, operatorStack);
                        valueStack.Push(result);
                    }
                    else
                        valueStack.Push(tokenResult);
                }
                else if (t == "+" || t == "-")
                {
                    if (operatorStack.Count > 0 && (operatorStack.Peek() == "+" || operatorStack.Peek() == "-"))
                    {
                        int result = performOperation(valueStack, operatorStack);
                        valueStack.Push(result);
                    }
                    operatorStack.Push(t);
                }
                else if (t == "*" || t == "/")
                {
                    operatorStack.Push(t);
                }
                else if (t == "(")
                {
                    operatorStack.Push(t);
                }
                else if (t == ")")
                {
                    if (operatorStack.Peek() == "+" || operatorStack.Peek() == "-")
                    {
                        int result = performOperation(valueStack, operatorStack);
                        valueStack.Push(result);
                    }

                    //check that there is an opening (
                    if (operatorStack.Peek() == "(")
                        operatorStack.Pop();

                    if (operatorStack.Peek() == "*" || operatorStack.Peek() == "/")
                    {
                        int result = performOperation(valueStack, operatorStack);
                        valueStack.Push(result);
                    }

                }
            }

            if (operatorStack.Count == 0)
                return valueStack.Pop();
            else
            {
                int result = performOperation(valueStack, operatorStack);
                return result;
            }
        }

        /// <summary>
        /// Helper method for Evaluate that uses 2 values and operator and returns the result of applying the operator to the 2 numbers
        /// 
        /// In this instance, one of the values comes from the current infix stack of values while the second value is the one currently being read
        /// </summary>
        /// <param name="valueStack"></param>
        /// <param name="val2"></param>
        /// <param name="opStack"></param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="ArgumentException"></exception>
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
        /// <param name="valueStack"></param>
        /// <param name="opStack"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static int performOperation(Stack<int> valueStack, Stack<String> opStack)
        {
            if (valueStack.Count < 2)
                throw new ArgumentException("Tried to perform an operation on a malformed expression");

            int val1 = valueStack.Pop();
            int val2 = valueStack.Pop();
            string op = opStack.Pop();
            switch (op)
            {
                case "*":
                    return val1 * val2;
                case "/":
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
}