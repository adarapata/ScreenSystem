using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace ScreenSystem.Modal
{
    public class LifecycleModalTest
    {
        private TestModal _testModal;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var obj = Utils.CreatePrimitive(PrimitiveType.Cube);
            var modal = obj.AddComponent<UnityScreenNavigator.Runtime.Core.Modal.Modal>();
            _testModal = new TestModal(modal);
            yield return _testModal.Initialize();
        }
        
        [UnityTest]
        public IEnumerator ModalEnter_ModalExitCancellation_Not_Requested()
        {
            yield return _testModal.WillPushEnter();
            Assert.That(_testModal.ExitCancellationToken.IsCancellationRequested, Is.False);
        }

        [UnityTest]
        public IEnumerator PageExit_PageExitCancellation_Requested()
        {
            yield return _testModal.WillPushEnter();
            yield return _testModal.WillPushExit();
            Assert.That(_testModal.ExitCancellationToken.IsCancellationRequested, Is.True);
        }

        
        [UnityTest]
        public IEnumerator Dispose_PageDisposeCancellation_Requested()
        {
            _testModal.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
            {
                var _ = _testModal.DisposeCancellationToken.IsCancellationRequested;
            });

            yield return null;
        }
    }
}