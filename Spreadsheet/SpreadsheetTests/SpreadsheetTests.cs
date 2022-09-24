using SS;
using SpreadsheetUtilities;
namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
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
            Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("25", 0.4));
            Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("25", "hello"));
            Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("25", new Formula("2+2")));
        }

        [TestMethod]
        public void setEmpty()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 0.4);
            Assert.AreEqual(0.4, s.GetCellContents("A1"));

            s.SetCellContents("A2", "hello");
            Assert.AreEqual("hello", s.GetCellContents("A2"));


            s.SetCellContents("A3", new Formula("2+2"));
            Assert.AreEqual(new Formula("2+2"), s.GetCellContents("A3"));
        }

        [TestMethod]
        public void setExisting()
        {
            //double to string
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 0.4);
            s.SetCellContents("A1", "hello");
            Assert.AreEqual("hello", s.GetCellContents("A1"));

            //string to formula
            s.SetCellContents("A2", "hello");
            s.SetCellContents("A2", new Formula("2+2"));
            Assert.AreEqual(new Formula("2+2"), s.GetCellContents("A2"));

            //formula to double
            s.SetCellContents("A3", new Formula("2+2"));
            s.SetCellContents("A3", 1.23);
            Assert.AreEqual(1.23, s.GetCellContents("A3"));
        }

        [TestMethod]
        public void getNames()
        {
            Spreadsheet s = new();
            s.SetCellContents("A1", 0.4);
            s.SetCellContents("A2", 0.4);
            s.SetCellContents("B31", 0.4);
            s.SetCellContents("C4", 0.4);

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
            s.SetCellContents("A1", 2);
            s.SetCellContents("B2", new Formula("A1 + 3"));       
        }

        [TestMethod]
        public void setCellWithNestedDependencies()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 2);
            s.SetCellContents("B2", new Formula("A1 + 3"));
            s.SetCellContents("C3", new Formula("B2 + 3"));
        }

        [TestMethod]
        public void setCellWithDependencyToEmptyCell()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("B2", new Formula("A1 + 3"));
        }

        [TestMethod]
        public void recalculateCells()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 2);
            s.SetCellContents("B2", new Formula("A1 + 3"));
            s.SetCellContents("C3", new Formula("B2 + 3"));

            List<string> recalc = s.SetCellContents("A1",4).ToList();
            Assert.AreEqual(3, recalc.Count);
            Assert.AreEqual("A1", recalc[0]);
            Assert.AreEqual("B2", recalc[1]);
            Assert.AreEqual("C3", recalc[2]);
        }

        [TestMethod]
        public void CreateNewCircular()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("B2", new Formula("A1 + 3"));
            s.SetCellContents("C3", new Formula("B2 + 3"));

            Assert.ThrowsException<CircularException>(() => s.SetCellContents("A1", new Formula("C3 * 2")));
        }

        [TestMethod]
        public void MakeExistingCircular()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("B2", new Formula("A1 + 3"));
            Assert.ThrowsException<CircularException>(() => s.SetCellContents("B2", new Formula("B2 * 2")));
        }

        [TestMethod]
        public void CreatingCircularRevertsCorrectlyForNewCell()
        {
            Spreadsheet s1 = new Spreadsheet();
            Spreadsheet s2 = new();

            s1.SetCellContents("B2", new Formula("A1 + 3"));
            s1.SetCellContents("C3", new Formula("B2 + 3"));

            s2.SetCellContents("B2", new Formula("A1 + 3"));
            s2.SetCellContents("C3", new Formula("B2 + 3"));

            Assert.ThrowsException<CircularException>(() => s1.SetCellContents("A1", new Formula("C3 * 2")));

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

            s1.SetCellContents("B2", new Formula("A1 + 3"));
            s1.SetCellContents("C3", new Formula("B2 + 3"));

            s2.SetCellContents("B2", new Formula("A1 + 3"));
            s2.SetCellContents("C3", new Formula("B2 + 3"));

            Assert.ThrowsException<CircularException>(() => s1.SetCellContents("B2", new Formula("C3 * 2")));

            List<string> s1Cells = s1.GetNamesOfAllNonemptyCells().ToList();
            List<string> s2Cells = s2.GetNamesOfAllNonemptyCells().ToList();

            Assert.AreEqual(s2Cells.Count, s1Cells.Count);
            Assert.AreEqual(s2Cells[0], s1Cells[0]);
            Assert.AreEqual(s2Cells[1], s1Cells[1]);
        }

    }
}