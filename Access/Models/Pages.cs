using System.Collections.Generic;
using System.Linq;

namespace DORA.Access.Models
{
    public class PagedResults<TEntity>
    {
        public PagedMetaData meta { get; set; }
        public IQueryable<TEntity> query { get; set; }
    }

    public class PagedMetaData
    {
        public int page { get; set; }
        public int size { get; set; }
        public int total { get; set; }
        public int pages { get; set; }
        public string sortBy { get; set; }
        public string sortDir { get; set; }
        public List<string> includes { get; set; }
    }
}
