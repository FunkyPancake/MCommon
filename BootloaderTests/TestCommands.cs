using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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


    private async Task CheckCommandNoData(byte[] expectedRequest, byte[] responseBytes,
        Func<Task<ResponseCode>> command) {
        PrepareChecks(responseBytes);
        Predicate<byte[]> isSequenceEqual = s => s.SequenceEqual(expectedRequest);
        var response = await command.Invoke();

        Assert.Equal(ResponseCode.Success, response);
        _iTpMock.Verify(m => m.WriteAsync(Match.Create(isSequenceEqual)));
    }

    private void PrepareChecks(byte[] responseBytes) {
        byte[] ackBytes = {0x5A, 0xA1};
        _iTpMock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<uint>(), CancellationToken.None)).ReturnsAsync(
            (TpStatus.Ok, responseBytes));
        _iTpMock.Setup(x => x.ReadAsync(2, It.IsAny<uint>(), CancellationToken.None))
            .ReturnsAsync((TpStatus.Ok, ackBytes));
        _iTpMock.Setup(x => x.WriteAsync(It.IsAny<byte[]>()));
    }

    [Fact]
    public async void Test_Ping() {
        var expectedFblVersion = new CommonTypes.Version(1, 2, 0);
        var requestExpected = new byte[] {
            0x5A, 0xA6
        };
        var responseBytes = new byte[] {
            0x5A, 0xA7, 0x00, 0x02, 0x01, 0x50, 0x00, 0x00, 0xAA, 0xEA
        };
        await CheckCommandNoData(requestExpected, responseBytes, () => _fblCommands.Ping());
        Assert.Equal(0, _fblCommands.Options);
        Assert.Equal(expectedFblVersion, _fblCommands.FblVersion);
    }

    [Fact]
    public async void Test_FlashEraseAll() {
        var requestExpected = new byte[] {
            0x5A, 0xA4, 0x04, 0x00, 0xC4, 0x2E, 0x01, 0x00, 0x00, 0x00
        };
        var responseBytes = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0x53, 0x63, 0xA0, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00
        };
        await CheckCommandNoData(requestExpected, responseBytes, () => _fblCommands.FLashEraseAll());
    }

    [Fact]
    public async void Test_FlashEraseRegion() {
        uint startAddress = 0;
        uint byteCount = 1024;
        var requestExpected = new byte[] {
            0x5A, 0xA4, 0x10, 0x00, 0x78, 0x06, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };
        var responseBytes = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0xBA, 0x55, 0xA0, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00
        };
        await CheckCommandNoData(requestExpected, responseBytes,
            () => _fblCommands.FlashEraseRegion(startAddress, byteCount));
    }

    [Fact]
    public async void Test_ReadMemory() {
        Assert.True(false);
    }

    [Fact]
    public async void Test_WriteMemory() {
        Assert.True(false);
    }

    [Fact]
    public async void Test_FlashSecurityDisable() {
        const ulong key = 0x0102030405060708;
        var requestExpected = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0x43, 0x7B, 0x06, 0x00, 0x00, 0x02, 0x04, 0x03, 0x02, 0x01, 0x08, 0x07, 0x06, 0x05
        };
        var responseBytes = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0x35, 0x78, 0xA0, 0x00, 0x0C, 0x02, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00
        };
        await CheckCommandNoData(requestExpected, responseBytes, () => _fblCommands.FLashSecurityDisable(key));
    }

    [Fact]
    public async void Test_GetProperty() {
        const PropertyTag propertyTag = PropertyTag.CurrentVersion;
        const uint expectedPropertyValue = 0x0000014b;
        var requestExpected = new byte[] {
            0x5A, 0xA4, 0x08, 0x00, 0x73, 0xD4, 0x07, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00
        };
        var responseBytes = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0x07, 0x7A, 0xA7, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x4B
        };
        PrepareChecks(responseBytes);

        var result = await _fblCommands.GetProperty(propertyTag);
        Predicate<byte[]> isSequenceEqual = s => s.SequenceEqual(requestExpected);
        _iTpMock.Verify(m => m.WriteAsync(Match.Create(isSequenceEqual)));
        Assert.Equal(ResponseCode.Success, result.Item1);
        Assert.Equal(expectedPropertyValue, result.Item2);
    }

    [Fact]
    public async void Test_Execute() {
        const uint startAddress = 0x100;
        const uint arg = 1;
        const uint stackPtrAddr = 0x40001000;
        var requestExpected = new byte[] {
            0x5A, 164, 16, 0, 75, 170, 9, 0, 0, 3, 0, 1, 0, 0, 1, 0, 0, 0, 0, 16, 0, 64
        };
        var responseBytes = new byte[]
            {0x5A, 0xA4, 0x0C, 0x00, 0x90, 0xe6, 0xA0, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x09, 00, 00, 00};
        await CheckCommandNoData(requestExpected, responseBytes,
            () => _fblCommands.Execute(startAddress, arg, stackPtrAddr));
    }

    [Fact]
    public async void Test_Reset() {
        var requestExpected = new byte[] {
            0x5A, 0xA4, 0x04, 0x00, 0x6F, 0x46, 0x0B, 0x00, 0x00, 0x00
        };
        var responseBytes = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0xF8, 0x0B, 0xA0, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x00, 0x00
        };
        await CheckCommandNoData(requestExpected, responseBytes, () => _fblCommands.Reset());
    }

    [Fact]
    public async void Test_SetProperty() {
        const PropertyTag propertyTag = PropertyTag.VerifyWrites;
        const uint propertyValue = 1;
        var requestExpected = new byte[] {
            0x5A , 0xA4 , 0x0C , 0x00 , 0x67 , 0x8D , 0x0C , 0x00 , 0x00 , 0x02 , 0x0A , 0x00 , 0x00 , 0x00 , 0x01 , 0x00 , 0x00 , 0x00
        };
        var responseBytes = new byte[] {
            0x5A , 0xA4 , 0x00 , 0x9E , 0x10 , 0xA0 , 0x00 , 0x0C , 0x02 , 0x00 , 0x00 , 0x00 , 0x00 , 0x0C , 0x00 , 0x00 , 0x00};
        await CheckCommandNoData(requestExpected, responseBytes, () => _fblCommands.SetProperty(propertyTag,propertyValue));
    }

    [Fact]
    public async void Test_FlashEraseAllUnsecure() {
        var requestExpected = new byte[] {
            0x5A, 0xA4, 0x04, 0x00, 0xF6, 0x61, 0x0D, 0x00, 0x00, 0x00
        };
        var responseBytes = new byte[] {
            0x5A, 0xA4, 0x0C, 0x00, 0x61, 0x2C, 0xA0, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x0D, 0x00, 0x00, 0x00
        };
        await CheckCommandNoData(requestExpected, responseBytes, () => _fblCommands.FlashEraseAllUnsecure());
    }
}