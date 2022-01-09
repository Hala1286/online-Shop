using CaseStudy.Helpers;
using CaseStudy.DAL.DomainClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace CaseStudy.DAL.DAO
{
    public class OrderDAO
    {
        private AppDbContext _db;
       
        public OrderDAO(AppDbContext ctx)
        {
            _db = ctx;
        }
        public async Task<(int orderId , bool isBackOrdered)> AddOrder(int userid, OrderSelectionHelper[] selections)
        {
              
              int orderId = -1;
            bool isBackOrdered = false;
            using (_db)
            {
                // we need a transaction as multiple entities involved
                using (var _trans = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        Order order = new Order();
                        order.OrderDate = System.DateTime.Now;
                        order.UserId = userid;
                        order.OrderAmount = 0;
                    
                            foreach (OrderSelectionHelper selection in selections)
                        {
                            order.OrderAmount += (decimal)selection.Qty * selection.product.MSRP;
                        }
                        await _db.Orders.AddAsync(order);
                        await _db.SaveChangesAsync();
                        ProductDAO proDAO = new ProductDAO(_db);
                       
                        foreach (OrderSelectionHelper selection in selections)
                    {
                        OrderLineItem oItem = new OrderLineItem();
                        oItem.OrderId = order.Id;
                        oItem.SellingPrice = (decimal)selection.product.MSRP;
                        oItem.ProductId = selection.product.Id;
                        oItem.QtyOrdered = selection.Qty;
                            if (oItem.QtyOrdered <= selection.product.QtyOnHand)
                            {

                                Product product = await proDAO.GetProduct(selection.product.Id);
                                product.QtyOnHand -= selection.Qty;
                                oItem.QtySold = selection.Qty;
                                oItem.QtyBackOrdered = 0;
                                isBackOrdered = false;

                            }
                            else 
                            {
                                Product product = await proDAO.GetProduct(selection.product.Id);
                                oItem.QtyBackOrdered = (selection.Qty - product.QtyOnHand);
                                oItem.QtySold = product.QtyOnHand;
                                oItem.QtyOrdered = selection.Qty;
                                product.QtyOnHand = 0;
                               
                              product.QtyOnBackOrder += (selection.Qty - selection.product.QtyOnHand);
                                isBackOrdered = true;

                        }
                        await _db.OrderLineItems.AddAsync(oItem);
                        await _db.SaveChangesAsync();
                    }
                   
                    await _trans.CommitAsync();
                    orderId = order.Id;
                }
                 catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await _trans.RollbackAsync();
                }
            }
        }
          return (orderId , isBackOrdered) ;
        }


        public async Task<List<Order>> GetAll(int id)
        {
            return await _db.Orders.Where(order=> order.UserId == id).ToListAsync<Order>();
        }

        public async Task<List<OrderDetailsHelper>> GetOrderDetails(int tid, string email)
        {
            Customer customer = _db.Customers.FirstOrDefault(customer => customer.Email == email);
            List<OrderDetailsHelper> allDetails = new List<OrderDetailsHelper>();
            // LINQ way of doing INNER JOINS
            var results = from o in _db.Orders
                          join oli in _db.OrderLineItems on o.Id equals oli.OrderId
                          join pro in _db.Products on oli.ProductId equals pro.Id
                          where (o.UserId == customer.Id && o.Id == tid)
                          select new OrderDetailsHelper
                          {

                              OrderId = o.Id,
                             CustomerId = customer.Id,
                              QtyO = oli.QtyOrdered,
                              Cost =pro.MSRP,
                              ProductId = oli.ProductId,
                              QtyB = oli.QtyBackOrdered,
                              QtyS = oli.QtySold ,
                              Name = pro.ProductName,
                              DateCreated = o.OrderDate.ToString("yyyy/MM/dd - hh:mm tt")
                          };
            allDetails = await results.ToListAsync<OrderDetailsHelper>();
            return allDetails;
        }
    }
}
