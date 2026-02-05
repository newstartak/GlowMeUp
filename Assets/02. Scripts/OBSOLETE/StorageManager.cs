using Google.Cloud.Storage.V1;
using System.IO;
using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public static class StorageManager
{
    private static StorageClient _storage;

    private static string bucketName = "beautyx";

    public static async Task InitStorage()
    {
        try
        {
            var gcsKeyPath = await StreamingWorker.GetFile("GcsKey.json");

            var credential = GoogleCredential.FromFile(gcsKeyPath);

            _storage = StorageClient.Create(credential);

            NLogManager.Info("Storage init completed.");
        }
        catch(Exception ex)
        {
            NLogManager.Error($"Error occured during init gcs storage: {ex}");

            throw;
        }
    }

    /// <summary>
    /// GCS에 로컬에 저장된 파일 업로드
    /// </summary>
    /// <param name="path">업로드할 파일 경로</param>
    /// <param name="fileName">업로드할 파일명</param>
    /// <returns></returns>
    static public async Task UploadFileAsync(string path, string fileName = null, string isAi = "false")
    {
        if(string.IsNullOrEmpty(fileName))
        {
            fileName = Path.GetFileName(path);
        }

        try
        {
            using var fileStream = File.OpenRead(path);

            var uploadObj = new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = bucketName,
                Name = $"test/img/{fileName}",

                ContentType = "image/png",
                CacheControl = "max-age=86400",

                Metadata = new Dictionary<string, string>
                {
                    { "isAi", isAi },
                    { "char_name", $"{fileName}" }
                }
            };

            await _storage.UploadObjectAsync(uploadObj, fileStream);

            NLogManager.Info($"File Uploaded: {fileName}");
        }
        catch(Exception ex)
        {
            NLogManager.Error($"Error occured during uploading file: {ex.Message}");
        }
    }

    /// <summary>
    /// GCS에 저장된 파일 삭제
    /// </summary>
    /// <param name="fileName">삭제할 파일명, 확장자 미작성 시 png로 적용</param>
    /// <returns></returns>
    static public async Task DeleteFileAsync(string fileName)
    {
        if(fileName.Contains('.') == false)
        {
            fileName += ".png";
        }

        try
        {
            await _storage.DeleteObjectAsync("beautyx", $"test/{fileName}");

            NLogManager.Info($"File Deleted: {fileName}");
        }
        catch (Exception ex)
        {
            NLogManager.Error($"Error occured during deleting file: {ex.Message}");
        }
    }

    /// <summary>
    /// 업로드와 삭제 동시에 하기 위한 유틸성 메서드
    /// </summary>
    /// <param name="path_upload">업로드할 파일이 저장된 경로</param>
    /// <param name="fileName_delete">삭제할 파일명, 확장자 미작성 시 png로 적용</param>
    /// <param name="fileName_upload">업로드할 파일명, 생략 가능</param>
    /// <returns></returns>
    static public async Task ReplaceFileAsync(string path_upload, string fileName_delete, string fileName_upload = null)
    {
        await DeleteFileAsync(fileName_delete);
        await UploadFileAsync(path_upload, fileName_upload);
    }

    /// <summary>
    /// 메모리에 올라가 있는 바이트 그대로 스토리지에 업로드
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    static public async Task UploadMemoryAsync(byte[] memory, string fileName)
    {
        try
        {
            using var memoryStream = new MemoryStream(memory);

            var uploadObj = new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = bucketName,
                Name = $"test/img/{fileName}",

                ContentType = "image/png",
                CacheControl = "max-age=86400",

                Metadata = new Dictionary<string, string>
                {
                    { "isAi", "false" },
                    { "char_name", $"{fileName}" }
                }
            };

            await _storage.UploadObjectAsync(uploadObj, memoryStream);

            NLogManager.Info($"File Uploaded: {fileName}");
        }
        catch (Exception ex)
        {
            NLogManager.Error($"Error occured during uploading file: {ex.Message}");
        }
    }
}
