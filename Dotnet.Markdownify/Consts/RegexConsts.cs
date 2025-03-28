using System.Text.RegularExpressions;

namespace Dotnet.Markdownify.Consts;

public static class RegexConsts
{
    public static readonly Regex ReHtmlHeading = new (@"h(\d+)");
    public static readonly Regex ReExtractNewlines  = new (@"^(\n*)((?:.*[^\n])?)(\n*)$", RegexOptions.Singleline);
    public static readonly Regex ReLineWithContent = new (@"^(.*)", RegexOptions.Multiline);

}