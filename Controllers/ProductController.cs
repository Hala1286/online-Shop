using System.Collections.Generic;
using System.Threading.Tasks;
using CaseStudy.DAL;
using CaseStudy.DAL.DAO;
using CaseStudy.DAL.DomainClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseStudy.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        AppDbContext _db;
        public ProductController(AppDbContext context)
        {
            _db = context;
        }
        [Route("{braid}")]
        public  async Task< ActionResult<List<Product>>> Index(int braid)
        {
            ProductDAO dao = new ProductDAO(_db);
            List<Product> productsForBrand = await dao.GetAllByBrand(braid);
            return productsForBrand;
        }
    }
}
