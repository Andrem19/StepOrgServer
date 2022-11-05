using AutoMapper;
using CloudinaryDotNet.Actions;
using StepOrg.DTOs;
using StepOrg.Entities.ModulesStruct.Ads;
using StepOrg.Entities.ModulesStruct.Payloads;
using StepOrg.Entities;

namespace StepOrg.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Group, GroupDto>();
            CreateMap<UserInGroup, UserInGroupDto>()
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));
            CreateMap<Payload, PayloadDto>();
            CreateMap<TaskItem, TaskItemDto>();
            CreateMap<Ad, AdDto>();
            CreateMap<Voting, VotingDto>();
            CreateMap<Variant, VariantDto>();
        }
    }
}
