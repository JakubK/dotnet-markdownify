using Dotnet.Markdownify;

var html = await File.ReadAllTextAsync("input.html");
var converter = new MarkdownConverter();
var markdown = await converter.ConvertAsync(html);
await File.WriteAllTextAsync("output.md", markdown);
