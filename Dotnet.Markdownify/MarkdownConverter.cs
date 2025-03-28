using System.Net;
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
        
        var childrenToConvert = node.ChildNodes.Where(x => !CanIgnore(x));
        
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
        if (RegexConsts.ReHtmlHeading.IsMatch(nodeName))
        {
            return ConvertHeader;
        }

        
        return NoOpTransform;
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
        parentTags = parentTags.Count > 0 ? parentTags : new List<string>();

        var text = node.InnerText;
        if (!parentTags.Contains("pre"))
        {
            // TODO: Normalize whitespace
        }
        
        // Escape special characters if not inside preformatted or code element
        if (!parentTags.Contains("_noformat"))
        {
            text = Escape(text);
        }
        
        // Remove leading/trailing whitespace
        if (ShouldRemoveWhitespaceOutside(node.PreviousSibling) ||
            ShouldRemoveWhitespaceInside(node.ParentNode) && node.PreviousSibling == null)
        {
            text = text.TrimStart();
        }

        if (ShouldRemoveWhitespaceOutside(node.NextSibling) ||
            ShouldRemoveWhitespaceInside(node.ParentNode) && node.NextSibling == null)
        {
            text = text.TrimEnd();
        }
        
        return text;
    }

    private string Escape(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text;
    }

    // Return to remove whitespace immediately inside a block-level element.
    private bool ShouldRemoveWhitespaceInside(HtmlNode? node)
    {
        if (node is null)
        {
            return false;
        }

        if (RegexConsts.ReHtmlHeading.Matches(node.Name).Count > 0)
        {
            return true;
        }

        return TagConsts.WhiteSpaceRemoveTags.Contains(node.Name);
    }

    private bool CanIgnore(HtmlNode? node)
    {
        var shouldRemoveInside = ShouldRemoveWhitespaceInside(node);
        if (node == null)
        {
            return true;
        }
        if (node.NodeType == HtmlNodeType.Element)
        {
            // Tags (elements) are always processed
            return false;
        }
        if (node.NodeType == HtmlNodeType.Comment || node.NodeType == HtmlNodeType.Document)
        {
            // Comment and Doctype elements are always ignored
            return true;
        }
        if (node.NodeType == HtmlNodeType.Text)
        {
            string text = node.InnerText.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                // Non-whitespace text nodes are always processed
                return false;
            }

            // Handling whitespace based on surrounding nodes
            if (shouldRemoveInside)
            {
                // Inside block elements, ignore adjacent whitespace elements
                return true;
            }

            if (ShouldRemoveWhitespaceOutside(node.PreviousSibling) || ShouldRemoveWhitespaceOutside(node.NextSibling))
            {
                // Outside block elements, ignore adjacent whitespace elements
                return true;
            }

            return false;
        }
        
        throw new ArgumentException($"Unexpected element type: {node.NodeType}");
    }

    // Return to remove whitespace immediately outside a block-level element.
    private bool ShouldRemoveWhitespaceOutside(HtmlNode? node)
    {
        if (node != null && node.Name == "pre")
            return true;
        return ShouldRemoveWhitespaceInside(node);
    }
}