using System.Collections.Frozen;
using System.Collections.Immutable;

namespace BoosterrCLI;

public static class Program
{
    private static async Task Main()
    {
        ImmutableArray<MediaManagerInstance> enabledInstances = Config.GetMediaManagerInstances()
            .Where(mediaManagerInstance => mediaManagerInstance.IsEnabled).ToImmutableArray();
        ImmutableArray<Term> terms = Database.GetTerms().Where(term => term.Sync).ToImmutableArray();
        FrozenSet<string> termNames = terms.Select(term => term.Name).ToFrozenSet();
        List<Task> tasks = [];
        foreach (MediaManagerInstance instance in enabledInstances)
        {
            Task task = Task.Run(async () =>
            {
                MediaManagerInstanceApiAsync mediaManagerInstanceApi = new MediaManagerInstanceApiAsync(instance);
                bool isConnectable = await mediaManagerInstanceApi.IsConnectableAsync();
                if (!isConnectable)
                {
                    Console.WriteLine($"{instance.Name}: Failed to connect");
                    return;
                }

                ImmutableArray<CustomFormat> customFormats = await mediaManagerInstanceApi.GetAllCustomFormatsAsync();
                await DeleteUnwantedCustomFormats(mediaManagerInstanceApi, instance, customFormats, termNames);
                await AddAndUpdateCustomFormats(mediaManagerInstanceApi, instance, customFormats, terms);
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks.ToArray());
        Console.WriteLine();
        Console.WriteLine("Synchronisation completed.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task DeleteUnwantedCustomFormats(MediaManagerInstanceApiAsync instanceApiAsync,
        MediaManagerInstance instance, ImmutableArray<CustomFormat> customFormats, FrozenSet<string> termNames)
    {
        // Delete custom formats that are not in the Excel file
        foreach (CustomFormat customFormat in customFormats)
        {
            if (!termNames.Contains(customFormat.Name))
            {
                if (customFormat.CreatedByBoosterr)
                {
                    Console.WriteLine($"{instance.Name}: Deleting {customFormat.PrettyName}");
                    await instanceApiAsync.DeleteCustomFormatAsync(customFormat.Id);
                }
                else if (instance.ShouldDeleteNonBoosterrCustomFormats)
                {
                    Console.WriteLine($"{instance.Name}: Eliminating {customFormat.Name}");
                    await instanceApiAsync.DeleteCustomFormatAsync(customFormat.Id);
                }
            }
        }
    }

    private static async Task AddAndUpdateCustomFormats(MediaManagerInstanceApiAsync instanceApiAsync,
        MediaManagerInstance instance, ImmutableArray<CustomFormat> customFormats, ImmutableArray<Term> terms)
    {
        FrozenDictionary<string, CustomFormat> customFormatMap =
            customFormats.ToFrozenDictionary(customFormat => customFormat.Name);
        // Add new custom formats and update existing custom formats
        foreach (Term term in terms)
        {
            // Update custom formats that are in both the Excel file and in the old custom formats list
            if (customFormatMap.TryGetValue(term.Name, out CustomFormat? oldCustomFormat))
            {
                CustomFormat newCustomFormat = new(oldCustomFormat.Id, term.Name, term.PrettyName,
                    oldCustomFormat.IncludeCustomFormatWhenRenaming, term.Regex, instance.Type);

                if (!oldCustomFormat.Equals(newCustomFormat))
                {
                    if (oldCustomFormat.CreatedByBoosterr)
                    {
                        Console.WriteLine($"{instance.Name}: Updating {term.PrettyName}");
                        await instanceApiAsync.UpdateCustomFormatAsync(newCustomFormat);
                    }
                    else if (instance.ShouldOverwriteNonBoosterrCustomFormats)
                    {
                        Console.WriteLine($"{instance.Name}: Overwriting {term.Name}");
                        await instanceApiAsync.UpdateCustomFormatAsync(newCustomFormat);
                    }
                }
            }
            else
                // Add new custom formats that are not in the old custom formats list
            {
                Console.WriteLine($"{instance.Name}: Adding {term.PrettyName}");
                CustomFormat customFormat = new(0, term.Name, term.PrettyName, false, term.Regex,
                    instance.Type);
                await instanceApiAsync.AddCustomFormatAsync(customFormat);
            }
        }
    }
}