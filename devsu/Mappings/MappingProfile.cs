using AutoMapper;
using devsu.DTOs;
using devsu.Models;

namespace devsu.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Cliente, ClienteDto>()
                .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.Id));
            CreateMap<ClienteDto, Cliente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Cuenta, CuentaDto>()
                .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente.Nombre));
            CreateMap<CuentaDto, Cuenta>();

            CreateMap<Movimiento, MovimientoDto>()
                .ForMember(dest => dest.NumeroCuenta, opt => opt.MapFrom(src => src.Cuenta.NumeroCuenta));
            CreateMap<MovimientoDto, Movimiento>();
        }
    }
}