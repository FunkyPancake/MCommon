﻿namespace DbcReader;

public class DbcReader {
    public DbcReader(string filePathDbc) {
    }

    public object GetFrameId(string ecuId, string caltx) {
        throw new NotImplementedException();
    }

    // public (CanFrame tx, CanFrame rx) GetCalFrames(string ecuId) {
        // throw new NotImplementedException();
    // }
    public (uint tx, uint rx) GetCalFrames(object getTargetEcu) {
        throw new NotImplementedException();
    }
}