using authen;
using System.Text;
using System.Web;

public class Authen {
    public static void Verify(
        string username,
        string password,
        string types
    ) {
        Finservice service = new Finservice();
        HttpCookie httpcookie = new HttpCookie(types);
        string user = service.XmlAuthen(username, password, types);
        httpcookie["result"] = ConvertAscii(user);
        HttpContext.Current.Response.Cookies.Add(httpcookie);
    }

    public static string ConvertAscii(string data) {
        string result = string.Empty;

        if (!string.IsNullOrEmpty(data)) {
            Encoding enc = Encoding.GetEncoding("TIS-620");
            byte[] arrByte = enc.GetBytes(data.ToCharArray());

            foreach (byte b in arrByte)
                result += b.ToString() + "@#@";
        }

        return result;
    }
}