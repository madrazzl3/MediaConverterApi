namespace Stratis.MediaConverterApi.Messages;

public interface ILinksConversionRequested
{
    /// <summary>
    /// Request ID.
    /// </summary>
    string RequestID { get; set; }

    /// <summary>
    /// Links to the file to be converted.
    /// </summary>
    List<string> Links { get; set; }
}