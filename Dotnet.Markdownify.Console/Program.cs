using Dotnet.Markdownify;

var html = await File.ReadAllTextAsync("hmsw.html");
var converter = new MarkdownConverter();
var markdown = await converter.ConvertAsync(html);
await File.WriteAllTextAsync("hmsw-new.md", markdown);
