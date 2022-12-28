using authen;
using System;
using System.Data;
using System.Web;

public class Logins {
    public Logins(string types) {
        Finservice authen = new Finservice();

        try {
            Result = HttpContext.Current.Request.Cookies[types]["result"].ToString();
            DataSet ds = authen.info(Result);
            int row = ds.Tables[0].Rows.Count;

            if (row > 0) {
                UId = ds.Tables[0].Rows[0]["uid"].ToString();
                UserType = ds.Tables[0].Rows[0]["userType"].ToString();
                FullNameEn = ds.Tables[0].Rows[0]["fullNameEn"].ToString();
                FullNameTh = ds.Tables[0].Rows[0]["fullNameTh"].ToString();
                DepCode = ds.Tables[0].Rows[0]["depCode"].ToString();
                Authen = ds.Tables[0].Rows[0]["authen"].ToString();
                Message = ds.Tables[0].Rows[0]["msg"].ToString();
                Username = ds.Tables[0].Rows[0]["username"].ToString();
                StudentId = ds.Tables[0].Rows[0]["studentid"].ToString();
                StudentCode = ds.Tables[0].Rows[0]["studentcode"].ToString();
            }
        }
        catch (Exception ex) {
            UId = string.Empty;
            UserType = string.Empty;
            FullNameEn = string.Empty;
            FullNameTh = string.Empty;
            DepCode = string.Empty;
            Authen = "False";
            Username = string.Empty;
            Message = string.Format("Service error :{0}", ex.Message);
            StudentId = string.Empty;
            StudentCode = string.Empty;
        }
    }

    public string UId {
        get;
        private set;
    }

    public string UserType {
        get;
        private set;
    }

    public string FullNameTh {
        get;
        private set;
    }

    public string FullNameEn {
        get;
        private set;
    }

    public string Authen {
        get;
        private set;
    }

    public string Username {
        get;
        set;
    }

    public string StudentId {
        get;
        private set;
    }

    public string StudentCode {
        get;
        private set;
    }

    public string Message {
        get;
        private set;
    }

    public string DepCode {
        get;
        private set;
    }

    public string MailAddress {
        get;
        private set;
    }

    public string StrResult {
        get;
        private set;
    }

    public string Result {
        get;
        private set;
    }
}