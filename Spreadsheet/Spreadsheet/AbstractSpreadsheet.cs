// Written by Joe Zachary for CS 3500, September 2013
// Change log:
// Last updated: 9/16/22 (Daniel Kopta), updated for non-nullable types

using System;
using System.IO;
using System.Collections.Generic;
using SpreadsheetUtilities;

namespace SS
{

  /// <summary>
  /// Thrown to indicate that a change to a cell will cause a circular dependency.
  /// </summary>
  public class CircularException : Exception
  {
  }


  /// <summary>
  /// Thrown to indicate that a name parameter was invalid.
  /// </summary>
  public class InvalidNameException : Exception
  {
  }


  /// <summary>
  /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
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
  public abstract class AbstractSpreadsheet
  {
    /// <summary>
    /// Enumerates the names of all the non-empty cells in the spreadsheet.
    /// </summary>
    public abstract IEnumerable<string> GetNamesOfAllNonemptyCells();


    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
    /// value should be either a string, a double, or a Formula.
    /// </summary>
    public abstract object GetCellContents( string name );


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
    public abstract IList<string> SetCellContents( string name, double number );

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
    public abstract IList<string> SetCellContents( string name, string text );

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
    public abstract IList<string> SetCellContents( string name, Formula formula );


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
    protected abstract IEnumerable<string> GetDirectDependents( string name );


    /// <summary>
    /// This method is implemented for you, but makes use of your GetDirectDependents.
    /// 
    /// Requires that name be a valid cell name.
    /// 
    /// If the cell referred to by name is involved in a circular dependency,
    /// throws a CircularException.
    /// 
    /// Otherwise, returns an enumeration of the names of all cells whose values must
    /// be recalculated, assuming that the contents of the cell referred to by name has changed.
    /// The cell names are enumerated in an order in which the calculations should be done.  
    /// 
    /// For example, suppose that 
    /// A1 contains 5
    /// B1 contains the formula A1 + 2
    /// C1 contains the formula A1 + B1
    /// D1 contains the formula A1 * 7
    /// E1 contains 15
    /// 
    /// If A1 has changed, then A1, B1, C1, and D1 must be recalculated,
    /// and they must be recalculated in an order which has A1 first, and B1 before C1
    /// (there are multiple such valid orders).
    /// The method will produce one of those enumerations.
    /// 
    /// PLEASE NOTE THAT THIS METHOD DEPENDS ON THE ABSTRACT METHOD GetDirectDependents.
    /// IT WON'T WORK UNTIL GetDirectDependents IS IMPLEMENTED CORRECTLY.
    /// </summary>
    protected IEnumerable<string> GetCellsToRecalculate( string name )
    {
      LinkedList<string> changed = new LinkedList<string>();
      HashSet<string> visited = new HashSet<string>();
      Visit( name, name, visited, changed );
      return changed;
    }


    /// <summary>
    /// A helper for the GetCellsToRecalculate method.
    /// 
    ///   -- You should fully comment what is going on below --
    /// </summary>
    private void Visit( string start, string name, ISet<string> visited, LinkedList<string> changed )
    {
      visited.Add( name );
      foreach ( string n in GetDirectDependents( name ) )
      {
        if ( n.Equals( start ) )
        {
          throw new CircularException();
        }
        else if ( !visited.Contains( n ) )
        {
          Visit( start, n, visited, changed );
        }
      }
      changed.AddFirst( name );
    }

  }
}
