using System;
using System.Linq;
using System.Collections.Generic;
using DORA.Access.Models;
using Newtonsoft.Json;

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

    public class EntityTypeScript
    {
        private Dictionary<string,string> columns { get; set; }
        private Type entityType { get; set; }

        public EntityTypeScript(string typename)
        {
            this.entityType = Type.GetType(typename);
            this.buildColumns();
        }
        public EntityTypeScript(Type entityType)
        {
            this.entityType = entityType;
            this.buildColumns();
        }

        private void buildColumns()
        {
            this.columns = new Dictionary<string, string>();

            foreach (System.Reflection.PropertyInfo prop in this.entityType.GetProperties())
            {
                // try to find the JsonProperty attr
                string jsonAttr = null;
                bool includeColumn = true;

                foreach(System.Attribute attr in prop.GetCustomAttributes(true))
                {
                    if (attr is JsonPropertyAttribute)
                        jsonAttr = (attr as Newtonsoft.Json.JsonPropertyAttribute).PropertyName;

                    if (attr is JsonIgnoreAttribute)
                        includeColumn = false;
                }

                if (jsonAttr == "id" || prop.Name.ToLower() == "id")
                    includeColumn = false;

                if (includeColumn)
                    this.columns.Add(jsonAttr != null ? jsonAttr : prop.Name, this.mapJSTypeName(prop.PropertyType));
            }
        }

        public string TypeDefinition
        {
            get
            {
                string result = "export type I" + this.entityType.Name + " = IEntity & {";
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
            ) return "Date";
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
            {
                string collectionType = GetCollectionTypeName(propType);
                return (collectionType != "object" ? "I" + collectionType : collectionType) + "[]";
            }

            return "any";
        }

        private static string GetCollectionTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                Type[] types = type.GetGenericArguments();
                if (types.Length == 1)
                {
                    return types[0].Name;
                }
                else
                {
                    // Could be null if implements two IEnumerable
                    foreach(Type itype in type.GetInterfaces())
                    {
                        if (itype.IsGenericType && itype.GetGenericTypeDefinition() == typeof(ICollection<>))
                        {
                            return itype.GetGenericArguments()[0].Name;
                        }
                    }
                }
            }
            else if (type.IsArray)
            {
                return type.GetElementType().Name;
            }
            // TODO: Who knows, but its probably not suitable to render in a table
            return "object";
        }
    }
}
