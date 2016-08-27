using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Mobile
{
    /// <summary>
    ///     An interface to describe how the mobile data file is uploaded.
    /// </summary>
    public interface IMobileDataUploader
    {
        /// <summary>
        ///     Uploads the data to cloud storage
        /// </summary>
        Task UploadDataFileAsync(string data);
    }
}