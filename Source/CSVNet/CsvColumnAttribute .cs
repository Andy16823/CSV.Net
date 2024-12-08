using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVNet
{
    /// <summary>
    /// Represents an attribute that specifies how a property should be mapped to a column in a CSV file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CsvColumnAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the column to which the property is mapped.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Gets the index of the column to which the property is mapped.
        /// </summary>
        public int? ColumnIndex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvColumnAttribute"/> class with the specified column name.
        /// </summary>
        /// <param name="columnName">The name of the column to which the property is mapped.</param>
        public CsvColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvColumnAttribute"/> class with the specified column index.
        /// </summary>
        /// <param name="columnIndex">The index of the column to which the property is mapped.</param>
        public CsvColumnAttribute(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }
    }
}
