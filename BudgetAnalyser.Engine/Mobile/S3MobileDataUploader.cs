using System.Diagnostics;
using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Mobile
{
    /// <summary>
    ///     Implementation to upload the mobile summary data to AWS S3 storage
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class S3MobileDataUploader : IMobileDataUploader
    {
        public Task UploadDataFileAsync(string data)
        {
            Debug.WriteLine(data);
            // Stubbed for now
            return Task.CompletedTask;
        }
    }
}