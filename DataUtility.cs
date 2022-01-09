using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CaseStudy.DAL.DomainClasses;
namespace CaseStudy.DAL
{
    public class DataUtility
    {
        private AppDbContext _db;
        public DataUtility(AppDbContext context)
        {
            _db = context;
        }
        public async Task<bool> loadProductsInfoFromWebToDb(string stringJson)
        {
            bool brandsLoaded = false;
            bool productsLoaded = false;
            try
            {
                // an element that is typed as dynamic is assumed to support any operation
                dynamic objectJson = JsonSerializer.Deserialize<Object>(stringJson);
                brandsLoaded = await loadBrands(objectJson);
                productsLoaded = await loadProducts(objectJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return brandsLoaded && productsLoaded;
        }
        private async Task<bool> loadBrands(dynamic jsonObjectArray)
        {
            bool loadedBrands = false;
            try
            {
                // clear out the old rows
                _db.Brands.RemoveRange(_db.Brands);
                await _db.SaveChangesAsync();
                List<String> allBrands = new List<String>();
                foreach (JsonElement element in jsonObjectArray.EnumerateArray())
                {
                    if (element.TryGetProperty("BRAND", out JsonElement productJson))
                    {
                        allBrands.Add(productJson.GetString());
                    }
                }
                IEnumerable<String> categories = allBrands.Distinct<String>();
                foreach (string catname in categories)
                {
                    Brand bra = new Brand();
                    bra.Name = catname;
                    await _db.Brands.AddAsync(bra);
                    await _db.SaveChangesAsync();
                }
                loadedBrands = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - brand" + ex.Message);
            }
            return loadedBrands;
        }
        private async Task<bool> loadProducts(dynamic jsonObjectArray)
        {
            bool loadedItems = false;
            try
            {
                List<Brand> brands = _db.Brands.ToList();
                // clear out the old
                _db.Products.RemoveRange(_db.Products);
                await _db.SaveChangesAsync();
                foreach (JsonElement element in jsonObjectArray.EnumerateArray())
                {

       




        Product item = new Product();
                    item.Id = element.GetProperty("Id").GetString();
                    item.ProductName = element.GetProperty("ProductName").GetString();
                    item.GraphicName = element.GetProperty("GraphicName").GetString();
                    item.CostPrice = element.GetProperty("CostPrice").GetDecimal();
                    item.MSRP = element.GetProperty("MSRP").GetDecimal();
                    item.QtyOnHand = element.GetProperty("QtyOnHand").GetInt32();
                    item.QtyOnBackOrder = element.GetProperty("QtyOnBackOrder").GetInt32();
                    item.Description = element.GetProperty("Description").GetString();
                    string bra = element.GetProperty("BRAND").GetString();
                    // add the FK here
                    foreach (Brand brand in brands)
                    {
                        if (brand.Name == bra)
                        {
                            item.Brand = brand;
                            break;
                        }
                    }
                    await _db.Products.AddAsync(item);
                    await _db.SaveChangesAsync();
                }
                loadedItems = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error -product " +  ex.Message);
            }
            return loadedItems;
        }



    }
}
