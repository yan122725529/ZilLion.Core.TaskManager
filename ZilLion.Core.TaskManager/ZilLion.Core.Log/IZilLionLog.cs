namespace ZilLion.Core.Log
{
    public interface IZilLionLog<in TMessage>
    {
        void LogInfo(TMessage message);
       
    }
}