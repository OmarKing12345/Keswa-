using Keswa_Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kesawa_Data_Access.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        IQueryable<Product> GetQuery(); 
    }
}