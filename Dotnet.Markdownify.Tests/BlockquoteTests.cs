namespace Dotnet.Markdownify.Tests;

public class BlockquoteTests
{
    [Fact]
    public async Task SimpleBlockquote_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/Blockquote.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/Blockquote.md");
        
        var md = sut.Convert(html);
        
        Assert.Equal(expectedMd, md);
    }
    
    [Fact]
    public async Task NestedBlockquote_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/NestedBlockquote.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/NestedBlockquote.md");
        
        var md = sut.Convert(html);
        
        Assert.Equal(expectedMd, md);
    }
}