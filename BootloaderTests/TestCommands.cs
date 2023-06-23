using System;
using System.Linq;
using CalTp.Bootloader.BootloaderLogic;
using CalTp.TransportProtocol;
using Moq;
using Serilog;
using Xunit;

namespace BootloaderTests;

public class TestCommands {
    private static Mock<ILogger> MockLogger()
    {
        var logger = new Mock<ILogger>();
        logger.Setup(x => x.Verbose(It.IsAny<Exception>(), It.IsAny<string>()));
        logger.Setup(x => x.Information(It.IsAny<Exception>(), It.IsAny<string>()));
        logger.Setup(x => x.Information(It.IsAny<string>(), It.IsAny<string>()));
        logger.Setup(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>()));
        logger.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<string>()));
        logger.Setup(x => x.Error(It.IsAny<string>()));
        logger.Setup(x => x.ForContext<object>()).Returns(logger.Object);
        logger.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), false)).Returns(logger.Object);

        return logger;
    }
    [Fact]
    public async void Test_Execute() {
        var requestExpected = new byte[] {
            0x5A, 164, 16, 0, 75, 170, 9, 0, 0, 3, 0, 1, 0, 0, 1, 0, 0, 0, 0, 16, 0, 64
        };
        var genericResponseBytes = new byte[]
            {0x5A, 0xA4, 0x0C, 0x00, 0x90, 0xe6, 0xA0, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x09, 00, 00, 00};
        Predicate<byte[]> isThreeCharsLong = s => s.SequenceEqual(requestExpected);
        ;
        var iTpMock = new Mock<ITransportProtocol>();
        iTpMock.Setup(x => x.ReadAsync(It.IsAny<int>())).ReturnsAsync(genericResponseBytes);
        iTpMock.Setup(
            x => x.WriteAsync(It.IsAny<byte[]>()));

        var loggerMock = MockLogger();
        var commands = new Commands(loggerMock.Object, iTpMock.Object);
        var response = await commands.Execute(0x100, 1, 0x40001000);
        Assert.True(response);
        iTpMock.Verify(m => m.WriteAsync(Match.Create<byte[]>(isThreeCharsLong)));
    }
}