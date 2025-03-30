using static Dotnet.Markdownify.Consts.FormattingConsts;

namespace Dotnet.Markdownify.Tests;

public class BasicConversionTests
{
    [Fact]
    public async Task TestSpan()
    {
        var sut = new MarkdownConverter();
        var md = await sut.ConvertAsync("<span>Hello</span>");
        Assert.Equal("Hello", md);
    }
    
    [Fact]
    public async Task TestDiv()
    {
        var sut = new MarkdownConverter();
        var md = await sut.ConvertAsync("<div>Hello</div>");
        Assert.Equal($"{NewLine}Hello{NewLine}", md);
    }

    [Fact]
    public async Task TestHn()
    {
        var sut = new MarkdownConverter();
        var md = await sut.ConvertAsync("<h1>Hello</h1>");
        Assert.Equal($"{NewLine}# Hello{NewLine}", md);
    }

    [Fact]
    public async Task TestLink()
    {
        var sut = new MarkdownConverter();
        var md = await sut.ConvertAsync("<a href=\"http://example.com\">Hello</a>");
        Assert.Equal($"[Hello](http://example.com)", md);
    }

    [Fact]
    public async Task TestBold()
    {
        var sut = new MarkdownConverter();
        
        var mdB = await sut.ConvertAsync("<b>Hello</b>");
        var mdStrong = await sut.ConvertAsync("<strong>Hello</strong>");
        
        Assert.Equal(mdB, mdStrong);
        Assert.Equal($"**Hello**", mdStrong);
    }
    
    [Fact]
    public async Task TestItalic()
    {
        var sut = new MarkdownConverter();
        
        var md = await sut.ConvertAsync("<i>Hello</i>");
        
        Assert.Equal($"_Hello_", md);
    }
    
    [Fact]
    public async Task TestCode()
    {
        var sut = new MarkdownConverter();
        
        var md = await sut.ConvertAsync("<code>Hello</code>");
        
        Assert.Equal($"`Hello`", md);
    }
    
    [Fact]
    public async Task TestHr()
    {
        var sut = new MarkdownConverter();
        
        var md = await sut.ConvertAsync("<hr>");
        
        Assert.Equal($"{DoubleNewLine}---{DoubleNewLine}", md);
    }

    [Fact]
    public async Task TestImg()
    {
        var sut = new MarkdownConverter();
        
        var md = await sut.ConvertAsync("<img src=\"http://example.com/test.png\" alt=\"Image title\"/>");
        
        Assert.Equal($"![Image title](http://example.com/test.png)", md);
    }
}