using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

Console.Write("Looking for svg files...");

string folderPath = @"..\..\..\..\..\..\MaterialDesign\svg\"; //MaterialDesignIcons folder

if (!Directory.Exists(folderPath))
{
    Console.WriteLine($"Path does not exist: {folderPath}");
    return;
}

var files = Directory.GetFiles(folderPath, "*.svg", SearchOption.AllDirectories)
    .OrderBy(name => name)
    .ToList();

if (files.Count == 0)
{
    Console.WriteLine("No svg files found.");
    return;
}
else
{
    Console.WriteLine($" {files.Count} svg files found.");
}

string template = @"namespace Chnuschti;

public enum IconKind : int
{
{enums}
}

public static class Icons
{
    public static Dictionary<IconKind, string> IconPaths = new()
    {
{ICONS}
    };
}
";

//Process each file
var enums = new StringBuilder();
var icons = new StringBuilder();
var pathRegex = new Regex("<path d=\"(.+?)\"");
Console.Write($"Processing files...");
foreach (var file in files)
{
    string fileName = Path.GetFileNameWithoutExtension(file);
    string iconKind = Regex.Replace(fileName, @"[^a-zA-Z0-9]", " ");
    //Capitalize the first letter of each word
    iconKind = string.Join("", iconKind.Split(' ').Select(word => char.ToUpper(word[0]) + word.Substring(1)));

    string svgContent = File.ReadAllText(file)
        .Replace("\r\n", " ")
        .Replace("\n", " ");

    string pathData = pathRegex.Match(svgContent).Groups[1].Value;

    enums.AppendLine($"    {iconKind},");
    icons.AppendLine($"        {{ IconKind.{iconKind}, \"{pathData}\" }},");
}
Console.WriteLine($"Done!");

string output = template.Replace("{enums}", enums.ToString().TrimEnd(','))
                        .Replace("{ICONS}", icons.ToString().TrimEnd(','));

var iconsFilePath = @"..\..\..\..\..\Chnuschti\Icons.cs";
var p = Path.GetFullPath(iconsFilePath);
if (File.Exists(iconsFilePath))
{
    Console.WriteLine($"Overwriting existing file: {iconsFilePath}");
}
else
{
    Console.WriteLine($"Creating new file: {iconsFilePath}");
}
File.WriteAllText(iconsFilePath, output);
Console.ReadLine();