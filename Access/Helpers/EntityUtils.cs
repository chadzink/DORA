using System;
using System.Collections.Generic;
using DORA.Access.Models;

namespace Access.Helpers
{
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
                if (!prop.PropertyType.Name.StartsWith("ICollection"))
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

    public class EntityTypeScript<T>
    {
        private Dictionary<string,string> columns { get; set; }

        public EntityTypeScript()
        {
            this.columns = new Dictionary<string, string>();
            Type entityType = typeof(T);

            foreach (System.Reflection.PropertyInfo prop in entityType.GetProperties())
            {
                this.columns.Add(prop.Name, this.mapJSTypeName(prop.PropertyType));
            }
        }

        public string TypeDefinition
        {
            get
            {
                Type entityType = typeof(T);

                string result = "export type I" + entityType.Name + " = IEntity & {";
                foreach (string columnName in this.columns.Keys)
                {
                    result += "\n\t" + columnName + ": " + this.columns[columnName] + ";";
                }
                result += "\n}";

                return result;
            }
        }

        private string mapJSTypeName(Type propType)
        {
            if (propType == typeof(DateTime)
                || propType == typeof(DateTime?)
            ) return "date";
            else if (propType == typeof(int)
                || propType == typeof(int?)
                || propType == typeof(Int32?)
                || propType == typeof(short)
                || propType == typeof(short?)
            ) return "number";
            else if (propType == typeof(decimal)
                || propType == typeof(decimal?)
                || propType == typeof(Decimal)
                || propType == typeof(Decimal?)
            ) return "number";
            else if (propType == typeof(float)
                || propType == typeof(float?)
            ) return "number";
            else if (propType == typeof(string)
                || propType == typeof(String)
            ) return "string";
            else if (propType == typeof(Guid)
                || propType == typeof(Guid?)
            ) return "string";
            else if (propType == typeof(bool)
                || propType == typeof(Boolean)
                || propType == typeof(Boolean?)
            ) return "boolean";
            else if (propType.Name.StartsWith("ICollection"))
                return "<object>[]";

            return "any";
        }
    }
}
