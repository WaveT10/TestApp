using TestApp.Abstractions;

namespace TestApp.Services
{
    public interface IModelConverter
    {
        void ToFunction(Function funtion, string? text, bool overwriteIfEmptyText = false);

        string FromFunction(Function function);

        void ToFunctions(Functions functions, string? text, bool overwriteIfEmptyText = false);

        string FromFunctions(Functions functions);
    }
}
