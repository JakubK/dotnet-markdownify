namespace Dotnet.Markdownify.Consts;

public static class TagConsts
{
    public static string[] WhiteSpaceRemoveTags =
    [
        "p", "blockquote", "article",
        "div", "section",
        "ol", "ul", "li", "dl",
        "dl", "dt", "dd",
        "table", "thead", "tbody", "tfoot",
        "tr", "td", "th"
    ];

    public static string[] TableCellTags =
    [
        "td", "th"
    ];

    public static string[] ListTags =
    [
        "ol", "ul"
    ];

    public static string[] PreformattedTags =
    [
        "pre", "code", "kbd", "samp"
    ];

    public static string[] MarkdownIgnoreTags =
    [
        "script", "link", "path", "button", "meta", "img", "svg", "title", "style", "head", "code", "iframe"
    ];
}