using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using PictureArchiveApi;

namespace Logic.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        private readonly DAL.Picture _dalPicture;
        private readonly UserManager<IdentityUser> _userManager;

        public PictureController(
            UserManager<IdentityUser> userManager
        )
        {
            _userManager = userManager;
            _dalPicture = new DAL.Picture(Startup.ConnectionString, Startup.AzureStorageConnectionString);
        }

        // api/picture
        [Authorize]
        [HttpGet]
        public List<string> Index(int amount = 0, int offset = 0)
        {
            List<string> referencesList = new List<string>();

            // Convert picture element to URL to image
            foreach (DalPicture picture in _dalPicture.GetPictures(amount, offset))
            {
                referencesList.Add($"/{picture.UserId}/{picture.Id}.{picture.FileExtension}");
            }

            return referencesList;
        }

        [HttpPost]
        [Route("upload")]
        [Authorize]
        public async Task<List<string>> UploadToBlobAsync([FromBody]List<LogicPicture> pictures)
        {
            if(pictures.Count > 0)
            {
                // Get user ID
                string token = Request.Headers.GetCommaSeparatedValues("Authorization").First().Remove(0, 7);

                string email = new JwtSecurityTokenHandler().ReadJwtToken(token).Subject;

                IdentityUser user = new IdentityUser
                {
                    Email = email,
                    UserName = email
                };

                user = await _userManager.GetUserAsync(HttpContext.User);

                // Convert LogicPicture to DalPicture
                List<DalPicture> dalPictures = new List<DalPicture>();
                foreach(LogicPicture picture in pictures)
                {
                    string fileExtension = Regex.Match(picture.Base64, @"(?<=\/)(.*)(?=;)").Groups.FirstOrDefault().Value;
                    string id = Convert.ToString(Guid.NewGuid());
                    byte[] base64 = Convert.FromBase64String(picture.Base64.Split(',')[1]);

                    DalPicture dalPicture = new DalPicture()
                    {
                        Name = picture.Name,
                        UserId = user.Id,
                        Id = id,
                        Base64 = base64,
                        MetaTags = picture.MetaTags,
                        Date = picture.Date,
                        FileExtension = fileExtension,
                    };

                    dalPictures.Add(dalPicture);
                }

                return await _dalPicture.StorePictureInBlobStorage(dalPictures);
            }

            return null;
        }
    }
}