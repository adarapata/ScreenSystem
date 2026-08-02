using System.Threading;
using Cysharp.Threading.Tasks;

namespace ScreenSystem.Page
{
	public interface IPageNavigator
	{
		UniTask<IPage> Push(IPageBuilder builder, CancellationToken cancellationToken);

		UniTask Pop(bool playAnimation, CancellationToken cancellationToken);
	}
}
