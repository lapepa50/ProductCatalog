using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Models;

namespace ProductCatalog.Controllers
{
    public class ProductsController : Controller // controller for managing products in the application
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IWebHostEnvironment webHostEnvironment) 
            // constructor that takes an IWebHostEnvironment parameter to access the web host environment (used for getting the content root path for reading/writing the products.json file)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index() 
            // action method that returns a view displaying a list of products
        {
            var products = GetProducts();
            return View(products);
        }

        public IActionResult Details(int id) 
            // action method that returns a view displaying the details of a specific product based on its ID
        {
            var product = GetProducts()
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Create() 
            // action method that returns a view for creating a new product
        {
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product) 
            // action method that handles the form submission for creating a new product
            // it validates the model state, assigns a new ID to the product, adds it to the list of products, and saves the updated list to the products.json file
            // if the model state is invalid, it returns the view with the product data for correction
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var products = GetProducts();

            product.Id = products.Any() ? products.Max(p => p.Id) + 1 : 1;
            products.Add(product);
            SaveProducts(products);

            return RedirectToAction(nameof(Index));
        }

        private List<Product> GetProducts()
            // private method that reads the products.json file and deserializes its content into a list of product objects
            // if the file does not exist or is empty, it returns an empty list
        {
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "data", "products.json");

            if (!System.IO.File.Exists(filePath))
            {
                return new List<Product>();
            }

            var json = System.IO.File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Product>();
            }

            return JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Product>();
        }

        private void SaveProducts(List<Product> products) 
            // private method that serializes the list of product objects into JSON format and writes it to the products.json file
        {
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "data", "products.json");
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(products, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.IO.File.WriteAllText(filePath, json);
        }
    }
}