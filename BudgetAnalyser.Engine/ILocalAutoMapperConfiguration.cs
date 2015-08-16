namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An interface to describe a localised set of AutoMapper configurations.  This allows AutoMapper configurations to be
    ///     split apart so that each bound context can
    ///     contain its own mapping configuration, rather than one big monolithic class with many references.
    /// </summary>
    public interface ILocalAutoMapperConfiguration
    {
        /// <summary>
        ///     Register the localised mappings.
        /// </summary>
        void RegisterMappings();
    }
}