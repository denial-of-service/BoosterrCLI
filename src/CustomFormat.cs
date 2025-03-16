using System.Collections.Immutable;
using System.Text.Json;

namespace Boosterr;

public record CustomFormat
{
    private const string BoosterrIdentifier = "Boosterr";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false
    };

    public int Id { get; }
    public string Name { get; }
    public string PrettyName { get; }
    public bool IncludeCustomFormatWhenRenaming { get; }
    public bool CreatedByBoosterr { get; }
    public string NormalizedJson { get; }

    public static ImmutableArray<CustomFormat> ParseAll(string jsonArray)
    {
        JsonDocument document = JsonDocument.Parse(jsonArray);
        return [..document.RootElement.EnumerateArray().Select(jsonElement => new CustomFormat(jsonElement))];
    }

    private CustomFormat(JsonElement rootElement)
    {
        // We don't care about the Regex field because it's not used in the comparison.
        NormalizedJson = JsonSerializer.Serialize(rootElement, JsonSerializerOptions);
        Id = rootElement.GetProperty("id").GetInt32();
        Name = rootElement.GetProperty("name").GetString() ?? throw new InvalidOperationException();
        IncludeCustomFormatWhenRenaming = rootElement.GetProperty("includeCustomFormatWhenRenaming").GetBoolean();
        JsonElement specifications = rootElement.GetProperty("specifications");
        JsonElement lastSpecification = specifications[specifications.GetArrayLength() - 1];
        CreatedByBoosterr = lastSpecification.GetProperty("name").GetString() == BoosterrIdentifier;
        PrettyName = Name;
        if (CreatedByBoosterr && specifications.GetArrayLength() == 2)
        {
            PrettyName = specifications[0].GetProperty("name").GetString() ?? throw new InvalidOperationException();
        }
    }

    public CustomFormat(int id, string name, string prettyName, bool includeCustomFormatWhenRenaming, string regex,
        MediaManagerType mediaManagerType)
    {
        Id = id;
        Name = name;
        PrettyName = prettyName;
        IncludeCustomFormatWhenRenaming = includeCustomFormatWhenRenaming;
        CreatedByBoosterr = true;
        NormalizedJson = ToJson(regex, mediaManagerType);
    }

    public virtual bool Equals(CustomFormat? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return NormalizedJson == other.NormalizedJson;
    }

    public override int GetHashCode()
    {
        return NormalizedJson.GetHashCode();
    }

    private string ToJson(string regex, MediaManagerType mediaManagerType)
    {
        string denormalizedJson;
        if (PrettyName == Name)
            denormalizedJson =
                $$"""
                  {
                    "id": {{Id}},
                    "name": {{JsonSerializer.Serialize(Name)}},
                    "includeCustomFormatWhenRenaming": {{JsonSerializer.Serialize(IncludeCustomFormatWhenRenaming)}},
                    "specifications": [
                      {
                        "name": "{{BoosterrIdentifier}}",
                        "implementation": "ReleaseTitleSpecification",
                        "implementationName": "Release Title",
                        "infoLink": "https://wiki.servarr.com/{{mediaManagerType.ToLowercase()}}/settings#custom-formats-2",
                        "negate": false,
                        "required": false,
                        "fields": [
                          {
                            "order": 0,
                            "name": "value",
                            "label": "Regular Expression",
                            "helpText": "Custom Format RegEx is Case Insensitive",
                            "value": {{JsonSerializer.Serialize(regex)}},
                            "type": "textbox",
                            "advanced": false,
                            "privacy": "normal",
                            "isFloat": false
                          }
                        ]
                      }
                    ]
                  }
                  """;
        else
            denormalizedJson =
                $$"""
                  {
                    "id": {{Id}},
                    "name": {{JsonSerializer.Serialize(Name)}},
                    "includeCustomFormatWhenRenaming": {{JsonSerializer.Serialize(IncludeCustomFormatWhenRenaming)}},
                    "specifications": [
                      {
                        "name": {{JsonSerializer.Serialize(PrettyName)}},
                        "implementation": "ReleaseTitleSpecification",
                        "implementationName": "Release Title",
                        "infoLink": "https://wiki.servarr.com/{{mediaManagerType.ToLowercase()}}/settings#custom-formats-2",
                        "negate": false,
                        "required": true,
                        "fields": [
                          {
                            "order": 0,
                            "name": "value",
                            "label": "Regular Expression",
                            "helpText": "Custom Format RegEx is Case Insensitive",
                            "value": {{JsonSerializer.Serialize(regex)}},
                            "type": "textbox",
                            "advanced": false,
                            "privacy": "normal",
                            "isFloat": false
                          }
                        ]
                      },
                      {
                        "name": "{{BoosterrIdentifier}}",
                        "implementation": "ReleaseTitleSpecification",
                        "implementationName": "Release Title",
                        "infoLink": "https://wiki.servarr.com/{{mediaManagerType.ToLowercase()}}/settings#custom-formats-2",
                        "negate": false,
                        "required": false,
                        "fields": [
                          {
                            "order": 0,
                            "name": "value",
                            "label": "Regular Expression",
                            "helpText": "Custom Format RegEx is Case Insensitive",
                            "value": "^_^",
                            "type": "textbox",
                            "advanced": false,
                            "privacy": "normal",
                            "isFloat": false
                          }
                        ]
                      }
                    ]
                  }
                  """;

        // Normalize the Json by parsing it and then serializing it.
        JsonDocument jsonDocument = JsonDocument.Parse(denormalizedJson);
        return JsonSerializer.Serialize(jsonDocument.RootElement, JsonSerializerOptions);
    }
}