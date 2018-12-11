using System.Collections.Generic;

namespace Models
{
    public abstract class Picture
    {
        public string Name;
        public string Date;
        public List<string> MetaTags;
    }
    public class DalPicture : Picture
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public byte[] Base64 { get; set; }
        public string FileExtension { get; set; }
    }

    public class LogicPicture : Picture
    {
        public int Index;
        public string Base64;
    }
}