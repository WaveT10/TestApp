namespace TestApp.Services
{
    internal sealed class ModelConverterFactory : IModelConverterFactory
    {
        public IModelConverter Get(string extension) 
        {
            return extension.TrimStart('.').ToLower() switch
            {
                ContentTypes.TSV => new TSVModelConverter(),
                _ => throw new NotImplementedException()
            };
        }
    }
}
