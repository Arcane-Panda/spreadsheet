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
            // TODO ...
            Stack<int> valueStack = new Stack<int>();
            Stack<String> operatorStack = new Stack<String>();

            //Trim white space and split the expression to be evaluated into tokens
            exp = exp.Trim();
            string[] tokens = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            foreach(string t in tokens)
            {
                //need to do case of variable
                //case of int
                if (int.TryParse(t, out int tokenResult))
                {
                    if (operatorStack.Peek() == "*" || operatorStack.Peek() == "/")
                    {
                        int result = performOperation(valueStack.Pop(), tokenResult, operatorStack.Pop());
                        valueStack.Push(result);
                    }
                    else
                        valueStack.Push(tokenResult);
                }
                else if (t == "+" || t == "-")
                {
                    if (operatorStack.Peek() == "+" || operatorStack.Peek() == "-")
                    {
                        int result = performOperation(valueStack.Pop(), valueStack.Pop(), operatorStack.Pop());
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
                        int result = performOperation(valueStack.Pop(), valueStack.Pop(), operatorStack.Pop());
                        valueStack.Push(result);
                    }

                    //check that there is an opening (
                    if(operatorStack.Peek() == "(")
                        operatorStack.Pop();

                    if (operatorStack.Peek() == "*" || operatorStack.Peek() == "/")
                    {
                        int result = performOperation(valueStack.Pop(), valueStack.Pop(), operatorStack.Pop());
                        valueStack.Push(result);
                    }

                }
            }

            if (operatorStack.Count == 0)
                return valueStack.Pop();
            else
            {
                int result = performOperation(valueStack.Pop(), valueStack.Pop(), operatorStack.Pop());
                return result;
            }
        }

        private static int performOperation(int val1, int val2, String op)
        {
            switch (op)
            {
                case "*":
                    return val1 * val2;
                case "/":
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