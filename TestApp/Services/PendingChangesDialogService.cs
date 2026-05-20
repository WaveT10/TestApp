using System.Threading.Tasks;
using Avalonia.Controls;

namespace TestApp
{
    internal sealed class PendingChangesDialogService : IPendingChangesDialogService
    {
        private readonly Window _dialogOwner;

        public PendingChangesDialogService(Window dialogOwner)
        {
            _dialogOwner = dialogOwner;
        }

        public async Task<PendingChangesDialogResult> Show()
        {
            return await new PendingChangesDialog().ShowDialog<PendingChangesDialogResult>(_dialogOwner);
        }
    }
}
