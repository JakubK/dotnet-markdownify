using HtmlAgilityPack;
using static Dotnet.Markdownify.Consts.FormattingConsts;

namespace Dotnet.Markdownify;

public class HtmlCleaner
{
    public string CleanHtml(string html)
    {
        html = html.Replace("\r\n", NewLine);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        CleanNodes(doc.DocumentNode);

        return doc.DocumentNode.OuterHtml;
    }
    
    private static void CleanNodes(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            node.InnerHtml = node.InnerHtml.Trim();
        }

        foreach (var childNode in node.ChildNodes)
        {
            CleanNodes(childNode);
        }
    }
}