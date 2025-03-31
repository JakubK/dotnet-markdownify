namespace Dotnet.Markdownify.Tests;

public class ListTests
{
    [Fact]
    public async Task SimpleUnorderedList_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/Ul.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/Ul.md");
        
        var md = sut.Convert(html);
        
        Assert.Equal(expectedMd, md);
    }

    [Fact]
    public async Task SimpleOrderedList_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/Ol.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/Ol.md");
        
        var md = sut.Convert(html);
        
        Assert.Equal(expectedMd, md);
    }
    
    [Fact]
    public async Task NestedOrderedList_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/NestedOl.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/NestedOl.md");
        
        var md = sut.Convert(html);
        
        Assert.Equal(expectedMd, md);
    }
    
    [Fact]
    public async Task NestedUnorderedList_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/NestedUl.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/NestedUl.md");
        
        var md = sut.Convert(html);
        
        Assert.Equal(expectedMd, md);
    }
}