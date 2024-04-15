using System.Threading;
using Cysharp.Threading.Tasks;

namespace ScreenSystem.Modal
{
    public interface IModal
    {
        UniTask OnCloseAsync(CancellationToken cancellationToken);
        
        string ModalId { get; }
    }
}