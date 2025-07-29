using Kesawa_Data_Access.Data;
using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kesawa_Data_Access.Repository
{
    public class ProductOrderRepository : Repository<ProductOrder>, IProductOrderRepository
    {
        public ProductOrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
