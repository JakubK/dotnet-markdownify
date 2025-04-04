﻿namespace Dotnet.Markdownify.Tests;

public class TableTests
{
    [Fact]
    public async Task Table_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/Table.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/Table.md");
        
        var md = sut.Convert(html);
        
        Assert.Equal(expectedMd, md);
    }
    
    [Fact]
    public async Task TableWithoutHead_WorksAsExpected()
    {
        var sut = new MarkdownConverter();
        var html = await File.ReadAllTextAsync("TestData/HeadlessTable.html");
        var expectedMd = await File.ReadAllTextAsync("TestData/HeadlessTable.md");
        
        var md = sut.Convert(html);
        
        Assert.Equal(expectedMd, md);
    }
}