using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boxes.Helpers.AutoGrid
{
    public struct Box : IComparable
    {
        //To be used to save and load boxes.
        public bool IsVisible { get; set; }
        public int Row { get; init; }
        public int Column { get; init; }
        public int Column_Span { get; init; }
        public int Row_Span { get; init; }

        public int CompareTo(object obj)
        {
            //returns position indicator.
            //1 if a>b, 0 if a==b, and -1 if a<b
            return this.CompareTo(obj);
        }
    }

    public class Boxes
    {
        IDictionary<int, Box> boxes_in_columns { get; set; }
        IDictionary<Box, int> boxes_in_rows { get; set; }
        ICollection<Box> boxCollection { get; set; }
    }

    public interface iBox
    {
        public Box Create(Box input);
        public Box Read(Box input);
        public Box Update(Box input);
        public void Delete(Box input);
    }
}
