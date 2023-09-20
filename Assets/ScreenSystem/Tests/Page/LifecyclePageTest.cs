using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace ScreenSystem.Page
{
    public class LifecyclePageTest
    {
        private TestPage _testPage;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var obj = Utils.CreatePrimitive(PrimitiveType.Cube);
            var page = obj.AddComponent<UnityScreenNavigator.Runtime.Core.Page.Page>();
            _testPage = new TestPage(page);
            yield return _testPage.Initialize();
        }

        [UnityTest]
        public IEnumerator PageEnter_PageExitCancellation_Not_Requested()
        {
            yield return _testPage.WillPushEnter();
            Assert.That(_testPage.ExitCancellationToken.IsCancellationRequested, Is.False);
        }

        [UnityTest]
        public IEnumerator PageExit_PageExitCancellation_Requested()
        {
            yield return _testPage.WillPushEnter();
            yield return _testPage.WillPushExit();
            Assert.That(_testPage.ExitCancellationToken.IsCancellationRequested, Is.True);
        }

        [UnityTest]
        public IEnumerator Dispose_PageDisposeCancellation_Requested()
        {
            _testPage.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
            {
                var _ = _testPage.DisposeCancellationToken.IsCancellationRequested;
            });

            yield return null;
        }
    }
}