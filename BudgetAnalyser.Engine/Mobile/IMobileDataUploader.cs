using System.Net.Http;
using System.Security;
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
        /// <param name="data">The JSON data to upload</param>
        /// <param name="storageKeyId">The Amazon AccessKeyId</param>
        /// <param name="storageSecret">The Amazon SecretAccessKey</param>
        /// <param name="region">The Amazon region the data is stored in</param>
        /// <exception cref="SecurityException">Will be thrown if authentication credentials fail.</exception>
        /// <exception cref="HttpRequestException">Will be thrown for any other kind of communications failure.</exception>
        Task UploadDataFileAsync(string data, string storageKeyId, string storageSecret, string region);
    }
}