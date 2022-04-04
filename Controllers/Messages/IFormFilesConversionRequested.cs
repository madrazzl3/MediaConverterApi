namespace Stratis.MediaConverterApi.Messages;

public interface IFormFilesConversionRequested
{
    /// <summary>
    /// Request ID.
    /// </summary>
    string RequestID { get; set; }

    /// <summary>
    /// Files to be converted.
    /// </summary>
    IList<string> Files { get; set; }
}