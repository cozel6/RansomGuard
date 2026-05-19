using System.Diagnostics.CodeAnalysis;

// CA1515: ASP.NET Core requires controllers and related types to be public for discovery and JSON serialization
[assembly: SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Required for ASP.NET Core controller discovery and JSON serialization")]

// CA1002 + CA2227: List<T> with setter required for JSON deserialization in DTO classes
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "List<T> required for JSON deserialization in DTO classes")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Setter required for JSON deserialization in DTO classes")]
