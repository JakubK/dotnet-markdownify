namespace Dotnet.Markdownify.Tests;

public class WhitespaceTests
{
    [Fact]
    public async Task PreCode_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/PreCode.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/PreCode.md");
        
        var md = await sut.ConvertAsync(html);
        
        Assert.Equal(expectedMd, md);
    }
}