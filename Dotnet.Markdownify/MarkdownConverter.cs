using System.Net;
using System.Text;
using Dotnet.Markdownify.Consts;
using HtmlAgilityPack;

namespace Dotnet.Markdownify;

public class MarkdownConverter
{
    public async Task<string> ConvertAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return await ProcessTag(doc.DocumentNode, []);
    }
    
    private async Task<string> ProcessTag(HtmlNode node, List<string> parentTags)
    {
        if (TagConsts.MarkdownIgnoreTags.Contains(node.Name))
        {
            return string.Empty;
        }
        
        var childrenToConvert = node.ChildNodes;
        
        var parentTagsForChildren = new List<string>(parentTags);
        parentTagsForChildren.Add(node.Name);

        // Add _inline pseudo tag for header or table cell
        if (RegexConsts.ReHtmlHeading.Matches(node.Name).Count > 0 || TagConsts.TableCellTags.Contains(node.Name))
        {
            parentTagsForChildren.Add("_inline");
        }

        if (TagConsts.PreformattedTags.Contains(node.Name))
        {
            parentTagsForChildren.Add("_noformat");
        }

        var childStrings = new List<string>();
        foreach (var child in childrenToConvert)
        {
            var converted = await ProcessElement(child, parentTagsForChildren);
            childStrings.Add(converted);
        }
        
        childStrings = childStrings.Where(x => !string.IsNullOrEmpty(x)).ToList();
        
        // Join all child text strings into a single string
        var text = string.Join(string.Empty, childStrings);
        
        // Apply this tag final conversion function
        var convertFn = GetConversionFunctionCached(node.Name);
        if (convertFn != null)
        {
            text = convertFn(node, text, parentTags);
        }

        return text;
    }

    private Func<HtmlNode, string, List<string>, string>?  GetConversionFunctionCached(string nodeName)
    {
        if (TagConsts.MarkdownIgnoreTags.Contains(nodeName))
        {
            return NoOpTransform;
        }

        if (nodeName == "div" || nodeName == "article" || nodeName == "section" || nodeName == "p")
        {
            return ConvertDiv;
        }

        if (nodeName == "ul" || nodeName == "ol")
        {
            return ConvertUl;
        }

        if (nodeName == "li")
        {
            return ConvertLi;
        }

        if (nodeName == "a")
        {
            return ConvertA;
        }

        if (nodeName == "hr")
        {
            return ConvertHr;
        }

        if (nodeName == "b" || nodeName == "strong")
        {
            return ConvertB;
        }

        if (nodeName == "i")
        {
            return ConvertI;
        }

        if (nodeName == "pre")
        {
            return ConvertPre;
        }

        if (nodeName == "code")
        {
            return ConvertCode;
        }

        if (nodeName == "img")
        {
            return ConvertImg;
        }

        if (nodeName == "table")
        {
            return ConvertTable;
        }

        if (nodeName == "tr")
        {
            return ConvertTr;
        }

        if (nodeName == "th" || nodeName == "td")
        {
            return ConvertTd_Th;
        }
        if (RegexConsts.ReHtmlHeading.IsMatch(nodeName))
        {
            return ConvertHeader;
        }

        
        return NoOpTransform;
    }

    private string ConvertImg(HtmlNode node, string text, List<string> parentTags)
    {
        var alt = node.GetAttributeValue("alt", string.Empty); 
        var src = node.GetAttributeValue("src", string.Empty);

        return $"![{alt}]({src})";
    }

    private string ConvertTr(HtmlNode node, string text, List<string> parentTags)
    {
        var tableInferHeader = false;
        var cells = node.SelectNodes(".//td|.//th")?.ToList() ?? new List<HtmlNode>();
        var isFirstRow = node.PreviousSibling == null;
        var isHeadRow = cells.All(cell => cell.Name == "th") ||
                         (node.ParentNode.Name == "thead" && node.ParentNode.SelectNodes(".//tr")?.Count == 1);
        var isHeadRowMissing = (isFirstRow && node.ParentNode.Name != "tbody") ||
                                (isFirstRow && node.ParentNode.Name == "tbody" &&
                                 node.ParentNode.ParentNode.SelectNodes(".//thead")?.Count < 1);

        var overline = new StringBuilder();
        var underline = new StringBuilder();
        var fullColspan = 0;

        foreach (var cell in cells)
        {
            if (cell.Attributes["colspan"] != null && int.TryParse(cell.Attributes["colspan"].Value, out int colspan))
            {
                fullColspan += colspan;
            }
            else
            {
                fullColspan += 1;
            }
        }

        if ((isHeadRow || (isHeadRowMissing && tableInferHeader)) && isFirstRow)
        {
            underline.AppendLine("| " + string.Join(" | ", Enumerable.Repeat("---", fullColspan)) + " |");
        }
        else if ((isHeadRowMissing && !tableInferHeader) ||
                 (isFirstRow && (node.ParentNode.Name == "table" ||
                                 (node.ParentNode.Name == "tbody" && node.ParentNode.PreviousSibling == null))))
        {
            overline.AppendLine("| " + string.Join(" | ", Enumerable.Repeat("", fullColspan)) + " |");
            overline.AppendLine("| " + string.Join(" | ", Enumerable.Repeat("---", fullColspan)) + " |");
        }

        return overline + "| " + node.InnerText.Trim() + "\n" + underline;
    }

    private string ConvertTd_Th(HtmlNode node, string text, List<string> parentTags)
    {
        var colspan = node.GetAttributeValue("colspan", 1);
        var colSpanSuffix = string.Concat(Enumerable.Repeat(" |", colspan));
        return ' ' + text.Trim().Replace("\n", " ") + colSpanSuffix;
    }

    private string ConvertTable(HtmlNode node, string text, List<string> parentTags)
    {
        return $"\n\n{text}\n\n";
    }
    
    private string ConvertCode(HtmlNode node, string text, List<string> parentTags)
    {
        if (parentTags.Contains("pre"))
        {
            return text;
        }
        return $"`{text}`";
    }

    private string ConvertB(HtmlNode node, string text, List<string> parentTags)
    {
        return $"**{text}**";
    }

    private string ConvertI(HtmlNode node, string text, List<string> parentTags)
    {
        return $"_{text}_";
    }

    private string ConvertLi(HtmlNode node, string text, List<string> parentTags)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "\n";
        }

        // Determine which character to use for bullet
        var bullet = string.Empty;
        var parent = node.ParentNode;
        if (parent != null && parent.Name == "ol")
        {
            var olStart = parent.GetAttributeValue("start", string.Empty);
            var olStartValue = string.IsNullOrEmpty(olStart) ? 1 : int.Parse(olStart);

            var count = 0;
            var prev = node.PreviousSibling;
            while (prev != null)
            {
                if (prev.Name != "#text")
                    count++;
                prev = prev.PreviousSibling;
            }

            bullet = $"{olStartValue + count}.";
        }
        else
        {
            var depth = -1;
            var el = node;
            while (el != null)
            {
                if (el.Name == "ul")
                {
                    depth++;
                }

                el = el.ParentNode;
            }

            var bullets = "*+-";
            bullet = bullets[depth % bullets.Length].ToString();
        }

        bullet += " ";
        var bulletWidth = bullet.Length;
        var bulletIndent = new string(' ', bulletWidth);

        text = RegexConsts.ReLineWithContent.Replace(text, match => match.Groups[1].Value.Length > 0 ? bulletIndent + match.Groups[1].Value : string.Empty);
        text = bullet + text.Substring(bulletWidth);
        return $"{text}\n";
    }

    private string ConvertHr(HtmlNode node, string text, List<string> parentTags)
    {
        return "\n\n---\n\n";
    }

    private string ConvertUl(HtmlNode node, string text, List<string> parentTags)
    {
        var nextSibling = node.NextSibling;
        if (parentTags.Contains("li"))
        {
            // Remove trailing newline if in nested list
            return text + text.TrimEnd();
        }
        if (nextSibling != null && !TagConsts.ListTags.Contains(nextSibling.Name))
        {
            return $"\n\n{text}\n";
        }
        return $"\n\n{text}";
    }
    
    private string ConvertA(HtmlNode node, string text, List<string> parentTags)
    {
        if (parentTags.Contains("_noformat"))
        {
            return text;
        }
        
        var href = node.GetAttributeValue("href", string.Empty);

        return $"[{text.Trim()}]({href.Trim()})";
    }

    private string ConvertHeader(HtmlNode node, string text, List<string> parentTags)
    {
        if (parentTags.Contains("_inline"))
        {
            return text;
        }

        var hLevel = int.Parse(RegexConsts.ReHtmlHeading.Match(node.Name).Groups[1].Value);
        var mdHeadingPrefix = new string('#', hLevel);
        return $"\n{mdHeadingPrefix} {text}\n";
    }

    private string ConvertDiv(HtmlNode node, string text, List<string> parentTags)
    {
        if (parentTags.Contains("_inline"))
        {
            return " " + text.Trim() + " ";
        }
        
        text = text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return $"\n{text}\n";
    }

    private string ConvertPre(HtmlNode node, string text, List<string> parentTags)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var x = $"\n\n```\n{text}\n```\n\n";
        return x;
    }

    private string NoOpTransform(HtmlNode node, string text, List<string> parentTags)
    {
        Console.WriteLine("Missing handler for " + node.Name);
        return WebUtility.HtmlDecode(text);
    }

    private async Task<string> ProcessElement(HtmlNode? node, List<string> parentTags)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = ProcessText(node, parentTags);
            return text;
        }
        
        return await ProcessTag(node, parentTags);
    }

    private string ProcessText(HtmlNode node, List<string> parentTags)
    {
        return node.InnerText;
    }
}