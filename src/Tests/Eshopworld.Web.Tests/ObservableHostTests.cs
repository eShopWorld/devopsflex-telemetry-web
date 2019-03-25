using System;
using System.Threading;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class ObservableHostTests
    {
        [Fact, IsLayer0]
        public void BasicFlowTest()
        {
            var os = new NotificationObservableHost();

            var signalA = new ManualResetEvent(false);
            os.Subscribe<TestNotification>((item) => { signalA.Set(); });

            os.NewEvent(new TestNotification());
            signalA.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        [Fact, IsLayer0]
        public void BasicFlowTestDifferentMessageType()
        {
            var os = new NotificationObservableHost();

            var signalA = new ManualResetEvent(false);
            os.Subscribe<TestNotification>((item) => { signalA.Set(); });

            os.NewEvent(new TestOtherNotification());
            signalA.WaitOne(TimeSpan.FromSeconds(1)).Should().BeFalse();
        }

        [Fact, IsLayer0]
        public void SubscribeAllFlow()
        {
            var os = new NotificationObservableHost();

            var signalA = new ManualResetEvent(false);
            os.SubscribeToAll((item) => { signalA.Set(); });

            os.NewEvent(new TestOtherNotification());
            signalA.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        public class TestNotification
        {
        }

        public class TestOtherNotification
        {
        }
    }
}
