using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Containers
{
    public class EnumEx
    {
        
        public static string GetStringFromArray(IEnumerable<Object> list)
        {
            var fields = new StringBuilder();
            if (list != null)
            {
                foreach (var o in list)
                {
                    if (o != null)
                    {
                        fields.Append(o).Append("\n");
                    }
                }
            }

            return fields.ToString();
        }

        public static string GetStringFromArray(IEnumerable<Object> list, bool checkList)
        {
            var fields = new StringBuilder();
            if (list != null)
            {
                int i = 0;
                foreach (var o in list)
                {
                    if (checkList && i > 4)
                    {
                        fields.Append("[SKIP]");
                        break;
                    }
                    if (o != null)
                    {
                        fields.Append(o).Append("\n");
                    }
                    i++;
                }
            }

            return fields.ToString();
        }

        public static string GetStringFromArray(IEnumerable<KeyValuePair<string, string>> list)
        {
            var fields = new StringBuilder();
            if (list != null)
            {
                foreach (var o in list)
                {
                    fields.Append(o.Key).Append(": ").Append(o.Value).Append("\n");
                }
            }

            return fields.ToString();
        }

        public static string GetStringFromArray(NameValueCollection list)
        {
            var fields = new StringBuilder();
            if (list != null)
            {
                foreach (string key in list)
                {
                    if (!String.IsNullOrEmpty(key))
                    {
                        fields.Append(String.Format("{0}: {1}", key, list[key])).Append("\n");
                    }
                }
            }

            return fields.ToString();
        }

        public static string GetStringFromArray(IEnumerable<int> list)
        {
            var fields = new StringBuilder();
            if (list != null)
            {
                foreach (var o in list)
                {
                    fields.Append(o).Append("\n");
                }
            }

            return fields.ToString();
        }

        public static string GetStringFromArray(IEnumerable<long> list)
        {
            var fields = new StringBuilder();
            if (list != null)
            {
                foreach (var o in list)
                {
                    fields.Append(o).Append("\n");
                }
            }

            return fields.ToString();
        }

        public static string GetStringFromArray(IEnumerable<float> list)
        {
            var fields = new StringBuilder();
            if (list != null)
            {
                foreach (var o in list)
                {
                    fields.Append(o).Append("\n");
                }
            }

            return fields.ToString();
        }

        public static string GetDescription(Enum value)
        {
            try
            {
                if (value == null)
                {
                    return string.Empty;
                }

                FieldInfo field = value.GetType().GetField(value.ToString());

                if (field == null)
                {
                    return string.Empty;
                }
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

                return attribute == null ? value.ToString() : attribute.Description;
            }
            catch (Exception)
            {
            }

            return String.Empty;
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                                                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
        }

        public static T[] ConvertToArray<T>(IEnumerable<T> values)
        {
            if (values == null)
            {
                return null;
            }

            var result = new List<T>();

            foreach (var value in values)
            {
                result.Add(value);
            }

            return result.ToArray();
        }

        public static String ToStringArray<T>(IEnumerable<T> array)
        {
            var result = new StringBuilder();
            Type typeParameterType = typeof(T);
            result.Append(String.Format("{0}: \n", typeParameterType.Name));

            foreach (var item in array)
            {
                result.Append(item).Append("\n");
            }

            return result.ToString();
        }


    }
}
