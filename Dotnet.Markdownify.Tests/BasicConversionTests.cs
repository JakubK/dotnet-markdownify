using static Dotnet.Markdownify.Consts.FormattingConsts;

namespace Dotnet.Markdownify.Tests;

public class BasicConversionTests
{
    [Fact]
    public void TestSpan()
    {
        var sut = new MarkdownConverter();
        var md = sut.Convert("<span>Hello</span>");
        Assert.Equal("Hello", md);
    }
    
    [Fact]
    public void TestDiv()
    {
        var sut = new MarkdownConverter();
        var md = sut.Convert("<div>Hello</div>");
        Assert.Equal($"{NewLine}Hello{NewLine}", md);
    }

    [Fact]
    public void TestHn()
    {
        var sut = new MarkdownConverter();
        var md = sut.Convert("<h1>Hello</h1>");
        Assert.Equal($"{NewLine}# Hello{NewLine}", md);
    }

    [Fact]
    public void TestLink()
    {
        var sut = new MarkdownConverter();
        var md = sut.Convert("<a href=\"http://example.com\">Hello</a>");
        Assert.Equal($"[Hello](http://example.com)", md);
    }

    [Fact]
    public void TestBold()
    {
        var sut = new MarkdownConverter();
        
        var mdB = sut.Convert("<b>Hello</b>");
        var mdStrong = sut.Convert("<strong>Hello</strong>");
        
        Assert.Equal(mdB, mdStrong);
        Assert.Equal($"**Hello**", mdStrong);
    }
    
    [Fact]
    public void TestItalic()
    {
        var sut = new MarkdownConverter();
        
        var md = sut.Convert("<i>Hello</i>");
        
        Assert.Equal($"_Hello_", md);
    }
    
    [Fact]
    public void TestCode()
    {
        var sut = new MarkdownConverter();
        
        var md = sut.Convert("<code>Hello</code>");
        
        Assert.Equal($"`Hello`", md);
    }
    
    [Fact]
    public void TestHr()
    {
        var sut = new MarkdownConverter();
        
        var md = sut.Convert("<hr>");
        
        Assert.Equal($"{DoubleNewLine}---{DoubleNewLine}", md);
    }

    [Fact]
    public void TestImg()
    {
        var sut = new MarkdownConverter();
        
        var md = sut.Convert("<img src=\"http://example.com/test.png\" alt=\"Image title\"/>");
        
        Assert.Equal($"![Image title](http://example.com/test.png)", md);
    }
}