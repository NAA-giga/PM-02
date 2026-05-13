using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
