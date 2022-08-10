using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.OleDb;
using WelderTracker150722.Data;
using WelderTracker150722.Models;

namespace WelderTracker150722.Controllers
{
    public class UploadController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private IWebHostEnvironment Environment;
        private IConfiguration Configuration;
        private readonly ApplicationDbContext _context;
        public UploadController(ILogger<HomeController> logger, IWebHostEnvironment _environment, IConfiguration _configuration, ApplicationDbContext context)
        {
            _logger = logger;
            Environment = _environment;
            Configuration = _configuration;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var orders = from o in _context.ExcelModels
                         select o;
            return View(await orders.ToListAsync());
        }

        //turned out that OleDB is not happy on azure - I renamed the method name to Upload to keep it for possible future tests-
        //Instead of OleDB i will use ExcelReader: the method bellow this one (post index)
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile postedFile)
        {
            if (postedFile != null)
            {
                //Create a Folder.
                string path = Path.Combine(this.Environment.WebRootPath, "Uploads");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                //Save the uploaded Excel file.
                string fileName = Path.GetFileName(postedFile.FileName);
                string filePath = Path.Combine(path, fileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }

                //Read the connection string for the Excel file.
                string conString = this.Configuration.GetConnectionString("ExcelConString");
                DataTable dt = new DataTable();
                conString = string.Format(conString, filePath);

                using (OleDbConnection connExcel = new OleDbConnection(conString))
                {
                    using (OleDbCommand cmdExcel = new OleDbCommand())
                    {
                        using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                        {
                            cmdExcel.Connection = connExcel;

                            //Get the name of First Sheet.
                            connExcel.Open();
                            DataTable dtExcelSchema;
                            dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                            connExcel.Close();

                            //Read Data from First Sheet.
                            connExcel.Open();
                            cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
                            odaExcel.SelectCommand = cmdExcel;
                            odaExcel.Fill(dt);
                            connExcel.Close();
                        }
                    }

                }

                //Insert the Data read from the Excel file to Database Table.
                // conString = this.Configuration.GetConnectionString("DefaultConnection");  //works with this
                conString = this.Configuration.GetConnectionString("AzureContext"); //doesn't work on azure
                using (SqlConnection con = new SqlConnection(conString))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        //Set the database table name.
                        // sqlBulkCopy.DestinationTableName =  "dbo.ExcelModels"; //maybe it makes sense to pass it straith to the main page?
                        sqlBulkCopy.DestinationTableName = "dbo.Items";

                        //[OPTIONAL]: Map the Excel columns with that of the database table.
                        sqlBulkCopy.ColumnMappings.Add("FirNumber", "FirNumber");
                        sqlBulkCopy.ColumnMappings.Add("ItemName", "ItemName");
                        sqlBulkCopy.ColumnMappings.Add("Amount", "Amount");
                        sqlBulkCopy.ColumnMappings.Add("ToCompleteBy", "ToCompleteBy");

                        con.Open();
                        sqlBulkCopy.WriteToServer(dt);
                        con.Close();
                    }
                }
            }
            var orders = from o in _context.ExcelModels
                         select o;
            return View(await orders.ToListAsync());
        }
        //Using ExcelReader to upload the excel file
        [HttpPost]
        public async Task<IActionResult> Index(IFormFile postedFile)
        {
            // Save the uploaded Excel file.
            if (postedFile != null)
            {
                //Create a Folder.
                string path = Path.Combine(this.Environment.WebRootPath, "Uploads");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                //Save the uploaded Excel file.
                string fileName = Path.GetFileName(postedFile.FileName);
                string filePath = Path.Combine(path, fileName);
                FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                /*using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }
                */
              
                DataTable dt = new DataTable();
                
                IExcelDataReader reader = null;
                reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                DataSet result = reader.AsDataSet(); //should change to datatable
                reader.Close();
                // ExcelModel excelData = new ExcelModel();
                DataTable dt2 = result.Tables[0];
                IList<ExcelModel> excelModels = dt2.AsEnumerable().Select(row => new ExcelModel
                {
                    FirNumber = row.Field<string>("Column0"),
                    ItemName = row.Field<string>("Column1"),
                    Amount = (int)row.Field<double>("Column2"),
                    ToCompleteBy = (row.Field<DateTime>("Column3")).ToString()
                }).ToList();
                
                foreach (ExcelModel o in excelModels)
                {
                    _context.ExcelModels.Add(o);
                    await _context.SaveChangesAsync();
                    
                }
                var orders = from o in _context.ExcelModels
                             select o;
                return View(await orders.ToListAsync());
               

            }
            var originalorders = from o in _context.ExcelModels
                                 select o;
            return View(await originalorders.ToListAsync());
          
        }
    }
}

