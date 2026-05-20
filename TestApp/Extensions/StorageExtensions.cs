using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using TestApp.Services;

namespace TestApp
{
    internal static class StorageExtensions
    {
        public static async Task<IStorageFile?> SaveFunctionsFilePickerAsync(this IStorageProvider storageProvider) 
        {
            return await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Functions",
                SuggestedFileName = "Functions",
                FileTypeChoices =
                [
                    new FilePickerFileType("Tab-Separated Values") { Patterns = [$"*.{ContentTypes.TSV}"] }
                ]
            });
        }

        public static async Task Save(this IStorageFile file, string content)
        {
            await using var stream = await file.OpenWriteAsync();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
        }

        public static async Task<IStorageFile?> OpenFunctionsFilePickerAsync(this IStorageProvider storageProvider)
        {
            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Functions",
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("Tab-Separated Values") { Patterns = [$"*.{ContentTypes.TSV}"] }]
            });

            return files.SingleOrDefault();
        }

        public static async Task<string> Open(this IStorageFile file)
        {
            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static string GetExtension(this IStorageFile file)
        {
            return Path.GetExtension(file.Name);
        }
    }
}
