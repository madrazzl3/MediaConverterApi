namespace Stratis.MediaConverterApi
{
    public interface IHashProvider
    {
        public string GetFileHash(string fileName);
    }
}