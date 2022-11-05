using StepOrg.Entities.ModulesStruct.Ads;
using StepOrg.Entities.ModulesStruct.Payloads;
using System.ComponentModel.DataAnnotations;

namespace StepOrg.Entities
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string? PictureUrl { get; set; }
        public string? PicturePublicId { get; set; }
        public List<UserInGroup> UsersInGroup { get; set; } = new();
        public bool IsAds { get; set; } = true;
        public List<Ad> Ads { get; set; }
        public bool IsPayloads { get; set; } = false;
        public List<Payload> Payloads { get; set; }
    }
}
