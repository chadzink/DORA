using System;
using System.Collections.Generic;
using System.Linq;
using DORA.Access.Models;

namespace DORA.Access.Helpers
{
    public static class Paging<TEntity>
    {
        public static PagedResults<TEntity> Page(
            IQueryable<TEntity> query,
            int page = 1, int size = 25, string order = null
        )
        {
            // calcualte total record in query
            int queryRecordCount = query.Count();

            if (page > 0 & size > 0)
                query = query.Skip((page - 1) * size).Take(size);

            PagedMetaData pageMeta = new PagedMetaData
            {
                page = page,
                size = size,
                total = queryRecordCount,
                pages = size > 0 ? (int)Math.Ceiling((double)queryRecordCount / size) : 0,
                order = order,
            };

            return new PagedResults<TEntity> { query = query, meta = pageMeta };
        }
    }
}
