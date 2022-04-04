using System.Collections.Concurrent;

namespace Stratis.MediaConverterApi.Models;

public class ConversionState
{
    public ConversionState()
    {
        Status = ConversionStatus.Pending;
        ConvertedEntries = new ConcurrentDictionary<string, string>();
    }

    public ConversionStatus Status;

    public ConcurrentDictionary<string, string> ConvertedEntries { get; set; }
    public enum ConversionStatus
    {
        Pending, Complete
    }
}