namespace TestApp.Services
{
    public interface IModelConverterFactory
    {
        IModelConverter Get(string extension);
    }
}
