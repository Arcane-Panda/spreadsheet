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



        //Equality
        [TestMethod]
        public void WhiteSpaceEquality()
        {
            Formula f1 = new Formula("2+3");
            Formula f2 = new Formula(" 2 + 3 ");
            Assert.AreEqual(f1,f2);
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
        }

        [TestMethod]
        public void NumberEquality()
        {
            Formula f1 = new Formula("2.0+3.0");
            Formula f2 = new Formula("2+3");
            Assert.AreEqual(f1, f2);
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
    }
}