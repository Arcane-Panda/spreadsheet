using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

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
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        {
            get;
            protected set;
        }

        /// <summary>
        /// Zero argument constructor
        /// </summary>
        public Spreadsheet() : base(x => true, x => x, "default")
        {
            nonEmpty = new();
            dependencyGraph = new();
            Changed = false;
        }

        /// <summary>
        /// Constructor that allows user to provide a validity delegate, normalizer, delegate, and a version
        /// </summary>
        /// <param name="validator">delegate that takes in a string and returns a bool</param>
        /// <param name="normalizer">delegate that takes and returns a string</param>
        /// <param name="version"></param>
        public Spreadsheet(Func<string, bool> validator, Func<string, string> normalizer, string version) : base(validator, normalizer, version)
        {
            nonEmpty = new();
            dependencyGraph = new();
            Changed = false;
        }

        /// <summary>
        /// Constructor that allows user to load a spreadsheet file from a specified path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="validator"></param>
        /// <param name="normalizer"></param>
        /// <param name="version"></param>
        public Spreadsheet(string path, Func<string, bool> validator, Func<string, string> normalizer, string version) : base(validator, normalizer, version)
        {
            nonEmpty = new();
            dependencyGraph = new();
            Changed = false;
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
            name = Normalize(name);

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
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            name = Normalize(name);

            //check for valid name
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }

            //if the cell is non-empty, return its value, otherwise return an empty string
            if (nonEmpty.TryGetValue(name, out Cell? result))
            {
                
                return result.Value;
            }
            else
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
        /// Writes the contents of this spreadsheet to the named file using a JSON format.
        /// The JSON object should have the following fields:
        /// "Version" - the version of the spreadsheet software (a string)
        /// "cells" - an object containing 0 or more cell objects
        ///           Each cell object has a field named after the cell itself 
        ///           The value of that field is another object representing the cell's contents
        ///               The contents object has a single field called "stringForm",
        ///               representing the string form of the cell's contents
        ///               - If the contents is a string, the value of stringForm is that string
        ///               - If the contents is a double d, the value of stringForm is d.ToString()
        ///               - If the contents is a Formula f, the value of stringForm is "=" + f.ToString()
        /// 
        /// For example, if this spreadsheet has a version of "default" 
        /// and contains a cell "A1" with contents being the double 5.0 
        /// and a cell "B3" with contents being the Formula("A1+2"), 
        /// a JSON string produced by this method would be:
        /// 
        /// {
        ///   "cells": {
        ///     "A1": {
        ///       "stringForm": "5"
        ///     },
        ///     "B3": {
        ///       "stringForm": "=A1+2"
        ///     }
        ///   },
        ///   "Version": "default"
        /// }
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            throw new NotImplementedException();
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
        protected override IList<string> SetCellContents(string name, double number)
        {
            //if the cell is not empty, modify its contents and return the cells needed to recalculate
            if (nonEmpty.ContainsKey(name))
            {
                //remove dependencies 
                dependencyGraph.ReplaceDependees(name, new List<string>());

                nonEmpty[name].Contents = number;
                nonEmpty[name].Value = number;

                List<string> cellsToEvaluate = this.GetCellsToRecalculate(name).ToList();

                //Try and evaluate all the cells that are directly/indirectly dependent on named cell
                evaluateCells(cellsToEvaluate);

                return cellsToEvaluate;
            }
            else
            {
                //if the cell is empty, create a new cell with the contents and add it to the dictionary
                //and then return the cells neede to recalculate
                nonEmpty.Add(name, new Cell(number, number));
                

                List<string> cellsToEvaluate = this.GetCellsToRecalculate(name).ToList();

                //Try and evaluate all the cells that are directly/indirectly dependent on named cell
                evaluateCells(cellsToEvaluate);

                return cellsToEvaluate;
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
        protected override IList<string> SetCellContents(string name, string text)
        {

            //if trying to explicitily make a cell empty, dont do anything
            if (text.Equals(""))
                return new List<string>();

            //if the cell is not empty, modify its contents and return the cells needed to recalculate
            if (nonEmpty.ContainsKey(name))
            {
                //remove dependencies 
                dependencyGraph.ReplaceDependees(name, new List<string>());

                nonEmpty[name].Contents = text;
                nonEmpty[name].Value = text;
                List<string> cellsToEvaluate = this.GetCellsToRecalculate(name).ToList();

                //Try and evaluate all the cells that are directly/indirectly dependent on named cell
                evaluateCells(cellsToEvaluate);

                return cellsToEvaluate;
            }
            else
            {
                //if the cell is empty, create a new cell with the contents and add it to the dictionary
                //and then return the cells neede to recalculate
                nonEmpty.Add(name, new Cell(text, text));

                List<string> cellsToEvaluate = this.GetCellsToRecalculate(name).ToList();

                //Try and evaluate all the cells that are directly/indirectly dependent on named cell
                evaluateCells(cellsToEvaluate);

                return cellsToEvaluate;
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
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
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
                    
                    List<string> cellsToEvaluate = this.GetCellsToRecalculate(name).ToList();

                    //Try and evaluate all the cells that are directly/indirectly dependent on named cell
                    evaluateCells(cellsToEvaluate);

                    return cellsToEvaluate;
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
                nonEmpty.Add(name, new Cell(formula, formula.Evaluate(varLookUp)));

                //add it to the dependency graph using the variables in the formula
                dependencyGraph.ReplaceDependees(name, NewDependeeVars);

                //make sure that no circular dependency exists
                try
                {
                    List<string> cellsToEvaluate = this.GetCellsToRecalculate(name).ToList();

                    //Try and evaluate all the cells that are directly/indirectly dependent on named cell
                    evaluateCells(cellsToEvaluate);

                    return cellsToEvaluate;
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
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            name = Normalize(name);
            //check for valid name
            if (!IsValidName(name))
                throw new InvalidNameException();

            //check if content is a double
            if (Double.TryParse(content, out double result))
            {
                Changed = true;
                return SetCellContents(name, result);
            }
            else
            //check if its a formula
            if (content[0].Equals('='))
            {
                String formula = content.Substring(1);
                try
                {
                    Changed = true;
                    return SetCellContents(name, new Formula(formula, Normalize, IsValid));
                }
                //Catches a FormulaFormatException or a CircularException
                catch (Exception)
                {                   
                    throw;
                }

            }
            else
            //set it to a string
            {
                Changed = true;
                return SetCellContents(name, content);
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
            return Regex.IsMatch(s, @"^[a-zA-Z]+[0-9]+$") && IsValid(s);
        }

        /// <summary>
        /// Lookup delegate to be used when evaluating cells
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private double varLookUp(string name)
        {
            //looks up the cell's value by its name (which has already been normalized in Evaluate())
            try
            {
                object value = this.GetCellValue(name);
                if (value is double)
                    return (double) value;
                else
                    //throws an argument exception if its not a double
                    throw new ArgumentException();
            }
            catch (InvalidNameException)
            {
                //also throw if the variable cant be found
                throw new ArgumentException(); ;
            }
            
        }

        private void evaluateCells(List<string> listOfCellNames)
        {
            //iterate through list
            foreach (string names in listOfCellNames)
            {
                //get the cell out of the dictionary
                if (nonEmpty.TryGetValue(names, out Cell? cell))
                {
                    if (cell.Contents is Formula)
                    {
                        //get the formula out of the cell
                        Formula f = (Formula)cell.Contents;
                        //evaluate the formula
                        cell.Value = f.Evaluate(varLookUp);
                    }
                    else if (cell.Contents is double)
                    { 
                        cell.Value = cell.Contents;
                    }
                    
                }
            }
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

            public object Value
            {
                get;
                set;
            }

            /// <summary>
            /// Constructor that takes in some contents
            /// </summary>
            /// <param name="contents"></param>
            public Cell(object contents, object value)
            {
                Contents = contents;
                Value = value;

            }
        }
    }

    
}