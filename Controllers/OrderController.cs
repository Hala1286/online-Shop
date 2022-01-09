using System;
using CaseStudy.DAL;
using CaseStudy.DAL.DAO;
using CaseStudy.DAL.DomainClasses;
using CaseStudy.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using System.Collections.Generic;
namespace CaseStudy.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        AppDbContext _ctx;
        public OrderController(AppDbContext context) // injected here
        {
            _ctx = context;
        }
        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<string>> Index(OrderHelper helper)
        {
            string retVal = "";
            try
            {
                CustomerDAO cDao = new CustomerDAO(_ctx);
                Customer cartOwner = await cDao.GetByEmail(helper.email);
                OrderDAO tDao = new OrderDAO(_ctx);
                var order = await tDao.AddOrder(cartOwner.Id, helper.selections);
                if (order.orderId > 0 && order.isBackOrdered == true)
                {
                    retVal = "order " + order.orderId + " Created!" + "Goods backordered!";
                }
                else if (order.orderId > 0 && order.isBackOrdered == false)
                {
                    retVal = "order " + order.orderId + " Created!";
                }
                else
                {
                    retVal = "order not created";
                }
            }
            catch (Exception ex)
            {
                retVal = "Order not created " + ex.Message;
            }
            return retVal;
        }

        [Route("{email}")]
        public async Task<ActionResult<List<Order>>> List(string email)
        {
            List<Order> orders = new List<Order>();
            CustomerDAO cDao = new CustomerDAO(_ctx);
            Customer orderOwner = await cDao.GetByEmail(email);
            OrderDAO oDao = new OrderDAO(_ctx);
            orders = await oDao.GetAll(orderOwner.Id);
            return orders;
        }


        [Route("{orderid}/{email}")]
        public async Task<ActionResult<List<OrderDetailsHelper>>> GetOrderDetails(int orderid, string email)
        {
            OrderDAO dao = new OrderDAO(_ctx);
            return await dao.GetOrderDetails(orderid, email);
        }
    }
}
