# Dotnet.Markdownify

Simple dotnet library for converting HTML into Markdown.

## Installation

To use `dotnet-markdownify` in your project, you can:

- simply clone this repo and add reference to `Dotnet.Markdownify.csproj` or
- Install via nuget (TODO)

## Usage

```csharp
using Dotnet.Markdownify;

var html = await File.ReadAllTextAsync("input.html");
var converter = new MarkdownConverter();
var markdown = converter.Convert(html);
await File.WriteAllTextAsync("output.md", markdown);
```

# Dotnet.Markdownify.Tool

This repository comes with dotnet tool for conversion of HTML into Markdown via CLI.

## Installation

There are 2 ways to consume the dotnet tool:

### From repository
1. Clone the repository
2. Build Dotnet.Markdownify.Tool
3. Locate *.nupkg file in repo directory

```bash
dotnet tool install -g dotnet.markdownify.tool --add-source .
```

### From nuget (TODO)

## Usage

```bash
markdownify <Path to existing html> <Path to output markdown file>
```

### Example
```bash
markdownify input.html output.html
```

# Acknowledgements

This repository was inspired by [python-markdownify](https://github.com/matthewwithanm/python-markdownify) 
and initially the C# code was meant to be a port from python-markdownify feature subset. 
