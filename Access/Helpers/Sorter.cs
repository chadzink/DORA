using System;
using System.Linq;
using System.Linq.Expressions;

namespace DORA.Access.Helpers
{
    public static class SorterUtility<TEntity>
    {
        public static IQueryable<TEntity> Apply(
            IQueryable<TEntity> query,
            string sortBy,
            string sortDir
        )
        {
            Sorter<TEntity> entitySorter = new Sorter<TEntity>(sortBy);

            if (entitySorter != null && entitySorter.Expr != null)
            {
                if (sortDir == "DESC")
                    query = query.OrderByDescending(entitySorter.Expr);
                else
                    query = query.OrderBy(entitySorter.Expr);
            }
            else throw new Exception("Entity Sort not initialized: " + sortBy + ", " + entitySorter.ToString());

            return query;
        }
    }

    public class Sorter<T>
    {
        #region CLASS PROPERTIES

        public string FieldName { get; private set; }

        #endregion

        public Sorter(string _fieldName)
        {
            this.FieldName = _fieldName;
        }
        //------------------------------------------------------------------------------------------//

        #region CLASS METHODS

        public Expression<Func<T, dynamic>> Expr
        {
            get
            {
                string propertyName = this.FieldName;

                ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
                MemberExpression memberProperty = Expression.Property(parameter, propertyName);

                if (Nullable.GetUnderlyingType(memberProperty.Type) != null)
                    memberProperty = Expression.Property(Expression.Property(parameter, propertyName), "Value");

                return Expression.Lambda<Func<T, dynamic>>(memberProperty, parameter);
            }
        }
        //------------------------------------------------------------------------------------------//

        #endregion
    }
}
