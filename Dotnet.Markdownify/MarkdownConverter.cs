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

        
        if (node?.Name == "pre" || node?.ParentNode?.Name == "pre") {

        }
        else
        {
            var updatedChildStrings = new List<string> { string.Empty };
            foreach (var childString in childStrings)
            {
                var match = RegexConsts.ReExtractNewlines.Match(childString);
                var leadingNl = match.Groups[1].Value;
                var content = match.Groups[2].Value;
                var trailingNl = match.Groups[3].Value;
                
                if (updatedChildStrings.Count > 0 && leadingNl != null)
                {
                    var prevTrailingNl = updatedChildStrings.Last();
                    updatedChildStrings.RemoveAt(updatedChildStrings.Count - 1);

                    var numNewlines = Math.Min(2, Math.Max(prevTrailingNl.Length, leadingNl.Length));
                    leadingNl = new string('\n', numNewlines);
                }

                updatedChildStrings.AddRange([leadingNl, content, trailingNl]);
            }
            
            childStrings = updatedChildStrings;
        }

        

        // Join all child text strings into a single string
        var text = string.Join(string.Empty, childStrings);
        
        // Apply this tag final conversion function
        var convertFn = GetConversionFunctionCached(node.Name);
        // Console.WriteLine(text);
        if (convertFn != null)
        {
            text = convertFn(node, text, parentTags);
            // Console.WriteLine("Converted " + node.Name + " into " + text);
        }

        return text;
    }

    private Func<HtmlNode, string, List<string>, string>?  GetConversionFunctionCached(string nodeName)
    {
        if (TagConsts.MarkdownIgnoreTags.Contains(nodeName))
        {
            return NoOpTransform;
        }
        //Console.WriteLine(nodeName);
        
        return NoOpTransform;
    }

    private string NoOpTransform(HtmlNode node, string text, List<string> parentTags)
    {
        return "" + text + "";
    }

    private async Task<string> ProcessElement(HtmlNode? node, List<string> parentTags)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            return ProcessText(node, parentTags);
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