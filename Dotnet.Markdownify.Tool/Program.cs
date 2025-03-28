// See https://aka.ms/new-console-template for more information

using Dotnet.Markdownify;

var inputHtmlPath = args[0];
var outputMdPath = args[1];

try
{
    var converter = new MarkdownConverter();
    var html = await File.ReadAllTextAsync(inputHtmlPath);
    var md = await converter.ConvertAsync(html);

    await File.WriteAllTextAsync(outputMdPath, md);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
