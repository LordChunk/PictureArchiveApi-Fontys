using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DAL
{
    public class Picture
    {
        private readonly string _connectionString;
        private readonly string _azureConnectionString;
        public Picture(
            string connectionString,
            string azureConnectionString
            )
        {
            _connectionString = connectionString;
            _azureConnectionString = azureConnectionString;
        }

        public List<DalPicture> GetPictures(int amount = 0, int offset = 0)
        {
            List<DalPicture> pictureList = new List<DalPicture>();

            // Open connection and execute procedure
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("GetPictures", conn))
            {
                command.CommandType = CommandType.StoredProcedure;
                
                // Check for negative values and exclude them if so
                if (amount > 0) { addParameter(command, "amount", amount.ToString()); }
                if (offset > 0) { addParameter(command, "offset", offset.ToString()); }

                conn.Open();
                DbDataReader reader = command.ExecuteReader();

                // Loop through all row
                while (reader.Read())
                {
                    // Create picture element
                    DalPicture newPicture = new DalPicture
                    {
                        Id = reader.GetValue(0).ToString(),
                        UserId    = reader.GetValue(1).ToString()
                    };

                    // Add to list
                    pictureList.Add(newPicture);
                }

                conn.Close();
            }

            return pictureList;
        }

        public void StorePictureInBlobStorage(List<LogicPicture> files, string userId)
        {
            // Create storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_azureConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("userpictures");
            
            // Create list for file names
            List<string> fileNameList = new List<string>();

            foreach (var file in files)
            {
                // Create data reader for form files 
                var data = new MemoryStream();

                // Get GUID file name with file extension
                string fileName = ConvertFileToGuidFile(file);

                // Generate file reference
                string fileRef = $"{userId}/{fileName}";

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileRef);

                // Convert form data to Byte array
                file.CopyTo(data);
                var fileBytes = data.ToArray();
                
                blockBlob.UploadFromByteArrayAsync(fileBytes, 0, fileBytes.Length);

                fileNameList.Add(fileName);
            }

            // Add all file names to the database
            AddFilePathsToDb(fileNameList, userId);
        }

        private void AddFilePathsToDb(List<string> fileNameList, string userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            foreach (string filename in fileNameList)
            {
                using (SqlCommand command = new SqlCommand("AddPicture", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    addParameter(command, "Id", filename);
                    addParameter(command, "UserId", userId);

                    conn.Open();
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
        private void addParameter(SqlCommand command, string name, string value)
        {
            command.Parameters.Add(new SqlParameter(name, value));
        }
        
        private string ConvertFileToGuidFile(IFormFile file)
        {
            string pictureGuid = Convert.ToString(Guid.NewGuid());

            // Getting FileName
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;

            // Getting file Extension + remove the \ character at the end of the string
            var FileExtension = Path.GetExtension(fileName).Replace("\"", "");

            // Combining FileName + FileExtension
           return pictureGuid + FileExtension;
        }

        // Checks if folder has already been created and creates one if false
        private void EnsureFolder(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            // If path is a file name only, directory name will be an empty string
            if (directoryName.Length > 0)
            {
                // Create all directories on the path that don't already exist
                Directory.CreateDirectory(directoryName);
            }
        }
    }
}
