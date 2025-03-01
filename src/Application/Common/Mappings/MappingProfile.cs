using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Domain.Entities.ContractPanel;

namespace Escrow.Api.Application.Common.Mappings
{
    class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MileStone, MileStoneDTO>().ReverseMap();
            CreateMap<ContractDetails, ContractDetailsDTO>().ReverseMap();
        }
    }
}
