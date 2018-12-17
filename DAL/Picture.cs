using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System;

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
                        UserId = reader.GetValue(1).ToString(),
                        FileExtension = reader.GetValue(2).ToString()
                    };

                    // Add to list
                    pictureList.Add(newPicture);
                }

                conn.Close();
            }

            return pictureList;
        }

        public async Task<List<string>> StorePictureInBlobStorage(List<DalPicture> files)
        {
            // Create storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_azureConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("userpictures");

            List<string> uriList = new List<string>();

            foreach (var file in files)
            {
                // Create data reader for form files 
                var data = new MemoryStream();

                // Generate file reference
                string fileRef = $"{file.UserId}/{file.Id}.{file.FileExtension}";

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileRef);

                // Configure blob upload
                //blockBlob.Properties.ContentType = file.FileExtension; 
                // ^replaced by just manually adding the file extension with the file ref
                await blockBlob.UploadFromByteArrayAsync(file.Base64, 0, file.Base64.Length);

                // Add item to database
                AddImageDataToDb(file);

                // Add item to list
                uriList.Add(fileRef);
            }

            // Return URI list
            return uriList;
        }

        private void AddImageDataToDb(DalPicture picture)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("AddPicture", conn))
            {
                command.CommandType = CommandType.StoredProcedure;

                addParameter(command, "Id", picture.Id);
                addParameter(command, "UserId", picture.UserId);
                addParameter(command, "Name", picture.Name);
                addParameter(command, "Date", picture.Date);
                addParameter(command, "@FileExtension", picture.FileExtension);

                conn.Open();
                command.ExecuteNonQuery();
                conn.Close();
            }

            // Add Metatag to db and add metatag link in link table
            foreach(string metaTag in picture.MetaTags)
            {
                // Add metatag and get metatag id
                string id = AddMetaTag(metaTag);

                // Add link between picture and metatag
                addPictureMetaTagLink(id, picture.Id);
            }
        }

        private string AddMetaTag(string name)
        {
            string Id;
            // Add new row
            try
            {
                Id = Convert.ToString(Guid.NewGuid());
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("AddMetaTag", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    addParameter(command, "Id", Id);
                    addParameter(command, "Name", name);

                    conn.Open();
                    command.ExecuteNonQuery();
                    conn.Close();
                }

                return Id;
            }
            // If meta tag already exists get already created metatag id
            catch(Exception e)
            {
                Console.WriteLine(e);
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("GetMetaTagId", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    
                    addParameter(command, "Name", name);

                    conn.Open();
                    DbDataReader reader = command.ExecuteReader();

                    // Get first row
                    reader.Read();
                    Id = reader.GetValue(0).ToString();
                    
                    conn.Close();

                    return Id;
                }
            }
        }
        private void addPictureMetaTagLink(string metaTagId, string pictureId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("PictureMetaTagLink", conn))
            {
                command.CommandType = CommandType.StoredProcedure;

                addParameter(command, "MetaTagId", metaTagId);
                addParameter(command, "PictureId", pictureId);

                conn.Open();
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        // slight shorthand for adding parameters
        private void addParameter(SqlCommand command, string name, string value)
        {
            command.Parameters.Add(new SqlParameter(name, value));
        }
    }
}
