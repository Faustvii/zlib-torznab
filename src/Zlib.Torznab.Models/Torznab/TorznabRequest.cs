using System.Text.RegularExpressions;

namespace Zlib.Torznab.Models.Torznab;

public partial record TorznabRequest(
    string[] Categories,
    string? Query,
    string? Author,
    string? Title,
    string? Year,
    int Limit = 100,
    int Offset = 0
)
{
    public bool IsRSS => Query is null && Author is null && Title is null && Year is null;
    public string? CleanAuthor => ParseAuthor();

    [GeneratedRegex(@"\b(\w{1})\b", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 100)]
    private static partial Regex SingleCharWords();

    private string? ParseAuthor()
    {
        if (Author is null)
            return null;

        try
        {
            var matches = SingleCharWords().Matches(Author);
            var groups = new List<(string, string)>();
            for (var i = 0; i < matches.Count; i++)
            {
                if (i == 0)
                    continue;
                var prev = matches[i - 1];
                var current = matches[i];
                if (current.Index - prev.Index == 2)
                {
                    var values = new[] { prev.Value, current.Value };
                    groups.Add((string.Join(" ", values), string.Join("", values)));
                }
            }

            var result = Author;
            foreach (var group in groups)
            {
                result = result.Replace(group.Item1, group.Item2);
            }

            return result;
        }
        catch (System.Exception)
        {
            return Author;
        }
    }
};
