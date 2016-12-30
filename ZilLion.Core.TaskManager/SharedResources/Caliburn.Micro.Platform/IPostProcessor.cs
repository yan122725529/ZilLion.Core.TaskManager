namespace Caliburn.Micro
{
    /// <summary>
    ///     Add by yanzhengyu
    /// </summary>
    public interface IPostProcessor
    {
        /// <summary>
        /// </summary>
        bool Execute(object actionContext);

        /// <summary>
        /// 执行顺序
        /// </summary>
        int PostOrder { get; set; }
    }

    
}