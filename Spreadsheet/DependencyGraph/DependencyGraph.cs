// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}



    ///Internally, each node is contained as a key in either the Dependents or Dependees dictionary, or both.
    ///
    /// The incoming edges of each node are stored in the value associated with the key in the 
    ///Dependees dictionaries, in the form of a list.
    ///
    ///The outgoing edges of each node are stored in the value associated with the key in the 
    ///Dependents dictionaries, in the form of a list.
    ///
    /// </summary>
    public class DependencyGraph
    {

        private Dictionary<string, List<string>> Dependents;
        private Dictionary<string, List<string>> Dependees;
        private int p_size;
        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            Dependents = new();
            Dependees = new();
            p_size = 0;
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return p_size; }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get 
            {  
                if(Dependees.ContainsKey(s))
                    return Dependees[s].Count; 
                else
                    return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            if (Dependents.ContainsKey(s))
                return Dependents[s].Count > 0;
            else
                return false;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (Dependees.ContainsKey(s))
                return Dependees[s].Count > 0;
            else
                return false;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if(Dependents.ContainsKey(s))
                return Dependents[s];
            else
                return new List<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (Dependees.ContainsKey(s))
                return Dependees[s];
            else
                return new List<string>();
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            //If the DG contains already contains S, make sure that the ordered pair (s,t) 
            //doesn't already exist
            if (Dependents.ContainsKey(s))
            {
                if (Dependents[s].Contains(t))
                {
                    return;
                }
                else
                {
                    //add the dependant to the set of dependants belonging to S
                    Dependents[s].Add(t);

                    //Add s as a dependee of t
                    AddDependee(s, t);

                    //increment size
                    p_size++;
                }
            }
            //if the DG doesn't contain S
            else {
                Dependents.Add(s,new List<string>());
                Dependents[s].Add(t);

                //Add s as a dependee of t
                AddDependee(s, t);

                p_size++;
            }
        }

        /// <summary>
        /// When a dependency is added, in addition to adding to the Dependents map, we also need to create
        /// the back-edge. 
        /// This helper method does that by adding the ordered pair (s,t), where t depends on s, to the map of dependees
        /// </summary>
        private void AddDependee(string s, string t)
        {
            //We don't need to reverify that the pair (s,t) doesn't already exist in 
            //the dependency graph since AddDependency already verifies this
            if (Dependees.ContainsKey(t))
            {
               //add s as a dependee of t
                Dependees[t].Add(s);              
            }
            else
            {
                Dependees.Add(t, new List<string>());
                Dependees[t].Add(s);
            }
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if (Dependents.ContainsKey(s))
            {
                if (Dependents[s].Contains(t))
                {
                    Dependents[s].Remove(t);
                    Dependees[t].Remove(s);
                    p_size--;
                }                         
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            
            //Remove all existing ordered pairs with s, if they exist
            if (Dependents.ContainsKey(s))
            {
                //uses an additional list in order to not be modifiying Dependents[s] during the loop
                List<string> dependentsToRemove = new();
                foreach (string r in Dependents[s])
                {
                    dependentsToRemove.Add(r);
                }
                foreach (string r in dependentsToRemove)
                { 
                    RemoveDependency(s, r);
                }
            }
            //add all the new dependencies
            foreach(string t in newDependents)
            { 
                AddDependency(s, t);
            }
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            //Remove all existing ordered pairs (r,s), if they exist
            if (Dependees.ContainsKey(s))
            {
                //uses an additional list in order to not be modifiying Dependents[s] during the loop
                List<string> dependeesToRemove = new();
                foreach (string r in Dependees[s])
                {
                    dependeesToRemove.Add(r);
                }
                foreach (string r in dependeesToRemove)
                {
                    RemoveDependency(r, s);
                }
            }
            //add all the new dependencies
            foreach (string t in newDependees)
            {
                AddDependency(t, s);
            }
        }

    }

}
