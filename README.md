
# CSV.Net

CSV.Net is a flexible and easy-to-use library for serializing and deserializing objects to and from CSV files. It leverages reflection to map CSV columns to object properties and supports custom formatting for numeric types based on culture-specific settings.

## Features

- Serialize and deserialize objects to/from CSV.
- Support for custom column names using `CsvColumnAttribute`.
- Handles various numeric types (`float`, `double`, `decimal`) with custom formatting.
- Configurable delimiter for CSV files (default is `;`).
- Error handling during serialization and deserialization.
- Supports both CSV files with and without headers.
- Supports automatic mapping of CSV columns to object properties using reflection.

## Installation

To use CSV.Net in your project, you can either clone this repository or add it as a dependency in your `.csproj` file.

### Clone the repository:

```bash
git clone https://github.com/Andy16823/CSV.Net.git
```

### Add the library as a dependency (if you're using it as a package):

If you publish it as a NuGet package, you can install it via NuGet:

```bash
dotnet add package CSV.Net
```

## Usage

### Serializing Objects to CSV

```csharp
using CSVNet;

var personList = new List<Person>
{
    new Person { FirstName = "Markus", LastName = "Mustermann", Age = 30, Salary = 1200.50f },
    new Person { FirstName = "Nadine", LastName = "Musterfrau", Age = 32, Salary = 1500.75f }
};

string filePath = "output.csv";
Converter.Serialize(personList, filePath);
```

### Deserializing CSV to Objects

```csharp
using CSVNet;

string filePath = "input.csv";
var deserializedList = Converter.Deserialize<Person>(filePath);
```

### Defining Object Properties for CSV

You can use the `CsvColumnAttribute` to specify how object properties should be mapped to CSV columns:

```csharp
public class Person
{
    [CsvColumn("First Name")]
    public string FirstName { get; set; }

    [CsvColumn("Last Name")]
    public string LastName { get; set; }

    [CsvColumn("Age")]
    public int Age { get; set; }

    [CsvColumn("Salary")]
    public float Salary { get; set; }
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

1. Fork the repository.
2. Create your feature branch (`git checkout -b feature/my-new-feature`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature/my-new-feature`).
5. Open a pull request.

