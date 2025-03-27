// See https://aka.ms/new-console-template for more information

using Dotnet.Markdownify;

var html = await File.ReadAllTextAsync("hmsw.html");
var converter = new MarkdownConverter();
var markdown = await converter.ConvertAsync(html);
// Console.WriteLine("BEGIN");
// Console.WriteLine(markdown);
// Console.WriteLine("END");

await File.WriteAllTextAsync("D:\\GitFork\\dotnet-markdownify\\Dotnet.Markdownify\\Dotnet.Markdownify.Console\\hmsw-new.md", markdown);
