using FormulaEvaluator;

namespace EvaluatorTester
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(Evaluator.Evaluate("(2 + A6) * 5 + 2 *", noVariables));
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
            
        }

        public static int noVariables(String v)
        {
            return 0;        
        }
    }
}