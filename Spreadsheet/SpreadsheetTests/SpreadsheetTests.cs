using SS;
using SpreadsheetUtilities;
namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        [TestMethod]
        public void defaultConstructor()
        { 
            Spreadsheet s = new Spreadsheet();
        }

        [TestMethod]
        public void ThreeArgConstructor()
        {
            Spreadsheet s = new Spreadsheet(x => true, x => x, "hello");
            Assert.AreEqual("hello", s.Version);
        }

        [TestMethod]
        public void getContentsOfInvalidCell()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("25"));
        }

        [TestMethod]
        public void getContentsOfEmptyCell()
        {
            Spreadsheet s = new Spreadsheet();
            object result = s.GetCellContents("A1");
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void setInvalid()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("25", "0.4"));
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("25", "hello"));
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("25", "=2+2"));
        }

        [TestMethod]
        public void setEmpty()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "0.4");
            Assert.AreEqual(0.4, s.GetCellContents("A1"));

            s.SetContentsOfCell("A2", "hello");
            Assert.AreEqual("hello", s.GetCellContents("A2"));


            s.SetContentsOfCell("A3", "=2+2");
            Assert.AreEqual(new Formula("2+2"), s.GetCellContents("A3"));
        }

        [TestMethod]
        public void setExisting()
        {
            //double to string
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "0.4");
            s.SetContentsOfCell("A1", "hello");
            Assert.AreEqual("hello", s.GetCellContents("A1"));

            //string to formula
            s.SetContentsOfCell("A2", "hello");
            s.SetContentsOfCell("A2", "=2+2");
            Assert.AreEqual(new Formula("2+2"), s.GetCellContents("A2"));

            //formula to double
            s.SetContentsOfCell("A3", "=2+2");
            s.SetContentsOfCell("A3", "1.23");
            Assert.AreEqual(1.23, s.GetCellContents("A3"));
        }

        [TestMethod]
        public void getNames()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "0.4");
            s.SetContentsOfCell("A2", "0.4");
            s.SetContentsOfCell("B31","0.4");
            s.SetContentsOfCell("C4", "0.4");

            List<string> names = s.GetNamesOfAllNonemptyCells().ToList();
            Assert.AreEqual(4, names.Count);
            Assert.AreEqual("A1", names[0]);
            Assert.AreEqual("A2", names[1]);
            Assert.AreEqual("B31", names[2]);
            Assert.AreEqual("C4", names[3]);
        }


        [TestMethod]
        public void setCellWithDependency()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "2");
            s.SetContentsOfCell("B2", "=A1 + 3");       
        }

        [TestMethod]
        public void setCellWithNestedDependencies()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "2");
            s.SetContentsOfCell("B2", "=A1 + 3");
            s.SetContentsOfCell("C3", "=B2 + 3");
        }

        [TestMethod]
        public void setCellWithDependencyToEmptyCell()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B2", "=A1 + 3");
        }

        [TestMethod]
        public void recalculateCells()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "2");
            s.SetContentsOfCell("B2", "=A1 + 3");
            s.SetContentsOfCell("C3", "=B2 + 3");

            List<string> recalc = s.SetContentsOfCell("A1","4").ToList();
            Assert.AreEqual(3, recalc.Count);
            Assert.AreEqual("A1", recalc[0]);
            Assert.AreEqual("B2", recalc[1]);
            Assert.AreEqual("C3", recalc[2]);
        }

        [TestMethod]
        public void CreateNewCircular()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("B2", "=A1 + 3");
            s.SetContentsOfCell("C3", "=B2 + 3");

            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("A1", "=C3 * 2"));
        }

        [TestMethod]
        public void MakeExistingCircular()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("B2", "=A1 + 3");
            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("B2", "=B2 * 2"));
        }

        [TestMethod]
        public void CreatingCircularRevertsCorrectlyForNewCell()
        {
            Spreadsheet s1 = new Spreadsheet();
            Spreadsheet s2 = new();

            s1.SetContentsOfCell("B2", "=A1 + 3");
            s1.SetContentsOfCell("C3", "=B2 + 3");

            s2.SetContentsOfCell("B2", "=A1 + 3");
            s2.SetContentsOfCell("C3", "=B2 + 3");

            Assert.ThrowsException<CircularException>(() => s1.SetContentsOfCell("A1", "=C3 * 2"));

            List<string> s1Cells = s1.GetNamesOfAllNonemptyCells().ToList();
            List<string> s2Cells = s2.GetNamesOfAllNonemptyCells().ToList();

            Assert.AreEqual(s2Cells.Count, s1Cells.Count);
            Assert.AreEqual(s2Cells[0], s1Cells[0]);
            Assert.AreEqual(s2Cells[1], s1Cells[1]);
        }

        [TestMethod]
        public void CreatingCircularRevertsCorrectlyForExistingCell()
        {
            Spreadsheet s1 = new Spreadsheet();
            Spreadsheet s2 = new();

            s1.SetContentsOfCell("B2", "=A1 + 3");
            s1.SetContentsOfCell("C3", "=B2 + 3");

            s2.SetContentsOfCell("B2", "=A1 + 3");
            s2.SetContentsOfCell("C3", "=B2 + 3");

            Assert.ThrowsException<CircularException>(() => s1.SetContentsOfCell("B2", "=C3 * 2"));

            List<string> s1Cells = s1.GetNamesOfAllNonemptyCells().ToList();
            List<string> s2Cells = s2.GetNamesOfAllNonemptyCells().ToList();

            Assert.AreEqual(s2Cells.Count, s1Cells.Count);
            Assert.AreEqual(s2Cells[0], s1Cells[0]);
            Assert.AreEqual(s2Cells[1], s1Cells[1]);
        }



        //FORMULA VALUES
        //Evaluation
        [TestMethod]
        public void InvalidVarLookup()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "=5 + notAVariable");
            Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));
        }

        [TestMethod]
        public void TestSingleNumber()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "=5");
            Assert.AreEqual(5.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void TestSingleVariable()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "=13");
            s.SetContentsOfCell("B1", "=A1");

            Assert.AreEqual(13.0, s.GetCellValue("B1"));
        }

        [TestMethod]
        public void TestSingleVariableWithReEvaluation()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("B1", "=A1");

            Assert.IsInstanceOfType(s.GetCellValue("B1"), typeof(FormulaError) );

            s.SetContentsOfCell("A1", "=13");

            Assert.AreEqual(13.0, s.GetCellValue("B1"));
        }

        [TestMethod]
        public void TestComplexMultiVar()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "=4");
            s.SetContentsOfCell("B2", "=1");
            s.SetContentsOfCell("C3", "=A1*3-8/2+4*(8-9*2)/14*B2");
            Assert.AreEqual(5.142857142857142, s.GetCellValue("C3"));
        }

        [TestMethod]
        public void TestRepeatedVar()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("a4", "3");
            s.SetContentsOfCell("B2", "=a4-a4*a4/a4");
            Assert.AreEqual(0.0, s.GetCellValue("B2"));
        }

        [TestMethod]
        public void TestComplexNestedParensLeft()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "=((((x1+x2)+x3)+x4)+x5)+x6");
            Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));

            s.SetContentsOfCell("x1", "2");
            Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));

            s.SetContentsOfCell("x2", "2");
            Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));

            s.SetContentsOfCell("x3", "2");
            Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));

            s.SetContentsOfCell("x4", "2");
            Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));

            s.SetContentsOfCell("x5", "2");
            Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));

            s.SetContentsOfCell("x6", "2");
            Assert.AreEqual(12.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void TestReevaluating()
        {
            Spreadsheet s = new();

            s.SetContentsOfCell("A1", "=B2 + 5");
            s.SetContentsOfCell("B2", "2");
            Assert.AreEqual(7.0, s.GetCellValue("A1"));

            s.SetContentsOfCell("B2", "3");
            Assert.AreEqual(8.0, s.GetCellValue("A1"));

            s.SetContentsOfCell("B2", "hello");
            Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));
        }

        //saving
        [TestMethod]
        public void SaveSpreadsheet()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "5");
            s.SetContentsOfCell("B3", "=A1+2");

            //  s.Save("testFile.json");
            Assert.AreEqual(5.0, s.GetCellValue("A1"));
        }

    }
}