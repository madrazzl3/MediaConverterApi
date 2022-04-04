using Stratis.MediaConverterApi.Models;

public interface IConversionRequestsStorage
{
    public Task StoreConversionResults(string requestID, ConversionState state);
    public Task<ConversionState?> GetConversionResults(string requestID);
}