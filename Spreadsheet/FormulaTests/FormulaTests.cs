using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace FormulaTests
{
    [TestClass]
    public class FormulaTests
    {
        //Syntax errors
        [TestMethod]
        public void EmptyFormula()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula(""));
        }

        [TestMethod]
        public void InvalidTokens()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>( () =>  f = new Formula("5 + 5 - $4")); 
        }

        [TestMethod]
        public void MustContainOneToken()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("")); 
        }

        [TestMethod]
        public void ClosingParenthesisShouldNotExceedOpening()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("(5+3) + 6+7)")); 
        }

        [TestMethod]
        public void BalancedParenthesisCount()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("((5+3) + (6+7)")); 
        }

        [TestMethod]
        public void IllegalStartingToken()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("*(5+3) + (6+7)")); 
        }
        [TestMethod]
        public void IllegalEndingToken()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("(5+3) + (6+7)*")); 
        }

        [TestMethod]
        public void ParenthesisFollowingRule()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("6 + (+5)")); 
        }

        [TestMethod]
        public void OperatorFollowingRule()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("6 + *(5-3)")); 
        }

        [TestMethod]
        public void ExtraFollowingRule()
        {
            Formula f;
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("6 + (5-3)5"));
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("2 + a1 3"));
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("6 + (5-3) + 5 ("));
        }

        //Variables
        [TestMethod]
        public void ValidVariable()
        {
            Formula f = new Formula("2 + a1");
        }

        [TestMethod]
        public void InvalidVariable()
        {
            Formula f;
            //normalize(v) does not return a legal variable
            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("2 + a1", s => "notValid", s => s.Equals("Valid") ));

            Assert.ThrowsException<FormulaFormatException>(() => f = new Formula("2 + a1", s => s, s => false ));
        }

        [TestMethod]
        public void variableEnumeration()
        {
            Formula f1 = new Formula("x+y*z", s => s.ToUpper(), s => true);
            string[] f1Vars = f1.GetVariables().ToArray();
            Assert.AreEqual(3, f1Vars.Length);
            Assert.AreEqual(f1Vars[0], "X");
            Assert.AreEqual(f1Vars[1], "Y");
            Assert.AreEqual(f1Vars[2], "Z");


            Formula f2 = new Formula("x+X*z", s => s.ToUpper(), s => true);
            string[] f2Vars = f2.GetVariables().ToArray();
            Assert.AreEqual(2, f2Vars.Length);
            Assert.AreEqual(f2Vars[0], "X");
            Assert.AreEqual(f2Vars[1], "Z");

            Formula f3 = new Formula("x+X*z");
            string[] f3Vars = f3.GetVariables().ToArray();
            Assert.AreEqual(3, f3Vars.Length);
            Assert.AreEqual(f3Vars[0], "x");
            Assert.AreEqual(f3Vars[1], "X");
            Assert.AreEqual(f3Vars[2], "z");
        }



        //Equality
        [TestMethod]
        public void WhiteSpaceEquality()
        {
            Formula f1 = new Formula("2+3");
            Formula f2 = new Formula(" 2 + 3 ");
            Assert.AreEqual(f1,f2);
            Assert.IsTrue(f1 == f2);
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
        }

        [TestMethod]
        public void NumberEquality()
        {
            Formula f1 = new Formula("2.0+3.0");
            Formula f2 = new Formula("2+3");
            Assert.AreEqual(f1, f2);
            Assert.IsFalse(f1 != f2);
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
        }

        [TestMethod]
        public void variableEquality()
        {
            Formula f1 = new Formula("x + y", s => s.ToUpper(), s => true);
            Formula f2 = new Formula("X+Y");
            Assert.AreEqual(f1, f2);
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
        }

        [TestMethod]
        public void nullEquality()
        {
            Formula f1 = new Formula("2.0+3.0");
            Assert.AreNotEqual(f1, null);
            Assert.AreNotEqual(f1, 3);
        }

        //Evaluation
        [TestMethod]
        public void TestSingleNumber()
        {
            Assert.AreEqual(5.0, new Formula("5").Evaluate(s => 0));
        }

        [TestMethod]
        public void TestSingleVariable()
        {
            Assert.AreEqual(13.0, new Formula("a1").Evaluate( s => 13));
        }

        [TestMethod]
        public void TestBasicOperatorsAndParenthesis()
        {
            Assert.AreEqual(18.0, new Formula("(1+2) + (3*4) + (6/3) + (2-1)").Evaluate(s => 0));
        }

        [TestMethod]
        public void TestArithmeticWithVariable()
        {
            Assert.AreEqual(6.0, new Formula("2+a1").Evaluate( s => 4));
        }

        [TestMethod]
        public void UnknownVariable()
        {
            object result = new Formula("2+a1").Evaluate(s => { throw new ArgumentException(); });
            Assert.IsTrue( result is FormulaError);
        }

        [TestMethod]
        public void DivideByZero()
        {
            object result = new Formula("2/0").Evaluate(s => 0);
            Assert.IsTrue(result is FormulaError);
        }
    }
}