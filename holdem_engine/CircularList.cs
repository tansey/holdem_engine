using System;
using System.Collections.Generic;
using System.Text;

namespace holdem_engine
{
    /// <summary>
    /// A list which acts like the List class, but with more of a Java
    /// influence.  This allows you to set a looping variable to true, and creates
    /// a circular list.  Also it utilizes the java iterator pattern of
    /// Next and HasNext.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    /// <typeparam name="T">The type of item that will be stored in the list</typeparam>
    public class CircularList<T> : List<T>
    {
        #region Member Variables
        private bool loop;
        private int index;
        #endregion

        #region Properties

        /// <summary>
        /// The index value that will be used on the next call to Next.
        /// 
        /// </summary>
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        /// <summary>
        /// If true, the list will loop to the beginning when Next
        /// is called after the last element has been accessed.
        /// </summary>
        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }
        
        /// <summary>
        /// The next element in the list.  The user is responsible for
        /// making sure that HasNext is true before getting the next element.
        /// </summary>
        public T Next
        {
            get
            {
                if (Count == 0)
                    throw new ArgumentOutOfRangeException("Next called with no elements in the list!");
                if (index >= Count)
                {
                    if (!loop)
                    {
#if DEBUG
                        throw new ArgumentOutOfRangeException("No more elements in the list");
#else
                        return default(T);
#endif
                    }
                    index = 0;
                }
                T result = this[index];
                index++;
                return result;
            }
        }

        /// <summary>
        /// Tells whether there is another element in the list
        /// </summary>
        public bool HasNext
        {
            get
            {
                if (Count == 0 || (!loop && index >= Count))
                {
                    return false;
                }
                return true;
            }
        }
        #endregion

        public CircularList()
        {
            loop = true;
            index = 0;
        }

        public CircularList( bool loop )
        {
            this.loop = loop;
            index = 0;
        }

        public new bool Remove(T element)
        {
            if (Contains(element) && IndexOf(element) < index)
                index--;
           return base.Remove(element);
        }

    }
}
