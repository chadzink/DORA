using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Models;
using DORA.DotAPI.Context.Entities;

namespace DORA.DotAPI.Helpers
{
    public static class FilterResult<TEntity>
    {
        public static JsonData<TEntity> ToJson(
            IQueryable<TEntity> query,
            User user,
            List<FilterField> filters = null,
            int page = 1, int size = 25
        )
        {
            if (query != null && filters != null)
            {
                query = FilterDataUtility<TEntity>.Apply(query, filters);
            }

            // Apply paging utility
            PagedResults<TEntity> paged = Paging<TEntity>.Page(query, page, size/*, order*/);

            List<TEntity> entities = paged.query.ToList();

            return new JsonData<TEntity>(entities, user.JwtToken, user.RefreshJwtToken, paged.meta);
        }

        public static JsonData<TEntity> ToJson(
            IQueryable<TEntity> query,
            List<FilterField> filters = null,
            int page = 1, int size = 25
        )
        {
            if (query != null && filters != null)
            {
                query = FilterDataUtility<TEntity>.Apply(query, filters);
            }

            // Apply paging utility
            PagedResults<TEntity> paged = Paging<TEntity>.Page(query, page, size/*, order*/);

            List<TEntity> entities = paged.query.ToList();

            return new JsonData<TEntity>(entities, paged.meta);
        }
    }

    public static class FilterDataUtility<TEntity>
    {
        public static IQueryable<TEntity> Apply(
            IQueryable<TEntity> query,
            List<FilterField> filters
        )
        {
            foreach (FilterField filterField in filters)
            {
                Filter<TEntity> entityFilter = new Filter<TEntity>(filterField.Name, filterField.Value, filterField.Operator);

                if (entityFilter != null && entityFilter.Expr != null)
                    query = query.Where(entityFilter.Expr);
            }

            return query;
        }
    }

    public class Filter<T>
    {
        #region CLASS PROPERTIES

        public string FieldName { get; private set; }
        public string FieldValue { get; private set; }
        public string Operator { get; private set; }
        public FilterFieldType FieldType { get; private set; }

        #endregion

        public Filter(string _fieldName, string _fieldValue, string _fieldOperator)
        {
            //convert == to =
            if (_fieldOperator == "==") _fieldOperator = "=";

            //convert <> to !=
            if (_fieldOperator == "<>") _fieldOperator = "!=";

            this.FieldName = _fieldName;
            this.FieldValue = _fieldValue;
            this.Operator = _fieldOperator;
            this.FieldType = this.getFieldType();
        }
        //------------------------------------------------------------------------------------------//

        #region CLASS METHODS

        public Expression<Func<T, bool>> Expr
        {
            get
            {
                // cannot process a field type of none
                if (this.FieldType == FilterFieldType.None)
                    return null;

                string propertyName = this.FieldName;

                ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
                MemberExpression memProperty = Expression.Property(parameter, propertyName);

                Expression target = null;
                Expression expMethod = null;
                Type inputType = null;

                //set the target and type
                switch (this.FieldType)
                {
                    case FilterFieldType.Number:

                        target = Expression.Constant(int.Parse(this.FieldValue));

                        if (memProperty.Type == typeof(short?) || memProperty.Type == typeof(int?) || memProperty.Type == typeof(Int32?))
                            memProperty = Expression.Property(Expression.Property(parameter, propertyName), "Value");

                        inputType = typeof(int);
                        break;

                    case FilterFieldType.Decimal:

                        target = Expression.Constant(decimal.Parse(this.FieldValue));

                        if (memProperty.Type == typeof(decimal?) || memProperty.Type == typeof(Decimal?))
                            memProperty = Expression.Property(Expression.Property(parameter, propertyName), "Value");

                        inputType = typeof(decimal);
                        break;

                    case FilterFieldType.Float:

                        target = Expression.Constant(float.Parse(this.FieldValue));

                        if (memProperty.Type == typeof(float?))
                            memProperty = Expression.Property(Expression.Property(parameter, propertyName), "Value");

                        inputType = typeof(float);
                        break;

                    case FilterFieldType.Date:

                        target = Expression.Constant(DateTime.Parse(this.FieldValue));

                        if (memProperty.Type == typeof(DateTime?))
                            memProperty = Expression.Property(Expression.Property(parameter, propertyName), "Value");

                        inputType = typeof(DateTime);
                        break;

                    case FilterFieldType.String:

                        target = Expression.Constant((String)this.FieldValue);
                        inputType = typeof(String);
                        break;

                    case FilterFieldType.Guid:

                        target = Expression.Constant(Guid.Parse((String)this.FieldValue));
                        inputType = typeof(Guid);
                        break;
                }
                //---------------//

                switch (this.Operator)
                {
                    case "eq":
                    case "=":
                        expMethod = Expression.Equal(memProperty, target);
                        break;

                    case "neq":
                    case "!=":
                        expMethod = Expression.NotEqual(memProperty, target);
                        break;

                    case "gt":
                    case ">":
                        if (this.FieldType != FilterFieldType.String)
                            expMethod = Expression.GreaterThan(memProperty, target);
                        else
                            throw new Exception("Error in class Filter, can not use greater with string type.");
                        break;

                    case "gte":
                    case ">=":
                        if (this.FieldType != FilterFieldType.String)
                            expMethod = Expression.GreaterThanOrEqual(memProperty, target);
                        else
                            throw new Exception("Error in class Filter, can not use greater or equal with string type.");
                        break;

                    case "lt":
                    case "<":
                        if (this.FieldType != FilterFieldType.String)
                            expMethod = Expression.LessThan(memProperty, target);
                        else
                            throw new Exception("Error in class Filter, can not use less with string type.");
                        break;

                    case "lte":
                    case "<=":
                        if (this.FieldType != FilterFieldType.String)
                            expMethod = Expression.LessThanOrEqual(memProperty, target);
                        else
                            throw new Exception("Error in class Filter, can not use less or equal with string type.");
                        break;

                    case "in":
                    case "contains":
                        if (this.FieldType == FilterFieldType.String)
                            expMethod = Expression.Call(memProperty, inputType.GetMethod("Contains", new Type[] { inputType }), target);
                        else
                            throw new Exception("Error in class Filter, can not use contains on non-string type.");
                        break;

                    case "ends":
                        if (this.FieldType == FilterFieldType.String)
                            expMethod = Expression.Call(memProperty, inputType.GetMethod("EndsWith", new Type[] { inputType }), target);
                        else
                            throw new Exception("Error in class Filter, can not use end with on non-string type.");
                        break;

                    case "starts":
                        if (this.FieldType == FilterFieldType.String)
                            expMethod = Expression.Call(memProperty, inputType.GetMethod("StartsWith", new Type[] { inputType }), target);
                        else
                            throw new Exception("Error in class Filter, can not use starts with on non-string type.");
                        break;
                }
                //---------------//

                return (expMethod != null)
                    ? Expression.Lambda<Func<T, bool>>(expMethod, parameter)
                    : null;
            }
        }
        //------------------------------------------------------------------------------------------//

        #endregion

        private FilterFieldType getFieldType()
        {
            Type entityType = typeof(T);
            System.Reflection.PropertyInfo[] props = entityType.GetProperties();

            foreach (System.Reflection.PropertyInfo prop in props)
            {
                if (prop.Name.ToLower() == this.FieldName.ToLower())
                {
                    if (prop.PropertyType == typeof(DateTime)
                       || prop.PropertyType == typeof(DateTime?)
                    )
                    {
                        return FilterFieldType.Date;
                    }
                    else if (prop.PropertyType == typeof(int)
                        || prop.PropertyType == typeof(int?)
                        || prop.PropertyType == typeof(Int32?)
                        || prop.PropertyType == typeof(short)
                        || prop.PropertyType == typeof(short?)
                    )
                    {
                        return FilterFieldType.Number;
                    }
                    else if (prop.PropertyType == typeof(decimal)
                        || prop.PropertyType == typeof(decimal?)
                        || prop.PropertyType == typeof(Decimal)
                        || prop.PropertyType == typeof(Decimal?)
                    )
                    {
                        return FilterFieldType.Decimal;
                    }
                    else if (prop.PropertyType == typeof(float)
                        || prop.PropertyType == typeof(float?)
                    )
                    {
                        return FilterFieldType.Float;
                    }
                    else if (prop.PropertyType == typeof(string)
                        || prop.PropertyType == typeof(String)
                    )
                    {
                        return FilterFieldType.String;
                    }
                    else if (prop.PropertyType == typeof(Guid)
                        || prop.PropertyType == typeof(Guid?)
                    )
                    {
                        return FilterFieldType.Guid;
                    }
                }
            }

            return FilterFieldType.None;
        }
    }


    public class FilterInfo<T>
    {
        public List<string> columns { get; private set; }
        public List<FilterOperatorOptions> operators { get; private set; }

        public FilterInfo()
        {
            this.columns = new List<string>();
            Type entityType = typeof(T);

            foreach (System.Reflection.PropertyInfo prop in entityType.GetProperties())
            {
                this.columns.Add(prop.Name);
            }

            // create operators list
            this.operators = new List<FilterOperatorOptions>() {
                new FilterOperatorOptions { Description = "Contains", Value = "in" },
                new FilterOperatorOptions { Description = "Ends With", Value = "ends" },
                new FilterOperatorOptions { Description = "Starts With", Value = "starts" },
                new FilterOperatorOptions { Description = "Equals", Value = "eq" },
                new FilterOperatorOptions { Description = "No Equal To", Value = "neq" },
                new FilterOperatorOptions { Description = "Greater", Value = "gt" },
                new FilterOperatorOptions { Description = "Greater or Equals", Value = "gte" },
                new FilterOperatorOptions { Description = "Less", Value = "lt" },
                new FilterOperatorOptions { Description = "Less or Equals", Value = "lte" },
            };
        }
    }
}
