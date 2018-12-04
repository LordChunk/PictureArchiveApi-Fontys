using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace DAL
{
    public class Picture
    {
        private readonly string _connectiongString;
        private readonly IHostingEnvironment _environment;
        public Picture(
            string ConnectionString,
            IHostingEnvironment environment
            )
        {
            _connectiongString = ConnectionString;
            _environment = environment;
        }

        public async Task<object> StorePicture(IFormFileCollection files, string userId)
        {

            List<string> fileNamesList = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Write file
                    string pictureGuid = WriteFile(file, userId);

                    // Add file location to list
                    fileNamesList.Add(pictureGuid);
                }
            }

            AddFilePathsToDb(fileNamesList, userId);
            return fileNamesList;
        }

        private void AddFilePathsToDb(List<string> fileNameList, string userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectiongString))
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

            void addParameter(SqlCommand command, string name, string value)
            {
                command.Parameters.Add(new SqlParameter(name, value));
            }
        }

        private string WriteFile(IFormFile file, string userId)
        {
            var fileName = string.Empty;
            var folderName = string.Empty;
            
            // Assigning Unique Filename (Guid)
            string pictureGuid = Convert.ToString(Guid.NewGuid());

            // Getting FileName
            fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString();
            
            // Getting file Extension
            var FileExtension = Path.GetExtension(fileName);

            // Concating  FileName + FileExtension + remove the \ character at the end of the string
            var newFileName = pictureGuid + FileExtension.Replace("\"", "");

            // Gets foldername for ensurefolder
            folderName = Path.Combine(_environment.WebRootPath, "userPictures", userId);

            // Combines two strings into a path.
            fileName = folderName + $@"\{newFileName}";

            EnsureFolder(fileName);

            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            // Return file name (can then be converted to location)
            return newFileName;
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
