using FormulaEvaluator;

namespace EvaluatorTester
{
    internal class Program
    {
        
        static void Main(string[] args)
        {

            Console.WriteLine(Evaluator.Evaluate("10 + 8 * 5 + 4", noVariables));
        }

        public static int noVariables(String v)
        {
            return 0;        
        }
    }
}