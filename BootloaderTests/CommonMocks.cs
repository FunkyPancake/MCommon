using System;
using Moq;
using Serilog;

namespace BootloaderTests;

public static class CommonMocks {

    public static Mock<ILogger> MockLogger() {
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
}