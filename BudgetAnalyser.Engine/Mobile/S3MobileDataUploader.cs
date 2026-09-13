using System.Security;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace BudgetAnalyser.Engine.Mobile;

/// <summary>
///     Implementation to upload the mobile summary data to AWS S3 storage
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class S3MobileDataUploader(ILogger logger) : IMobileDataUploader
{
    private const string AwsBucketName = "baxmobilesummary";
    private const string AwsBucketFileName = MobileDataExporter.MobileDataExportName;
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task UploadDataFileAsync(string data, string storageKeyId, string storageSecret, string region)
    {
        var regionIdentifier = RegionEndpoint.GetBySystemName(region);
        using var client = new AmazonS3Client(storageKeyId, storageSecret, regionIdentifier);
        try
        {
            var putRequest1 = new PutObjectRequest { BucketName = AwsBucketName, Key = AwsBucketFileName, ContentBody = data, ContentType = "application/json" };

            await client.PutObjectAsync(putRequest1);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            if (amazonS3Exception.ErrorCode is not null &&
                (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
            {
                throw new SecurityException("Invalid Amazon S3 credentials - data was not uploaded.", amazonS3Exception);
            }

            throw new HttpRequestException("Unspecified error attempting to upload data: " + amazonS3Exception.Message, amazonS3Exception);
        }

        var response = await client.GetObjectAsync(AwsBucketName, AwsBucketFileName);
        this.logger.LogInfo(_ =>
            {
                using var reader = new StreamReader(response.ResponseStream);
                return reader.ReadToEnd();
            }
        );
    }
}
