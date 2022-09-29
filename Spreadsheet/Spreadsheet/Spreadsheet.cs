using SpreadsheetUtilities;
using System.Text.RegularExpressions;

namespace SS
{
    /// <summary>
    /// A Spreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a valid cell name if and only if:
    ///   (1) its first character is an underscore or a letter
    ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
    /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
    /// 
    /// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
    /// "25", "2x", and "&" are not.  Cell names are case sensitive, so "x" and "X" are
    /// different cell names.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected).
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// We are not concerned with values in PS4, but to give context for the future of the project,
    /// the value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid). 
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        //Dictionary containing all name -> cell pairings
        private Dictionary<string,Cell> nonEmpty;

        //Dependency graph containing all dependent/dependee relationships in the spreadsheet
        private DependencyGraph dependencyGraph;

        /// <summary>
        /// Zero argument constructor
        /// </summary>
        public Spreadsheet()
        {
            nonEmpty = new();
            dependencyGraph = new();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        /// <param name="name">Name of cell</param>
        /// <returns>Contents of the named cell</returns>
        /// <exception cref="InvalidNameException">The given name is invalid</exception>
        public override object GetCellContents(string name)
        {
            //check for valid name
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }

            //if the cell is non-empty, return its contents, otherwise return an empty string
            if (nonEmpty.TryGetValue(name, out Cell? result))
            { 
                return result.Contents;
            } else
            {
                return "";
            }
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        /// <returns>Names of all non-empty cells in an IEUnumerable</returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            //iterate through the dictionary and return all the key values
            foreach (var cell in nonEmpty)
            {
                yield return cell.Key;
            }
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">Name of cell</param>
        /// <param name="number">double to set as cell's contents</param>
        /// <returns>List with name + names of all indirect or direct dependents</returns>
        /// <exception cref="InvalidNameException"></exception>
        public override IList<string> SetCellContents(string name, double number)
        {
            //check for valid name
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }

            //if the cell is not empty, modify its contents and return the cells needed to recalculate
            if (nonEmpty.ContainsKey(name))
            {
                //remove dependencies 
                dependencyGraph.ReplaceDependees(name, new List<string>());

                nonEmpty[name].Contents = number;
                return this.GetCellsToRecalculate(name).ToList();
            }
            else
            {
                //if the cell is empty, create a new cell with the contents and add it to the dictionary
                //and then return the cells neede to recalculate
                nonEmpty.Add(name, new Cell(number));
                return this.GetCellsToRecalculate(name).ToList();
            }
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">Name of cell</param>
        /// <param name="text">string to set as cell's contents</param>
        /// <returns>List with name + names of all indirect or direct dependents</returns>
        /// <exception cref="InvalidNameException">Cell name was invalid</exception>
        public override IList<string> SetCellContents(string name, string text)
        {
            //check for valid name
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }

            //if trying to explicitily make a cell empty, dont do anything
            if (text.Equals(""))
                return new List<string>();

            //if the cell is not empty, modify its contents and return the cells needed to recalculate
            if (nonEmpty.ContainsKey(name))
            {
                //remove dependencies 
                dependencyGraph.ReplaceDependees(name, new List<string>());

                nonEmpty[name].Contents = text;
                return this.GetCellsToRecalculate(name).ToList();
            }
            else
            {
                //if the cell is empty, create a new cell with the contents and add it to the dictionary
                //and then return the cells neede to recalculate
                nonEmpty.Add(name, new Cell(text));
                return this.GetCellsToRecalculate(name).ToList();
            }
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">Name of cell to change</param>
        /// <param name="formula">The formula object to set as its content</param>
        /// <returns>List with name + names of all indirect or direct dependents</returns>
        /// <exception cref="CircularException">Change would have caused a circular dependency</exception>
        public override IList<string> SetCellContents(string name, Formula formula)
        {
            //check for valid name
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }

            //get the new dependees from the formula
            List<string> NewDependeeVars = formula.GetVariables().ToList();

            //if the cell is not empty, modify its contents and return the cells needed to recalculate
            if (nonEmpty.ContainsKey(name))
            {
                //get old dependee vars and old contents in case a change needs to be reverted
                List<string> OldDependeeVars = dependencyGraph.GetDependees(name).ToList();
                object OldContents = nonEmpty[name].Contents;

                //modify the contents of the cell
                nonEmpty[name].Contents = formula;

                //update dependency graph
                dependencyGraph.ReplaceDependees(name, NewDependeeVars);

                //make sure that no circular dependency exists
                try
                {
                    return this.GetCellsToRecalculate(name).ToList();
                }
                catch (CircularException e)
                {
                    //if the change made it ciruclar, undo change that was made
                    dependencyGraph.ReplaceDependees(name, OldDependeeVars);
                    nonEmpty[name].Contents = OldContents;
                    throw e;
                }
            }
            else
            {
                //if the cell is empty, create a new cell with the contents and add it to the dictionary
                nonEmpty.Add(name, new Cell(formula));

                //add it to the dependency graph using the variables in the formula
                dependencyGraph.ReplaceDependees(name, NewDependeeVars);

                //make sure that no circular dependency exists
                try
                {
                    return this.GetCellsToRecalculate(name).ToList();
                }
                catch (CircularException e)
                {
                    //if the change made it ciruclar, undo change that was made
                    nonEmpty.Remove(name);
                    dependencyGraph.ReplaceDependees(name, new List<string>());
                    throw e;
                }
                
            }
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        /// <param name="name">Name of the cell</param>
        /// <returns>IENumerable of its direct dependents</returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return dependencyGraph.GetDependents(name);
        }

        /// <summary>
        /// Helper method that checks if a given string is a valid cell name
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool IsValidName(string s)
        {
            return Regex.IsMatch(s, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

      

        /// <summary>
        /// This class represents a non-empty cell in a spreadsheet
        /// 
        /// Currently, it only contains the Contents since for PS4 we are not worrying about value
        /// </summary>
        private class Cell
        { 
            //contents of the cell
            public object Contents
            { 
                get;
                set;
            }  

            /// <summary>
            /// Constructor that takes in some contents
            /// </summary>
            /// <param name="contents"></param>
            public Cell(object contents)
            {
                Contents = contents;
            }
        }
    }

    
}