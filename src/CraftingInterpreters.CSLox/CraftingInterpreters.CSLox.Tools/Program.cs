﻿// See https://aka.ms/new-console-template for more information

using System.Xml.Linq;

Console.WriteLine("Hello, World!");
Console.WriteLine("Enter the ouput file name: ");
var outputDirectory = Console.ReadLine();

DefineAst(outputDirectory, "LoxExpression", new()
{
	"Binary   : LoxExpression left, Token @operator, LoxExpression right",
	"Grouping : LoxExpression expression",
	"Literal  : object value",
	"Unary    : Token @operator, LoxExpression right"
});

Console.WriteLine($"Done - File has been generated at {outputDirectory}.");

static void DefineAst(string outputDir, string baseName, List<string> types)
{
	var path = $"{outputDir}/{baseName}.cs";
	using var writer = new StreamWriter(path);
	writer.WriteLine("// Path: CraftingInterpreters.CSLox.Core/LoxExpression.cs");
	writer.WriteLine("// This file was generated by the tool at CraftingInterpreters.CSLox.Tools");
	writer.WriteLine("// Do not edit this file directly.");
	writer.WriteLine();
	writer.WriteLine("using System;");
	writer.WriteLine("using System.Collections.Generic;");
	writer.WriteLine("using System.Linq;");
	writer.WriteLine("using System.Text;");
	writer.WriteLine("using System.Threading.Tasks;");
	writer.WriteLine();
	writer.WriteLine("namespace CraftingInterpreters.CSLox.Core;");
	writer.WriteLine();
	writer.WriteLine($"public abstract class {baseName}");
	writer.WriteLine("{");
	// Define the abstract accept method for the visitor pattern
	writer.WriteLine($"\tpublic abstract T Accept<T>(IVisitor<T> visitor);");

	writer.WriteLine("}");

	writer.WriteLine();
	// Define the visitor interface
	DefineVisitor(writer, baseName, types);

	foreach (var type in types)
	{
		var className = type.Split(":")[0].Trim();
		var fields = type.Split(":")[1].Trim();
		DefineType(writer, baseName, className, fields);
	}

	writer.WriteLine();

	writer.Dispose();

}

static void DefineType(StreamWriter writer, string baseName, string className, string fields)
{
	writer.WriteLine($"public class {className}LoxExpression : {baseName}");
	writer.WriteLine("{");
	// Constructor
	writer.WriteLine($"\tpublic {className}{baseName}({fields})");
	writer.WriteLine("\t{");

	// Define the fields
	var fieldList = fields.Split(", ");
	foreach (var field in fieldList)
	{
		var name = field.Split(" ")[1];
		var propertyName = GetPropertyName(name);
		writer.WriteLine($"\t\tthis.{propertyName} = {name};");
	}

	writer.WriteLine("\t}");

	// Define the visitor pattern Accept method
	writer.WriteLine();
	writer.WriteLine($"\tpublic override T Accept<T>(IVisitor<T> visitor)");
	writer.WriteLine("\t{");
	writer.WriteLine($"\t\treturn visitor.Visit{GetPropertyName(className)}{baseName}(this);");
	writer.WriteLine("\t}");

	writer.WriteLine();

	// Define the properties
	foreach (var field in fieldList)
	{
		var type = field.Split(" ")[0];
		var name = field.Split(" ")[1];
		name = GetPropertyName(name);
		writer.WriteLine($"\tpublic {type} {name} {{ get; set; }}");
	}

	writer.WriteLine();
	writer.WriteLine("}");
	writer.WriteLine();
}

/// <summary>
/// Define the interface related to the visitor pattern
/// </summary>
static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
{
	writer.WriteLine($"public interface IVisitor<T>");
	writer.WriteLine("{");
	foreach (var item in types)
	{
		var typeName = item.Split(":")[0].Trim();
		var methodLine = $"\tT Visit{GetPropertyName(typeName)}{baseName}({typeName}{baseName} loxExpression);";
		writer.WriteLine(methodLine);
	}
	writer.WriteLine("}");
	writer.WriteLine();
}

/// <summary>
/// Capitalize the first letter in a name
/// </summary>
static string GetPropertyName(string field)
{
	if (field.StartsWith("@"))
		field = field.Substring(1);
	return field.Substring(0, 1).ToUpper() + field.Substring(1);
}