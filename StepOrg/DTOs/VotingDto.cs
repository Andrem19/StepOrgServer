namespace StepOrg.DTOs
{
    public class VotingDto
    {
        public List<VariantDto> Variants { get; set; }
        public bool IsSecret { get; set; }
    }
}
