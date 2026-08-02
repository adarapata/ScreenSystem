using NUnit.Framework;

namespace ScreenSystem.Page
{
	public sealed class PageTransitionGateTest
	{
		[Test]
		public void TryAcquire_Leaseの解放まで後続取得を拒否する()
		{
			var gate = new PageTransitionGate();

			Assert.That(gate.TryAcquire(out var firstLease), Is.True);
			Assert.That(gate.TryAcquire(out _), Is.False);

			firstLease.Dispose();

			Assert.That(gate.TryAcquire(out var secondLease), Is.True);
			secondLease.Dispose();
		}

		[Test]
		public void Dispose_同じLeaseを複数回解放しても新しい取得を壊さない()
		{
			var gate = new PageTransitionGate();
			Assert.That(gate.TryAcquire(out var firstLease), Is.True);
			firstLease.Dispose();
			Assert.That(gate.TryAcquire(out var secondLease), Is.True);

			firstLease.Dispose();

			Assert.That(gate.TryAcquire(out _), Is.False);
			secondLease.Dispose();
		}

		[Test]
		public void PageManager_IPageNavigatorを実装する()
		{
			Assert.That(typeof(IPageNavigator).IsAssignableFrom(typeof(PageManager)), Is.True);
		}
	}
}
