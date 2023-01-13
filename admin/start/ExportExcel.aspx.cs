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

public partial class admin_start_ExportExcel: System.Web.UI.Page {
    /*
    private readonly string conn = "Data Source=stddb.mahidol;Initial Catalog=MUInternationalWeek;Persist Security Info=True;User ID=odd;Password='8;pd^.sPj,kd';";
    */
    private readonly string conn = "Data Source=stddb.mahidol;Initial Catalog=MUInternationalWeek;Persist Security Info=True;User ID=MuStudent53;Password=oydL7dKk53;";
    private IDbConnection db {
        get {
            return new SqlConnection(conn);
        }
    }

    protected void Page_Load(
        object sender,
        EventArgs e
    ) {
        string registeredYear = Request["selectyear"];
        dynamic data = db.Query<dynamic>("sp_GetRegisterReportInternightExportExcel", new { registeredYear }, commandType: CommandType.StoredProcedure);
        string jsonstring = JsonConvert.SerializeObject(data);
        DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonstring);
        ExportExcels(dt, registeredYear);
    }

    public void ExportExcels(
        DataTable dt,
        string value
    ) {
        using (ExcelPackage pck = new ExcelPackage()) {
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("ลงทะเบียนปี " + value);

            ws.Cells["A1"].LoadFromDataTable(dt, true);

            string cellRange = "A1:" + Convert.ToChar('A' + dt.Columns.Count - 1) + 1;
            
            using (ExcelRange rng = ws.Cells[cellRange]) {
                rng.Style.WrapText = false;
                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                rng.Style.Fill.BackgroundColor.SetColor(Color.Black);
                rng.Style.Font.Color.SetColor(Color.White);
            }

            string rowsCellRange = "A2:" + Convert.ToChar('A' + dt.Columns.Count - 1) + dt.Rows.Count * dt.Columns.Count;

            using (ExcelRange rng = ws.Cells[rowsCellRange]) {
                rng.Style.WrapText = false;
                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                rng.AutoFitColumns();
            }

            Byte[] fileBytes = pck.GetAsByteArray();

            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Cookies.Clear();
            Response.Cache.SetCacheability(HttpCacheability.Private);
            Response.CacheControl = "private";
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            Response.AppendHeader("Content-Length", fileBytes.Length.ToString());
            Response.AppendHeader("Pragma", "cache");
            Response.AppendHeader("Expires", "60");
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=MUInterNightRegistration" + value + ".xlsx");
            Response.BinaryWrite(fileBytes);
            Response.End();
        }
    }

    public void ExportExcelFixColumns(
        DataTable dt,
        string value
    ) {
        ExcelPackage excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add(value);
        var totalCols = dt.Columns.Count;
        var totalRows = dt.Rows.Count;

        for (var col = 1; col <= totalCols; col++) {
            workSheet.Cells[1, col].Value = dt.Columns[col - 1].ColumnName;
        }

        for (var row = 1; row <= totalRows; row++) {
            for (var col = 0; col < totalCols; col++) {
                workSheet.Cells[row + 1, col + 1].Value = dt.Rows[row - 1][col];
            }
        }

        using (var memoryStream = new MemoryStream()) {
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=" + value + ".xlsx");
            excel.SaveAs(memoryStream);
            memoryStream.WriteTo(Response.OutputStream);
            Response.Flush();
            Response.End();
        }
    }
}