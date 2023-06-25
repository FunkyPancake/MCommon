using System;
using System.Linq;
using CalTp.Bootloader;
using CalTp.TransportProtocol;
using Moq;
using Xunit;

namespace BootloaderTests;

public class TestCommands : IDisposable {
    private readonly FblCommands _fblCommands;
    private readonly Mock<ITransportProtocol> _iTpMock = new();

    public TestCommands() {
        var logger = CommonMocks.MockLogger();
        _fblCommands = new FblCommands(logger.Object, _iTpMock.Object);
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async void Test_Ping() {
    }

    [Fact]
    public async void Test_FlashEraseAll() {
    }

    [Fact]
    public async void Test_FlashEraseRegion() {
    }

    [Fact]
    public async void Test_ReadMemory() {
    }

    [Fact]
    public async void Test_WriteMemory() {
    }

    [Fact]
    public async void Test_FlashSecurityDisable() {
        const ulong key = 0x0102030405060708;
        var requestExpected = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0x43, 0x7B, 0x06, 0x00, 0x00, 0x02, 0x04, 0x03, 0x02, 0x01, 0x08, 0x07, 0x06, 0x05
        };
        var responseBytes = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0x90, 0xe6, 0xA0, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x09, 00, 00, 00
        };
        Predicate<byte[]> isThreeCharsLong = s => s.SequenceEqual(requestExpected);
        _iTpMock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(responseBytes);
        _iTpMock.Setup(x => x.WriteAsync(It.IsAny<byte[]>()));

        var response = await _fblCommands.FLashSecurityDisable(key);
        Assert.Equal(ResponseCode.Success, response);

        _iTpMock.Verify(m => m.WriteAsync(Match.Create(isThreeCharsLong)));
    }

    [Fact]
    public async void Test_GetProperty() {
    }

    [Fact]
    public async void Test_Execute() {
        var requestExpected = new byte[] {
            0x5A, 164, 16, 0, 75, 170, 9, 0, 0, 3, 0, 1, 0, 0, 1, 0, 0, 0, 0, 16, 0, 64
        };
        var responseBytes = new byte[]
            {0x5A, 0xA4, 0x0C, 0x00, 0x90, 0xe6, 0xA0, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x09, 00, 00, 00};
        Predicate<byte[]> isThreeCharsLong = s => s.SequenceEqual(requestExpected);
        ;
        _iTpMock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(responseBytes);
        _iTpMock.Setup(x => x.WriteAsync(It.IsAny<byte[]>()));

        var loggerMock = CommonMocks.MockLogger();
        var commands = new FblCommands(loggerMock.Object, _iTpMock.Object);
        var response = await commands.Execute(0x100, 1, 0x40001000);
        Assert.Equal(ResponseCode.Success, response);
        _iTpMock.Verify(m => m.WriteAsync(Match.Create<byte[]>(isThreeCharsLong)));
    }

    [Fact]
    public async void Test_Reset() {
        var requestExpected = new byte[] {
            0x5A, 0xA4, 0x04, 0x00, 0x6F, 0x46, 0x0B, 0x00, 0x00, 0x00
        };
        var responseBytes = new byte[]

            {0x5A, 0xA4, 0x0C, 0x00, 0x90, 0xe6, 0xA0, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x09, 00, 00, 00};
        Predicate<byte[]> isThreeCharsLong = s => s.SequenceEqual(requestExpected);
        ;
        _iTpMock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(responseBytes);
        _iTpMock.Setup(x => x.WriteAsync(It.IsAny<byte[]>()));

        var response = await _fblCommands.Reset();
        Assert.Equal(ResponseCode.Success, response);

        _iTpMock.Verify(m => m.WriteAsync(Match.Create(isThreeCharsLong)));
        _iTpMock.Verify(m => m.WriteAsync(Match.Create(isThreeCharsLong)));
    }

    [Fact]
    public async void Test_SetProperty() {
    }

    [Fact]
    public async void Test_FlashEraseAllUnsecure() {
    }
}