namespace StepOrg.DTOs
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string GroupName { get; set; }
        public string? PictureUrl { get; set; }
        public List<UserInGroupDto> Users { get; set; }
        public bool IsPayloads { get; set; }
        public List<PayloadDto>? Payloads { get; set; }
        public bool IsAds { get; set; }
        public List<AdDto>? Ad { get; set; }
    }
}
