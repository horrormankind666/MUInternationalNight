using Dapper;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Web;

public partial class admin_start_ExportExcel : System.Web.UI.Page
{
    //private readonly string conn = "Data Source=stddb.mahidol;Initial Catalog=MUInternationalWeek;Persist Security Info=True;User ID=odd;Password='8;pd^.sPj,kd';";
    private readonly string conn = "Data Source=stddb.mahidol;Initial Catalog=MUInternationalWeek;Persist Security Info=True;User ID=MuStudent53;Password=oydL7dKk53;";
    private IDbConnection db { get { return new SqlConnection(conn); } }

    protected void Page_Load(object sender, EventArgs e)
    {
        string registeredYear = Request["selectyear"];
        dynamic data = db.Query<dynamic>("sp_GetRegisterReportInternightExportExcel", new { registeredYear }, commandType: CommandType.StoredProcedure);
        string jsonstring = JsonConvert.SerializeObject(data);
        DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonstring);
        ExportExcels(dt, registeredYear);
    }

    public void ExportExcels(DataTable dt, string _value)
    {
        using (ExcelPackage pck = new ExcelPackage())
        {
            //Create the worksheet
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("ลงทะเบียนปี " + _value);

            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
            ws.Cells["A1"].LoadFromDataTable(dt, true);

            //prepare the range for the column headers
            string cellRange = "A1:" + Convert.ToChar('A' + dt.Columns.Count - 1) + 1;

            //Format the header for columns
            using (ExcelRange rng = ws.Cells[cellRange])
            {
                rng.Style.WrapText = false;
                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                rng.Style.Fill.BackgroundColor.SetColor(Color.Black);
                rng.Style.Font.Color.SetColor(Color.White);
            }

            //prepare the range for the rows
            string rowsCellRange = "A2:" + Convert.ToChar('A' + dt.Columns.Count - 1) + dt.Rows.Count * dt.Columns.Count;

            //Format the rows
            using (ExcelRange rng = ws.Cells[rowsCellRange])
            {
                rng.Style.WrapText = false;
                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                rng.AutoFitColumns();
            }

            //Read the Excel file in a byte array
            Byte[] fileBytes = pck.GetAsByteArray();

            //Clear the response
            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Cookies.Clear();

            //Add the header & other information
            Response.Cache.SetCacheability(HttpCacheability.Private);
            Response.CacheControl = "private";
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            Response.AppendHeader("Content-Length", fileBytes.Length.ToString());
            Response.AppendHeader("Pragma", "cache");
            Response.AppendHeader("Expires", "60");
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=MUInterNightRegistration" + _value + ".xlsx");

            //Write it back to the client
            Response.BinaryWrite(fileBytes);
            Response.End();
        }
    }

    public void ExportExcelFixColumns(DataTable dt, string _value)
    {
        ExcelPackage excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add(_value);
        var totalCols = dt.Columns.Count;
        var totalRows = dt.Rows.Count;

        for (var col = 1; col <= totalCols; col++)
        {
            workSheet.Cells[1, col].Value = dt.Columns[col - 1].ColumnName;
        }
        for (var row = 1; row <= totalRows; row++)
        {
            for (var col = 0; col < totalCols; col++)
            {
                workSheet.Cells[row + 1, col + 1].Value = dt.Rows[row - 1][col];
            }
        }
        using (var memoryStream = new MemoryStream())
        {
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=" + _value + ".xlsx");
            excel.SaveAs(memoryStream);
            memoryStream.WriteTo(Response.OutputStream);
            Response.Flush();
            Response.End();
        }
    }
}