using System;
using NUnit.Framework;
using FluentAssertions;
using WebTimer.ServiceEntities.UserData;

namespace WebTimer.ServiceEntities.Tests
{
    [TestFixture]
    public class DeviceTests
    {
        [Test]
        public void ValidateDeviceId()
        {
            var id = Device.CreateDeviceId();
            id.Length.Should().Be(12);
        }
    }
}
