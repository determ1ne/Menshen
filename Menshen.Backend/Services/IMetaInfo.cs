namespace Menshen.Backend.Services
{
    public interface IMetaInfo
    {
        // no plan to use other dbms yet
        // so the methods are NOT async
        string GetValue(string key);
        void SetValue(string key, string value);
    }
}