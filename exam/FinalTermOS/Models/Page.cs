// Models/Page.cs
namespace FinalTermOS.Models
{
    public class Page
    {
        public int PageNumber { get; set; } // The identifier for the page

        public Page(int pageNumber)
        {
            PageNumber = pageNumber;
        }

        // Override Equals and GetHashCode for proper comparison in collections like List.Contains
        public override bool Equals(object obj)
        {
            return obj is Page page &&
                   PageNumber == page.PageNumber;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PageNumber);
        }

        public override string ToString()
        {
            return PageNumber.ToString();
        }
    }
}