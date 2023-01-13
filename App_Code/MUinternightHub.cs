using Dapper;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public class MuinternightHub: Hub {
    #region ค่าเริ่มต้น

    private readonly string conn = @"Data Source=stddb.mahidol;Initial Catalog=MUInternationalWeek;Persist Security Info=True;User ID=MuStudent53;Password=oydL7dKk53;";

    private IDbConnection Db {
        get {
            return new SqlConnection(conn);
        }
    }

    public string Querys {
        get;
        private set;
    }

    public dynamic Dappers {
        get;
        private set;
    }

    public readonly CommandType proc = CommandType.StoredProcedure;
    public readonly CommandType texts = CommandType.Text;
    private static int eventYear = 2023;
    private static int user = 0;

    #endregion ค่าเริ่มต้น

    #region connect status

    public override Task OnConnected() {
        user++;
        Clients.All.ConnectFromServer(String.Format("Online"));

        return base.OnConnected();
    }

    public override Task OnDisconnected(bool stopCalled) {
        user--;
        user = (user < 2 ? 1 : user--);

        if (stopCalled)  {
            Clients.Caller.ConnectFromServer(String.Format("Explicitly"));
            Clients.All.OnlineUserFromServer(user);
        }
        else {
            Clients.Caller.ConnectFromServer(String.Format("Timeout"));
            Clients.All.OnlineUserFromServer(user);
        }

        return base.OnDisconnected(stopCalled);
    }

    public override Task OnReconnected() {
        Clients.Caller.ConnectFromServer(String.Format("Online"));

        return base.OnReconnected();
    }

    public void OnlineUserToServer() {
        Clients.All.OnlineUserFromServer(user);
    }

    #endregion connect status

    #region server time

    public void GetServerTime() {
        while (true) {
            Clients.Caller.SetTime(DateTime.Now.ToString("h:mm:ss tt"));
            Thread.Sleep(10000);
        }
    }

    public void Start() {
        Thread thread = new Thread(GetServerTime);
        thread.Start();
    }

    public void CurrentyearToServer() {
        Clients.Caller.CurrentyearFromServer(eventYear);
    }

    #endregion server time

    #region front end

    /*
    open date range
    */
    public void OpenDateToServer() {
        Querys = @"sp_GetRegisterDateInternight";
        Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear }, commandType: proc)).Result;
        Clients.Caller.OpenDateFromServer(Dappers);
    }

    /*
    load faculty
    */
    public void FacultyToServer() {
        Querys = (
            @"
            select   id,
                     facultycode,
                     facultyname
            from     Faculty
            order by facultycode
            "
        );
        Dappers = (Db.QueryAsync<dynamic>(Querys, commandType: texts)).Result;
        Clients.Caller.FacultyFromServer(Dappers);
    }

    /*
    load country
    */
    public void CountriesToServer() {
        Querys = (
            @"
            select   id,
                     countryNameEN
            from     Infinity..plcCountry
            order by countryNameEN
            "
        );
        Dappers = (Db.QueryAsync<dynamic>(Querys, commandType: texts)).Result;
        Clients.Caller.CountriesFromServer(Dappers);
    }

    /*
    limit thai people
    */
    public void LimitThaiPeopleToServer() {
        Querys = (
            @"
            declare @opendate datetime = (select top 1 opendate from interconfig where (eventtype = 'Internight') and (Years = @eventYear) order by Years desc)

            select  (150 - count(*)) as total
            from    international
            where   (CreateDate between @opendate and dateadd(day, 1, getdate())) and
                    (country = 217)
            "
        );
        Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear }, commandType: texts)).Result;
        Clients.All.LimitThaiPeopleFromServer(Dappers);
    }

    /*
    save register
    */
    public void RegisterToServer(dynamic data) {
        /*
        load ondate and location
        */
        string livedate = string.Empty, location = string.Empty;

        Querys = @"sp_GetRegisterDateInternight";
        Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear }, commandType: proc)).Result;

        if (Dappers.Count > 0) {
            livedate = Dappers[0].livedate;
            location = Dappers[0].location;

            /*
            register and create register-code
            */
            Profile profiles = new Profile();
            profiles = JsonConvert.DeserializeObject<Profile>(data);
            Querys = "sp_SetRegistrationInternight";
            Dappers = (Db.QueryAsync<dynamic>(Querys, new {
                eventYear,
                profiles.IdCard,
                profiles.TitleName,
                profiles.TitleNameOther,
                profiles.Firstname,
                profiles.MiddleName,
                profiles.Lastname,
                profiles.Country,
                profiles.StatusMU,
                profiles.StudentId,
                profiles.StudentDegree,
                profiles.StudentDegreeOther,
                profiles.StaffPosition,
                profiles.FacultyPosition,
                profiles.OtherPosition,
                profiles.Section,
                profiles.Email,
                profiles.Facebook,
                profiles.Food,
                profiles.IpAddress
            }, commandType: proc)).Result;

            /*
            recheck for best titlename and fullname
            */
            string title = !string.IsNullOrEmpty(profiles.TitleNameOther) ? profiles.TitleNameOther : profiles.TitleName;
            string middlename = !string.IsNullOrEmpty(profiles.MiddleName) ? profiles.MiddleName : " ";
            string fullName = String.Format("{0} {1}{2}{3}", title, profiles.Firstname, middlename, profiles.Lastname);

            /*
            ส่งเมล์เฉพาะครั้งแรกที่ ลงทะเบียน
            */
            if (!string.IsNullOrEmpty(Dappers[0].runningCode) &&
                Dappers[0].found.Equals(0))
                SendEmailActivate(Dappers[0].runningCode, eventYear.ToString(), fullName, profiles.Email, livedate, location);

            Clients.Caller.RegisterFromServer(Dappers);
            LimitThaiPeopleToServer();
            RegisteredTableToServer();
            RegisteredTableExcelToServer(string.Empty);
        }
        else
            Clients.Caller.RegisterFromServer(null);
    }

    /*
    load registered list
    */
    public void RegisteredTableToServer() {
        /*
        Querys = (
            @"
            declare @opendate datetime = (select top 1 opendate from [InterConfig] where eventtype='Internight' order by Years desc);
            declare @closedate datetime = (select top 1 DATEADD(day,1,ondate) from [InterConfig] where eventtype='Internight' order by Years desc);

            select   (a.section + right('000'+ convert(varchar(4),ROW_NUMBER() OVER (PARTITION BY a.section ORDER By section, a.id)),3)) rn,
                     (case when a.titlename!='Other' then a.titlename else a.titleNameOther end) as totaltitle,
					 isnull(a.firstName,'') + ' ' +isnull(a.middleName,'') + ' ' + isnull(a.lastName,'') as fullname,
                     b.countryNameEN,
					 a.section,
                     CONVERT(VARCHAR(10), a.createdate, 103) +', '+ CONVERT(VARCHAR(8), a.createdate, 24) AS [createdatex]
            from     [MUInternationalWeek].[dbo].[international] as a left join
                     [Infinity].[dbo].[plcCountry] as b on a.country = b.id
            where    (a.createdate between @opendate and @closedate) and
                     (a.eventtype = 'Internight')
            order by a.createdate desc
            "
        );
        */
        Querys = (
            @"
            declare @opendate datetime = (select top 1 opendate from InterConfig where (eventtype = 'Internight') and (Years = @eventYear) order by Years desc);
            declare @closedate datetime = (select top 1 dateadd(day, 1, ondate) from InterConfig where (eventtype = 'Internight') and (Years = @eventYear) order by Years desc);
                    
            select   a.runningCode as rn,
                     (case when a.titlename != 'Other' then a.titlename else a.titleNameOther end) as totaltitle,
					 (isnull(a.firstName, '') + ' ' + isnull(a.middleName, '') + ' ' + isnull(a.lastName, '')) as fullname,
                     b.countryNameEN,
					 a.section,
                     (convert(varchar(10), a.createdate, 103) + ', ' + convert(varchar(8), a.createdate, 24)) as createdatex
            from     MUInternationalWeek..international as a left join
                     Infinity..plcCountry as b on a.country = b.id
            where    (a.createdate between @opendate and @closedate) and 
                     (a.eventtype = 'Internight')
            order by a.createdate desc
            "
        );

        Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear }, commandType: texts)).Result;
        Clients.All.RegisteredTableFromServer(Dappers);
    }

    /*
    search studentCode
    */
    public void SearchStudentCodeToServer(string studentCode) {
        Querys = @"sp_GetStudentProfile";
        Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear, studentCode }, commandType: proc)).Result;
        Clients.Caller.SearchStudentCodeFromServer(Dappers);
    }

    /*
    send email to staff
    */
    public void SendEmailToServer(
        string name,
        string email,
        string message
    ) {
        message = String.Format("From {0} ( {1} )<br> {2}", name, email, message);
        EmailBroadcast("naowarat.imj@mahidol.ac.th", "Mahidol University International Night Question", message);
        Clients.Caller.SendEmailFromServer("completed");
    }

    /*
    send email to people
    */
    public static string SendEmailActivate(
        string registrationCode,
        string years,
        string fullName,
        string receiverMail,
        string livedate,
        string location
    ) {
        string subject = ("Mahidol University International Night " + years + " QR Code Registration");
        string body = BodyEmailActivate(registrationCode, years, fullName, livedate, location);

        return EmailBroadcast(receiverMail, subject, body);
    }

    /*
    email body
    */
    public static string BodyEmailActivate(
        string registrationCode,
        string years,
        string fullName,
        string livedate,
        string location
    ) {
        string body = string.Empty;

        body += String.Format("<div style='border:1px solid #52a6f9;padding: 10px; background-color: #e8f1f9; border-radius: 10px;'>");
        body += String.Format("<h2><u style='color:#363F9C'>Mahidol University International Night</u>&nbsp;</h2>");
        body += String.Format("&#128515; <b>Dear</b> {0} ", fullName);
        body += String.Format("<br>");
        body += String.Format("<br>");
        body += String.Format("<span style='color:#1A6600'>");
        body += String.Format("<div>");
        body += String.Format("<b>Thank you for your registration ");
        body += String.Format("to attend Mahidol University International Night {0}.</b>", years);
        body += String.Format("</div>");
        body += String.Format("<br>");
        body += String.Format("<div style='color:red'>Please use the “Registration Code” and “QR Code” sent to your email to register at the venue on <b>{0}</b>.</div>", livedate);
        /*
        body += String.Format("<br>");
        body += String.Format("<br>");
        body += String.Format("<div><b style='color:black'>We invite you to wear your national dress and be apart of the Mr. and Miss International Night Contest.</b></div>");
        body += String.Format("<br>");
        body += String.Format("<br>");
        body += String.Format("&#128353; Please use the registration code to register at the venue  <b>{0}</b>", livedate);
        */
        body += String.Format("<br>");
        body += String.Format("<b>Location</b> {0} <b><a target='_blank' href='https://www.google.com/maps/place/%E0%B8%84%E0%B8%93%E0%B8%B0%E0%B8%A8%E0%B8%B4%E0%B8%A5%E0%B8%9B%E0%B8%A8%E0%B8%B2%E0%B8%AA%E0%B8%95%E0%B8%A3%E0%B9%8C/@13.7961807,100.3211662,17.44z/data=!4m5!3m4!1s0x30e2938bb44e1dad:0x39ca370247bf6f24!8m2!3d13.7973142!4d100.3212985'>View Google Map</a></b>", location);
        body += String.Format("</span>");
        body += String.Format("<br>");        
        body += String.Format("<br>");
        body += String.Format("<br>");
        body += String.Format("<div style='border:1px solid #000000;background-color: #ffffff; padding: 1em 2em 1em 2em; border-radius: 10px;'>");
        body += String.Format("<br>");
        body += String.Format("<div style='font-size:20px'>&#128073; <b style='color:#960000'>Registration Code : </b> {0}</div>", registrationCode);
        body += String.Format("<div style='font-size:20px'>&#128073; <b style='color:#960000'>Your QR Code Link : </b>");
        body += String.Format("<a target='_blank' href='https://smartedu.mahidol.ac.th/MuInternight/start/index.cshtml?qr={0}'>QR Code</a></div>", registrationCode);
        body += String.Format("<br>");
        body += String.Format("</div>");
        body += String.Format("<br>");
        body += String.Format("<br>");
        body += String.Format("<img src='https://smartedu.mahidol.ac.th/muinternight/assets/img/bg/poster.png' style='background-color:#af952a; width:77%;height:auto;display: block;margin-left: auto;margin-right: auto;border:1px solid #333; border-radius: 5px;padding: 3px;'>");
        body += String.Format("<br>");

        body += String.Format("<span style='color:#363F9C'>Best Regards</span>");
        body += String.Format("<br>");
        body += String.Format("<br>");
        body += String.Format("<br>Mahidol University Internaitonal Night Staff");
        body += String.Format("<br>");
        body += String.Format("<br>");
        body += String.Format("<hr>");
        body += String.Format("<b style='color:#a86030'>This is an automated message, Please do not reply direct to this mail</b><br>");
        body += String.Format("<b style='color:#2c9943'><a target='_blank' href='https://smartedu.mahidol.ac.th/muinternight/start/index.cshtml'>Website: Mahidol University International Night</a></b><br>");
        body += String.Format("<br>");
        body += String.Format("</div>");

        return body;
    }

    /*
    email engine
    */
    public static string EmailBroadcast(
        string receiverMail,
        string subject,
        string body
    ) {
        string senderMail = "studentconsult@mahidol.ac.th"; // sender mail
        string password = "503eexvg";
        string message = "failed";
        /*
        receiverMail = "naowarat.imj@mahidol.ac.th"; // test sendmail 1
        receiverMail = "nopparat.jap@mahidol.ac.th"; // test sendmail 2
        receiverMail = "yutthaphoom.taw@mahidol.ac.th"; // test sendmail 3
        */
        try {
            MailMessage myMail = new MailMessage(senderMail, receiverMail);
            SmtpClient smtpServer = new SmtpClient();
            smtpServer.Host = "mumail.mahidol.ac.th";
            smtpServer.Port = 587;
            smtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpServer.UseDefaultCredentials = false;
            smtpServer.Credentials = new NetworkCredential(senderMail, password);
            myMail.From = new MailAddress(senderMail);
            myMail.Subject = subject;
            myMail.IsBodyHtml = true;
            myMail.Body = body;
            smtpServer.Send(myMail);
            message = "completed";
        }
        catch (Exception ex) {
            message = ex.Message;
        }

        return message;
    }

    #endregion front end

    #region backend

    /*
    search profile
    */
    public void SearchForApproveToServer(string registrationCode) {
        Querys = @"sp_GetRegistrationProfileForApprove";
        Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear, registrationCode }, commandType: proc)).Result;
        Clients.Caller.SearchApproveFromCode(Dappers);
    }

    /*
    approve profile
    */
    public void ApproveToServer(
        string registrationCode,
        string username,
        string password
    ) {
        string defaultPassword = String.Format("{0}{1}", "muin", eventYear);
        bool approve = password.ToLower().Equals(defaultPassword) ? true : false;
        
        if (approve) {
            Querys = @"sp_SetRegistrationProfileForApprove";
            Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear, registrationCode, username }, commandType: proc)).Result;
            Clients.Caller.ApproveFromServer(Dappers);
            CounterApprovedToServer();
        }
        else
            Clients.Caller.ApproveFromServer(string.Empty);
    }

    /*
    approved counter
    */
    public void CounterApprovedToServer() {
        Querys = (
            @"
            select  count(*) as total
            from    international with (nolock)
            where   (eventtype = 'Internight') and
                    (approve is not null) and
                    (@eventYear = year(createdate))
            "
        );
        Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear }, commandType: texts)).Result;
        Clients.All.CounterApprovedFromServer(Dappers);
    }

    /*
    registration by year on tabs
    */
    public void YearRegistrationToServer() {
        Querys = (
            @"
            select   year(createdate) as yearRegistered
            from     international
            group by year(createDate)
            order by yearRegistered desc
            "
        );
        Dappers = (Db.QueryAsync<dynamic>(Querys, commandType: texts)).Result;
        Clients.Caller.YearRegistrationFromServer(Dappers);
    }

    /*
    load full registered list
    */
    public void RegisteredTableExcelToServer(string registeredYear) {
        Querys = @"sp_GetRegisterReportInternightExportExcel";
        Dappers = (Db.QueryAsync<dynamic>(Querys, new { registeredYear }, commandType: proc)).Result;
        Clients.All.RegisteredTableExcelFromServer(Dappers);
    }

    /*
    update food coupon
    */
    public void CouponToServer(
        string runningCode,
        string coupon
    ) {
        Querys = (
            @"
            update international set
                coupon = @coupon
            where   (runningCode = @runningCode) and
                    (year(createdate) = @eventYear)
            
            select  isnull(coupon, 0) as coupon
            from    international
            where   (runningCode = @runningCode) and
                    (year(createdate) = @eventYear)
            "
        );
        Dappers = (Db.QueryAsync<dynamic>(Querys, new { eventYear, runningCode, coupon }, commandType: texts)).Result;
        Clients.Caller.CouponFromServer(Dappers);
    }

    #endregion backend
}

public class Profile {
    public string IdCard {
        get;
        set;
    }

    public string RunningCode {
        get;
        set;
    }

    public string TitleName {
        get;
        set;
    }

    public string TitleNameOther {
        get;
        set;
    }

    public string Firstname {
        get;
        set;
    }

    public string MiddleName {
        get;
        set;
    }

    public string Lastname {
        get;
        set;
    }

    public string Country {
        get;
        set;
    }

    public string StatusMU {
        get;
        set;
    }

    public string StudentId {
        get;
        set;
    }

    public string StudentDegree {
        get;
        set;
    }

    public string StudentDegreeOther {
        get;
        set;
    }

    public string StaffPosition {
        get;
        set;
    }

    public string FacultyPosition {
        get;
        set;
    }

    public string OtherPosition {
        get;
        set;
    }

    public string Section {
        get;
        set;
    }

    public string Email {
        get;
        set;
    }

    public string Facebook {
        get;
        set;
    }

    public string Food {
        get;
        set;
    }

    public string IpAddress {
        get;
        set;
    }
}