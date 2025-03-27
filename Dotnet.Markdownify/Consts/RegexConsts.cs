using System.Text.RegularExpressions;

namespace Dotnet.Markdownify.Consts;

public static class RegexConsts
{
    public static readonly Regex ReHtmlHeading = new Regex(@"h(\d+)");
    public static readonly Regex ReExtractNewlines  = new (@"^(\n*)((?:.*[^\n])?)(\n*)$", RegexOptions.Singleline);
}