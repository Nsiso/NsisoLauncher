namespace NsisoLauncherCore.Modules
{
    /// <summary>
    /// GC类型
    /// </summary>
    public enum GCType
    {
        /// <summary>
        /// 默认G1垃圾回收器 兼容JAVA9
        /// </summary>
        G1GC = 0,

        /// <summary>
        /// 串行垃圾回收器
        /// </summary>
        SerialGC = 1,

        /// <summary>
        /// 并行垃圾回收器
        /// </summary>
        ParallelGC = 2,

        /// <summary>
        /// 并发标记扫描垃圾回收器
        /// </summary>
        CMSGC = 3,

        /// <summary>
        /// 设置为空（手动设置）
        /// </summary>
        NULL = 4
    }
}
