using AutoMapper;
using Keswa_Entities.Dtos.Response;
using Keswa_Entities.Models;
using Keswa_Project.Keswa_Entities.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa_Entities.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductResponse>();

            CreateMap<ProductImage, ProductImageResponse>();

            
            CreateMap<Category, CategoryHomeResponse>();
            
            CreateMap<Brand, BrandHomeResponse>();


        }
    }
}
