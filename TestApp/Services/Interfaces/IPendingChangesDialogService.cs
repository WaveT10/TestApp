using System.Threading.Tasks;

namespace TestApp
{
    public interface IPendingChangesDialogService
    {
        Task<PendingChangesDialogResult> Show();
    }
}