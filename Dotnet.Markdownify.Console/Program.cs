using Dotnet.Markdownify;

var html = await File.ReadAllTextAsync("input.html");
var converter = new MarkdownConverter();
var markdown = converter.Convert(html);
await File.WriteAllTextAsync("output.md", markdown);
