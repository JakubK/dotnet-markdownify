using System.Net;
using System.Text;
using Dotnet.Markdownify.Consts;
using HtmlAgilityPack;
using static Dotnet.Markdownify.Consts.FormattingConsts;

namespace Dotnet.Markdownify;

public class MarkdownConverter
{
    public string Convert(string html)
    {
        var cleaner = new HtmlCleaner();
        var cleanHtml = cleaner.CleanHtml(html);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(cleanHtml);
        
        var md = ProcessTag(doc.DocumentNode, []);
        return md.Replace(WindowsNewLine, NewLine);
    }
    
    private static readonly Dictionary<string, Func<HtmlNode, string, List<string>, string>> ConversionFunctions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["div"] = ConvertDiv,
            ["article"] = ConvertDiv,
            ["section"] = ConvertDiv,
            ["p"] = ConvertDiv,
            ["li"] = ConvertLi,
            ["a"] = ConvertA,
            ["hr"] = ConvertHr,
            ["b"] = ConvertB,
            ["strong"] = ConvertB,
            ["i"] = ConvertI,
            ["pre"] = ConvertPre,
            ["code"] = ConvertCode,
            ["img"] = ConvertImg,
            ["table"] = ConvertTable,
            ["tr"] = ConvertTr,
            ["th"] = ConvertTd_Th,
            ["td"] = ConvertTd_Th,
            ["blockquote"] = ConvertBlockquote,
        };
    
    private string ProcessTag(HtmlNode node, List<string> parentTags)
    {
        if (TagConsts.MarkdownIgnoreTags.Contains(node.Name))
        {
            return string.Empty;
        }

        var childrenToConvert = node.ChildNodes;
        
        var parentTagsForChildren = new List<string>(parentTags);
        parentTagsForChildren.Add(node.Name);
        
        var childStrings = new List<string>();
        foreach (var child in childrenToConvert)
        {
            var converted = ProcessElement(child, parentTagsForChildren);
            childStrings.Add(converted);
        }

        if (parentTags.Contains("pre"))
        {
            childStrings = childStrings.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        else
        {
            childStrings = childStrings.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
        
        var text = string.Join(string.Empty, childStrings);
        
        var convertFn = GetConversionFunction(node.Name);
        var value = convertFn(node, text, parentTags);
        return value;
    }

    private Func<HtmlNode, string, List<string>, string>  GetConversionFunction(string nodeName)
    {
        if (TagConsts.MarkdownIgnoreTags.Contains(nodeName))
        {
            return NoOpTransform;
        }

        if (ConversionFunctions.TryGetValue(nodeName, out var conversionFunction))
        {
            return conversionFunction;
        }
        
        if (RegexConsts.ReHtmlHeading.IsMatch(nodeName))
        {
            return ConvertHeader;
        }
        
        return NoOpTransform;
    }

    private static string ConvertImg(HtmlNode node, string text, List<string> parentTags)
    {
        var alt = node.GetAttributeValue("alt", string.Empty); 
        var src = node.GetAttributeValue("src", string.Empty);

        return $"![{alt}]({src})";
    }
    
    private static string ConvertBlockquote(HtmlNode node, string text, List<string> parentTags)
    {
        var updatedText = RegexConsts.ReLineWithContent.Replace(text, match =>
        {
            var blockquoteContent = match.Groups[1].Value;
            if (blockquoteContent.Length > 0)
            {
                if (blockquoteContent.StartsWith(">"))
                {
                    return $">{blockquoteContent}";
                }
                return $"> {blockquoteContent}";
            }
            
            return ">";
        });
        updatedText = updatedText.Replace($">{NewLine}>", ">");
        if (updatedText.EndsWith(">"))
        {
            updatedText = updatedText.Substring(0, updatedText.Length - 1);
        }
        if (updatedText.EndsWith(NewLine))
        {
            updatedText = updatedText.Substring(0, updatedText.Length - 1);
        }
        return $"{NewLine}{updatedText}{NewLine}";
    }

    private static string ConvertTr(HtmlNode node, string text, List<string> parentTags)
    {
        var cells = node.SelectNodes(".//td|.//th").ToList();
        var isFirstRow = node.ParentNode.SelectNodes(".//tr")[0] == node;
        var isHeadRow = cells.All(cell => cell.Name == "th") || node.ParentNode.Name == "thead";
        var isHeadRowMissing = isFirstRow && !isHeadRow && node.ParentNode.Name == "tbody" && node.ParentNode.ParentNode.SelectNodes(".//thead") == null;
    
        var underline = new StringBuilder();
        var fullColspan = 0;
    
        foreach (var cell in cells)
        {
            fullColspan += 1;
        }
    
        if ((isFirstRow && isHeadRow) || isHeadRowMissing)
        {
            underline.AppendLine("|" + string.Join("|", Enumerable.Repeat("---", fullColspan)) + "|");
        }
    
        var innerText = string.Join("|", cells.Select(x => " " + x.InnerText.Trim() + " "));
        var value = "|" + innerText + "|" + "\n" + underline;
        return value;
    }

    private static string ConvertTd_Th(HtmlNode node, string text, List<string> parentTags)
    {
        var colspan = node.GetAttributeValue("colspan", 1);
        var colSpanSuffix = string.Concat(Enumerable.Repeat(" |", colspan));
        var value = ' ' + text.Trim().Replace(NewLine, " ") + colSpanSuffix;
        return value;
    }

    private static string ConvertTable(HtmlNode node, string text, List<string> parentTags)
    {
        return $"{NewLine}{text}";
    }
    
    private static string ConvertCode(HtmlNode node, string text, List<string> parentTags)
    {
        if (parentTags.Contains("pre"))
        {
            return text;
        }
        return $"`{text}`";
    }

    private static string ConvertB(HtmlNode node, string text, List<string> parentTags)
    {
        return $"**{text}**";
    }

    private static string ConvertI(HtmlNode node, string text, List<string> parentTags)
    {
        return $"_{text}_";
    }

    private static string ConvertLi(HtmlNode node, string text, List<string> parentTags)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Determine which character to use for bullet
        string bullet;
        var parent = node.ParentNode;
        if (parent != null && parent.Name == "ol")
        {
            var olStart = parent.GetAttributeValue("start", string.Empty);
            var olStartValue = string.IsNullOrEmpty(olStart) ? 1 : int.Parse(olStart);

            var count = 0;
            var prev = node.PreviousSibling;
            while (prev != null)
            {
                if (prev.NodeType == HtmlNodeType.Element)
                {
                    count++;
                }
                prev = prev.PreviousSibling;
            }

            bullet = $"{olStartValue + count}.";
        }
        else
        {
            bullet = "-";
        }

        bullet += " ";
        var bulletWidth = bullet.Length;
        if (node.ParentNode.Name == "ol")
        {
            bulletWidth--;
        }
        var bulletIndent = new string(' ', bulletWidth);

        text = RegexConsts.ReLineWithContent.Replace(text, match => match.Groups[1].Value.Length > 0 ? bulletIndent + match.Groups[1].Value : string.Empty);
        text = bullet + text.Substring(bulletWidth);
        
        if (node.ParentNode.FirstChild == node && parentTags.Contains("li"))
        {
            text = $"{NewLine}{text}{NewLine}";
        } 
        else if (node.ParentNode.LastChild == node && parentTags.Contains("li"))
        {
            text = $"{text}";
        }
        else
        {
            text = $"{text}{NewLine}";
        }
        
        return text;
    }

    private static string ConvertHr(HtmlNode node, string text, List<string> parentTags)
    {
        return $"{DoubleNewLine}---{DoubleNewLine}";
    }
    
    private static string ConvertA(HtmlNode node, string text, List<string> parentTags)
    {
        var href = node.GetAttributeValue("href", string.Empty);

        return $"[{text.Trim()}]({href.Trim()})";
    }

    private static string ConvertHeader(HtmlNode node, string text, List<string> parentTags)
    {
        var hLevel = int.Parse(RegexConsts.ReHtmlHeading.Match(node.Name).Groups[1].Value);
        var mdHeadingPrefix = new string('#', hLevel);
        return $"{NewLine}{mdHeadingPrefix} {text}{NewLine}";
    }

    private static string ConvertDiv(HtmlNode node, string text, List<string> parentTags)
    {
        text = text.Trim();
        return $"{NewLine}{text}{NewLine}";
    }

    private static string ConvertPre(HtmlNode node, string text, List<string> parentTags)
    {
        return $"{NewLine}```{NewLine}{text}{NewLine}```{NewLine}";
    }

    private static string NoOpTransform(HtmlNode node, string text, List<string> parentTags)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }
        return WebUtility.HtmlDecode(text);
    }

    private string ProcessElement(HtmlNode? node, List<string> parentTags)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = ProcessText(node, parentTags);
            return text;
        }
        
        return ProcessTag(node, parentTags);
    }

    private string ProcessText(HtmlNode node, List<string> parentTags)
    {
        if (string.IsNullOrWhiteSpace(node.InnerText) && !parentTags.Contains("pre"))
        {
            return WebUtility.HtmlDecode(node.InnerText.Trim());
        }
        
        return WebUtility.HtmlDecode(node.InnerText);
    }
}