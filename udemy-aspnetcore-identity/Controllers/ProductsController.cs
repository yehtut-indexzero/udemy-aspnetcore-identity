using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using udemy_aspnetcore_identity.Models;

namespace udemy_aspnetcore_identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {


        [Route(template:"List")]
        [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        public List<Product> GetList()
        {
            var chair = new Product()
            {
                Name = "chari",
                Price = 50
            };
            var desk = new Product()
            {
                Name = "Desk",
                Price = 50
            };

            return new List<Product> { chair, desk };
        }
    }
}
