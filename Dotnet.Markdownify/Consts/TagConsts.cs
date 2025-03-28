namespace Dotnet.Markdownify.Consts;

public static class TagConsts
{
    public static readonly string[] WhiteSpaceRemoveTags =
    [
        "p", "blockquote", "article",
        "div", "section",
        "ol", "ul", "li", "dl",
        "dl", "dt", "dd",
        "table", "thead", "tbody", "tfoot",
        "tr", "td", "th"
    ];

    public static readonly string[] TableCellTags =
    [
        "td", "th"
    ];

    public static readonly string[] ListTags =
    [
        "ol", "ul"
    ];

    public static readonly string[] PreformattedTags =
    [
        "pre", "code", "kbd", "samp"
    ];

    public static readonly string[] MarkdownIgnoreTags =
    [
        "script", "link", "path", "button", "meta", "svg", "title", "style", "head", "iframe", "font"
    ];
}