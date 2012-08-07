using System;

namespace Junior.Map.Mapper
{
    /// <summary>
    /// Specifies flags that control how a mapper configures the mappings.
    /// </summary>
    [Flags]
    public enum MapperFlags
    {
        /// <summary>
        /// Specifies no mapper flag.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Specifies that the mapper should apply the default mapping conventions included with the framework.
        /// </summary>
        ApplyDefaultMappingConventions = 0x1
    }
}