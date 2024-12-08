using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSVNet
{
    /// <summary>
    /// A utility class for serializing and deserializing objects to and from CSV files.
    /// </summary>
    public class Converter
    {

        /// <summary>
        /// Serializes a list of objects into a CSV file.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the list.</typeparam>
        /// <param name="list">The list of objects to serialize.</param>
        /// <param name="file">The path to the output CSV file.</param>
        /// <param name="header">Specifies whether to include a header row in the CSV file. Default is true.</param>
        /// <param name="seperator">The character to use as a separator between columns. Default is ';'.</param>
        public static void Serialize<T>(List<T> list, String file, bool header = true, char seperator = ';', IFormatProvider formatProvider = null)
        {
            var format = formatProvider ?? CultureInfo.InvariantCulture;
            int columns = GetNumColums<T>();
            var properties = GetProperties<T>();
            String headerStr = "";
            String contentStr = "";

            // Header
            if (header)
            {
                StringBuilder headerBuilder = new StringBuilder();

                for (int i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var attribute = property.GetCustomAttribute<CsvColumnAttribute>();
                    headerBuilder.Append(attribute.ColumnName);

                    if (i < properties.Count - 1)
                    {
                        headerBuilder.Append(seperator);
                    }
                }
                headerStr = headerBuilder.ToString();
            }

            // Content
            StringBuilder contentBuilder = new StringBuilder();
            foreach (var item in list)
            {
                StringBuilder entryBuilder = new StringBuilder();
                for (int i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var value = properties[i].GetValue(item);

                    if (value is float || value is double || value is decimal)
                    {
                        entryBuilder.Append(Convert.ToDecimal(value, format).ToString("G", format));
                    }
                    else
                    {
                        entryBuilder.Append(value?.ToString());
                    }

                    if (i < properties.Count - 1)
                    {
                        entryBuilder.Append(seperator);
                    }
                }
                contentBuilder.AppendLine(entryBuilder.ToString());
            }
            contentStr = contentBuilder.ToString();

            StringBuilder fileBuilder = new StringBuilder();
            if (header)
            {
                fileBuilder.AppendLine(headerStr);
            }
            fileBuilder.AppendLine(contentStr);

            var fileContent = fileBuilder.ToString();
            File.WriteAllText(file, fileContent);
        }

        /// <summary>
        /// Deserializes a CSV file into a list of objects.
        /// </summary>
        /// <typeparam name="T">The type of objects to deserialize into.</typeparam>
        /// <param name="file">The path to the CSV file to deserialize.</param>
        /// <param name="header">Specifies whether the CSV file includes a header row. Default is true.</param>
        /// <param name="seperator">The character used as a separator between columns. Default is ';'.</param>
        /// <returns>A list of objects of type T populated with the data from the CSV file.</returns>
        public static List<T> Deserialize<T>(String file, bool header = true, char seperator = ';', IFormatProvider formatProvider = null) where T : class
        {
            var provider = formatProvider ?? CultureInfo.InvariantCulture;
            var output = new List<T>();
            var lines = System.IO.File.ReadAllLines(file);
            var properties = GetProperties<T>();
            var start = 0;
            String[] columnHeader = null;

            if (header)
            {
                start = 1;
                columnHeader = lines[0].Split(seperator);
            }

            for (int i = start; i < lines.Count() - 1; i++)
            {
                var line = lines[i];
                var columns = line.Split(seperator);

                Dictionary<String, String> columnTable = null;
                if (columnHeader != null)
                {
                    bool anyPropeterySet = false;
                    var columnsDictonary = GenerateColumnLookupTable(columnHeader, columns);
                    T instance = CreateInstance<T>();
                    foreach (var property in properties)
                    {
                        var attribute = property.GetCustomAttribute<CsvColumnAttribute>();
                        if(header && attribute.ColumnName != null && columnsDictonary.TryGetValue(attribute.ColumnName, out var columnValue))
                        {
                            SetProperty(instance, property, columnValue, provider);
                            anyPropeterySet = true;
                        }
                    }
                    if (anyPropeterySet)
                    {
                        output.Add(instance);
                    }
                }
                else
                {
                    bool anyPropeterySet = false;
                    var columnsDictonary = GenerateColumnLookupTable(columns);

                    T instance = CreateInstance<T>();
                    foreach (var property in properties)
                    {
                        var attribute = property.GetCustomAttribute<CsvColumnAttribute>();
                        if(!header && attribute.ColumnIndex.HasValue && columnsDictonary.TryGetValue(attribute.ColumnIndex.Value, out var columnValue))
                        {
                            SetProperty(instance, property, columnValue, provider);
                            anyPropeterySet = true;
                        }
                    }
                    if(anyPropeterySet)
                    {
                        output.Add(instance);
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Gets the number of columns defined in the CsvColumnAttribute for a type.
        /// </summary>
        /// <typeparam name="T">The type to inspect for column definitions.</typeparam>
        /// <returns>The number of columns defined by CsvColumnAttribute.</returns>
        internal static int GetNumColums<T>()
        {
            int num = 0;
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<CsvColumnAttribute>();
                if (attribute != null)
                {
                    num++;
                }
            }
            return num;
        }

        /// <summary>
        /// Gets the list of properties from a type that are decorated with the CsvColumnAttribute.
        /// </summary>
        /// <typeparam name="T">The type to inspect for properties.</typeparam>
        /// <returns>A list of properties with the CsvColumnAttribute.</returns>
        internal static List<PropertyInfo> GetProperties<T>()
        {
            var list = new List<PropertyInfo>();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<CsvColumnAttribute>();
                if(attribute != null)
                {
                    list.Add(property);
                }
            }
            return list;
        }

        /// <summary>
        /// Generates a dictionary mapping column headers to their corresponding content values.
        /// </summary>
        /// <param name="header">An array of column header names.</param>
        /// <param name="content">An array of content values.</param>
        /// <returns>A dictionary where keys are headers and values are the corresponding content.</returns>
        internal static Dictionary<String, String> GenerateColumnLookupTable(String[] header, String[] content)
        {
            var output = new Dictionary<String, String>();
            for (int i = 0; i < header.Length; i++)
            {
                var headerStr = header[i];
                var contentStr = content[i];

                output.Add(headerStr, contentStr);
            }
            return output;
        }

        /// <summary>
        /// Generates a dictionary mapping column indices to their corresponding content values.
        /// </summary>
        /// <param name="content">An array of content values.</param>
        /// <returns>A dictionary where keys are column indices and values are the corresponding content.</returns>
        internal static Dictionary<int, String> GenerateColumnLookupTable(String[] content)
        {
            var output = new Dictionary<int, String>();
            var columns = content.Length;
            for (int i = 0; i < columns; i++)
            {
                var contentStr = content[i];
                output[i] = contentStr;
            }
            return output;
        }

        /// <summary>
        /// Creates a new instance of a type using the most appropriate constructor.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <returns>A new instance of the specified type.</returns>
        private static T CreateInstance<T>() where T : class
        {
            var type = typeof(T);
            var constructor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);

            if (constructor != null)
            {
                return (T)constructor.Invoke(null);
            }

            constructor = type.GetConstructors()
                .OrderBy(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (constructor != null)
            {
                var parameters = constructor.GetParameters();
                var args = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = GetDefaultValue(parameters[i].ParameterType);
                }
                return (T)constructor.Invoke(args);
            }

            return null;
        }

        /// <summary>
        /// Gets the default value for a specified type.
        /// </summary>
        /// <param name="type">The type for which to get the default value.</param>
        /// <returns>The default value for the specified type.</returns>
        private static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Sets the value of a property on an object using reflection.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="instance">The object on which to set the property.</param>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The value to set on the property.</param>
        internal static void SetProperty<T>(T instance, PropertyInfo property, string value, IFormatProvider formatProvider)
        {
            try
            {
                if (property.CanWrite)
                {
                    if (property.PropertyType == typeof(float))
                    {
                        var convertedValue = float.Parse(value, formatProvider);
                        property.SetValue(instance, convertedValue);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        var convertedValue = double.Parse(value, formatProvider);
                        property.SetValue(instance, convertedValue);
                    }
                    else if (property.PropertyType == typeof(decimal))
                    {
                        var convertedValue = decimal.Parse(value, formatProvider);
                        property.SetValue(instance, convertedValue);
                    }
                    else
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(instance, convertedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while setting property '{property.Name}': {ex.Message}");
            }
        }
    }
}
