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
}