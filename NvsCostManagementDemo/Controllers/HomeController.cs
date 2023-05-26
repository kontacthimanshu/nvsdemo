using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using NvsCostManagementDemo.Models;
using System.Diagnostics;

namespace NvsCostManagementDemo.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, ITokenAcquisition tokenAcquisition, IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient();
            var scope = "https://management.azure.com/.default";
            var accessToken = await _tokenAcquisition.GetAccessTokenForAppAsync(scope);

            // Set your Azure subscription ID and resource group name
            string subscriptionId = "";
            string resourceGroupName = "";

            // Set the Azure Cost Management API endpoint
            string endpoint = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.CostManagement/query?api-version=2023-03-01";

            // Set the Azure Cost Management API query
            string query = $"{{\\n  \\\"type\\\": \\\"Usage\\\",\\n  \\\"timeframe\\\": \\\"MonthToDate\\\",\\n  \\\"dataset\\\": {{\\n    \\\"granularity\\\": \\\"Daily\\\",\\n    \\\"filter\\\": {{\\n      \\\"and\\\": [\\n        {{\\n          \\\"or\\\": [\\n            {{\\n              \\\"dimensions\\\": {{\\n                \\\"name\\\": \\\"ResourceLocation\\\",\\n                \\\"operator\\\": \\\"In\\\",\\n                \\\"values\\\": [\\n                  \\\"East US\\\",\\n                  \\\"West Europe\\\"\\n                ]\\n              }}\\n            }},\\n            {{\\n              \\\"tags\\\": {{\\n                \\\"name\\\": \\\"Environment\\\",\\n                \\\"operator\\\": \\\"In\\\",\\n                \\\"values\\\": [\\n                  \\\"UAT\\\",\\n                  \\\"Prod\\\"\\n                ]\\n              }}\\n            }}\\n          ]\\n        }},\\n        {{\\n          \\\"dimensions\\\": {{\\n            \\\"name\\\": \\\"ResourceGroup\\\",\\n            \\\"operator\\\": \\\"In\\\",\\n            \\\"values\\\": [\\n              \\\"API\\\"\\n            ]\\n          }}\\n        }}\\n      ]\\n    }}\\n  }}\\n}}";

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Make the API call
            HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(query));

            // Check the response status
            if (response.IsSuccessStatusCode)
            {
                // Read the response content
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine(jsonResponse);
            }
            else
            {
                Console.WriteLine($"API call failed with status code: {response.StatusCode}");
            }


            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}