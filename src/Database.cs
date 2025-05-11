using System.Collections.Immutable;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace BoosterrCLI;

public static class Database
{
    private const string FilePath = "boosterr.xlsx";

    private const string TermWorksheet = "Term";

    // Column headers
    private const string SyncHeader = "Sync";
    private const string NameHeader = "Name";
    private const string PrettyNameHeader = "Pretty Name";
    private const string RegexHeader = "Regex";
    private const string TestsMustMatchHeader = "Must Match";
    private const string TestsMustNotMatchHeader = "Must Not Match";
    private const string WebsiteHeader = "Website";

    private static readonly string[] TermWorksheetHeaders =
    [
        SyncHeader, NameHeader, PrettyNameHeader, RegexHeader, TestsMustMatchHeader,
        TestsMustNotMatchHeader, WebsiteHeader
    ];

    public static ImmutableList<Term> GetTerms()
    {
        List<Term> terms = LoadTerms();
        ValidateTerms(terms);
        return [..terms];
    }

    private static List<Term> LoadTerms()
    {
        using XLWorkbook workbook = new(FilePath);
        IXLWorksheet? worksheet = workbook.Worksheet(TermWorksheet);
        Dictionary<string, int> columnHeaderToIdx = ParseHeaders(worksheet.FirstRow());
        List<Term> terms = [];
        foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
        {
            string name = row.Cell(columnHeaderToIdx[NameHeader]).GetString();
            string prettyName = row.Cell(columnHeaderToIdx[PrettyNameHeader]).GetString();
            if (string.IsNullOrEmpty(name))
            {
                name = prettyName;
            }

            bool sync = !string.IsNullOrWhiteSpace(row.Cell(columnHeaderToIdx[SyncHeader]).GetString());
            string regex = row.Cell(columnHeaderToIdx[RegexHeader]).GetString();
            string[] testsMustMatch = row.Cell(columnHeaderToIdx[TestsMustMatchHeader]).GetString()
                .Split("\n", StringSplitOptions.RemoveEmptyEntries);
            string[] testsMustNotMatch = row.Cell(columnHeaderToIdx[TestsMustNotMatchHeader]).GetString()
                .Split("\n", StringSplitOptions.RemoveEmptyEntries);
            Term term = new(sync, name, prettyName, regex, testsMustMatch, testsMustNotMatch);
            terms.Add(term);
        }

        return terms;
    }

    private static Dictionary<string, int> ParseHeaders(IXLRow headerRow)
    {
        Dictionary<string, int> columnNameToIndex = new();
        foreach (string columnName in TermWorksheetHeaders)
        {
            // Column index is 1-based
            
            foreach (IXLCell? cell in headerRow.CellsUsed())
            {
                string cellValue = cell.Value.ToString();
                if (cellValue.StartsWith(columnName))
                {
                    columnNameToIndex[columnName] = cell.Address.ColumnNumber;
                    break;
                }
            }

            if (!columnNameToIndex.ContainsKey(columnName))
                Console.WriteLine($"Error in Database: Index of column header '{columnName}' not found");
        }

        return columnNameToIndex;
    }

    private static void ValidateTerms(List<Term> terms)
    {
        HashSet<string> termNames = new();
        HashSet<string> termPrettyNames = new();
        foreach (Term term in terms)
        {
            Regex regex = new(term.Regex, RegexOptions.IgnoreCase);
            foreach (string testMustMatch in term.TestsMustMatch)
                if (!regex.IsMatch(testMustMatch))
                    Console.WriteLine(
                        $"Error in Database: '{term.PrettyName}' should have matched test case: '{testMustMatch}'");

            foreach (string testMustNotMatch in term.TestsMustNotMatch)
                if (regex.IsMatch(testMustNotMatch))
                    Console.WriteLine(
                        $"Error in Database: '{term.PrettyName}' should not have matched test case '{testMustNotMatch}'");
            if (!termNames.Add(term.Name))
            {
                Console.WriteLine($"Error in Database: Duplicate Name '{term.Name}'");
            }

            if (!termPrettyNames.Add(term.PrettyName))
            {
                Console.WriteLine($"Error in Database: Duplicate Pretty Name '{term.PrettyName}'");
            }
        }
    }
}