Vue.directive("uppercase", {
    update: function (element) {
        element.value = element.value.toUpperCase();
    }
});

var app = new Vue({
    el: "#page-container",
    data: {
        connect: {
            class: "",
            time: "",
            online: 0
        },
        disabled: {
            registerForm: false,
            otherTitle: true,
            degree: false,
            degreeOther: true,
            position: false,
            sendmessage: false,
            searchPerson: false
        },
        numbers: 100,
        isAdd: true,
        isUpdate: false,
        forms: {
            IdCard: "",
            TitleName: "",
            TitleNameOther: "",
            Firstname: "",
            MiddleName: "",
            Lastname: "",
            Email: "",
            Facebook: "",
            Section: "",
            Country: "217",            
            FoodDefault: "General",
            Food: this.FoodDefault,
            StatusMU: "",
            StaffPosition: "",
            FacultyPosition: "",
            OtherPosition: "", 
            StudentId: "",
            StudentDegreeDefault: "Bachelor",
            StudentDegree: "",
            StudentDegreeOther: "",
            IpAddress: document.getElementById("ipaddress").value
        },
        positionDefault: "Officer",
        position: "",        
        registerCode: document.getElementById("registercode").value,
        labels: {
            currentyear: "",
            missing: "",
            limit: 150,
            register: "Register",
            update: "Update",
            sendmessage: "Send Message"
        },
        contacts: {
            name: "",
            email: "",
            message: ""
        },
        faculties: [],
        countries: [],
        profiles: [],
        openSeasons: [],
        openSeason: 0,
        registrationOpenStatus: "",
        eventInfo: {}
    },
    methods: {
        OpenDateFromServer(data) {
            this.openSeasons = data;
            this.openSeason = (this.openSeasons && this.openSeasons.length ? this.openSeasons[0].openSeason : 0);
            this.registrationOpenStatus = (this.openSeasons && this.openSeasons.length ? this.openSeasons[0].registrationOpenStatus : "Close");
            this.eventInfo = (this.openSeasons && this.openSeasons.length ? JSON.parse(this.openSeasons[0].eventInfo) : null);
        },
        ConnectFromServer(data) {
            this.connect.class = (data ? "fg-emerald" : "fg-gray");
        },
        OnlineUserFromServer(data) {
            this.connect.online = data;
        },
        FacultyFromServer(data) {
            this.faculties = data;
        },
        CurrentyearFromServer(data) {
            this.labels.currentyear = data;
        },
        CountriesFromServer(data) {
            this.countries = data;
        },
        LimitThaiPeopleFromServer(data) {
            this.labels.limit = data[0].total;
        },
        SearchStudentCodeFromServer(data) {
            this.isAdd = true;
            this.isUpdate = false;

            this.forms.TitleName = "";
            this.forms.TitleNameOther = "";
            this.forms.Firstname = "";
            this.forms.MiddleName = "";
            this.forms.Lastname = "";
            this.forms.Email = "";
            this.forms.Facebook = "";
            this.forms.Section = "";
            this.forms.Country = "";
            this.forms.Food = "";
            this.forms.StatusMU = "";
            this.position = "";
            this.forms.StudentDegree = "";
            this.forms.StudentDegreeOther = "";

            var titleName = "";
            var statusMU = "";
            var degree = "";
            
            if (data && data.length) {
                this.isAdd = (data[0].action === "add" ? true : false);
                this.isUpdate = (data[0].action === "update" ? (data[0].approve !== "Y" ? true : false) : false);

                this.forms.IdCard = data[0].idcard;
                titleName = data[0].enTitleInitials;                
                this.forms.TitleName = titleName;
                this.forms.Firstname = data[0].enFirstName;
                this.forms.MiddleName = data[0].enMiddleName;
                this.forms.Lastname = data[0].enLastName;
                this.forms.Email = data[0].email;
                this.forms.Facebook = data[0].facebook;
                this.forms.Section = (data[0].facultyId === "MU" ? "O" : data[0].facultyId);
                this.forms.Country = (data[0].plcCountryId ? data[0].plcCountryId : "");
                this.forms.Food = (data[0].food ? data[0].food : this.forms.FoodDefault);
                statusMU = (data[0].statusMU ? data[0].statusMU : "");
                this.forms.StatusMU = statusMU;
                degree = (data[0].studentDegree ? data[0].studentDegree : (statusMU === "Student" ? this.forms.StudentDegreeDefault : ""));                
            }

            this.TitleNameOnChange(titleName);
            this.StatusMUOnChange(statusMU);
            this.DegreeOnChange(degree);

            if (data && data.length) {
                this.forms.TitleNameOther = data[0].titleNameOther;
                this.position = (data[0].statusMU === "Student" ? data[0].studentCode : this.position);
                this.position = (data[0].statusMU === "Staff" ? (data[0].staffPosition ? data[0].staffPosition : this.positionDefault) : this.position);
                this.position = (data[0].statusMU === "Faculty" ? (data[0].facultyPosition ? data[0].facultyPosition : this.positionDefault) : this.position);
                this.position = (data[0].statusMU === "Other" ? (data[0].otherPosition ? data[0].otherPosition : this.positionDefault) : this.position);
                this.forms.StudentDegree = degree;
                this.forms.StudentDegreeOther = data[0].studentDegreeOther;
            }

            this.disabled.searchPerson = false;
            this.disabled.registerForm = false;
        },
        mounted() {
            this.querystring();
            this.createQrcode();
        },
        validEmail(email) {
            var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

            return re.test(email);
        },
        englishNumeric(e) {
            var key;
            var isCtrl = false;
            var keychar;
            var reg;

            if (window.event) {
                key = e.keyCode;
                isCtrl = window.event.ctrlKey;
            }
            else {
                if (e.which) {
                    key = e.which;
                    isCtrl = e.ctrlKey;
                }
            }

            if (isNaN(key))
                return true;

            keychar = String.fromCharCode(key);

            if (key === 8 || isCtrl)
                return true;

            reg = /^[A-Za-z0-9]$/;

            if (!reg.test(keychar)) {
                e.stopImmediatePropagation();
                e.preventDefault();
            }
        },
        englishChar(e) {
            var key;
            var isCtrl = false;
            var keychar;
            var reg;

            if (window.event) {
                key = e.keyCode;
                isCtrl = window.event.ctrlKey;
            }
            else {
                if (e.which) {
                    key = e.which;
                    isCtrl = e.ctrlKey;
                }
            }

            if (isNaN(key))
                return true;

            keychar = String.fromCharCode(key);

            if (key === 8 || isCtrl)
                return true;

            reg = /^[A-Za-z0-9& \.\-\\(\\)<>@,;:\"\[\]\/]$/;

            if (!reg.test(keychar)) {
                e.stopImmediatePropagation();
                e.preventDefault();
            }
        },
        registering() {
            if (this.rechecking()) {
                this.modalConfirm("Confirm Registration", "Do you want to save changes ?", "warning").then(result => {
                    if (result) {
                        var labelObj = {};

                        if (this.isAdd)
                            labelObj = this.labels.register;

                        if (this.isUpdate)
                            labelObj = this.labels.update;

                        labelObj = "Saving...";
                        this.disabled.registerForm = true;
                        muinternightHub.server.registerToServer(JSON.stringify(this.forms));
                    }
                });                
            }
            else
                this.modalMessage("Require", this.labels.missing, "warning", "btn btn-warning", "");
        },
        RegisterFromServer(data) {
            this.profiles = data;
            
            if (this.profiles) {
                this.modalMessage("Completed", ("Registration Code : " + this.profiles[0].runningCode + (this.isAdd ? (", send to your mail.") : "") + "\nPlease use it to register at the venue on " + this.openSeasons[0].livedate), "success", "btn btn-success", "Create QR Code").then(() => {
                    this.isAdd = false;
                    this.isUpdate = true;

                    if (this.isAdd)
                        this.labels.register = "Register";

                    if (this.isUpdate)
                        this.labels.update = "Update";

                    this.disabled.registerForm = false;
                    this.registerCode = this.profiles[0].runningCode;

                    $("#qrcode-link").click();

                    setTimeout(() => {
                        this.createQrcode();
                    }, 500);
                });
            }
            else
                this.modalMessage("Registration Close", "Save failed", "warning", "btn btn-danger", "");
        },
        rechecking() {
            this.labels.missing = "";
            this.labels.missing += (!this.forms.IdCard ? "- Student ID / Passport No. / Thai Citizen ID" : "");
            this.labels.missing += (!this.forms.TitleName ? ((this.labels.missing ? "\n" : "") + "- Title Prefix") : "");
            this.labels.missing += (this.forms.TitleName === "Other" && !this.forms.TitleNameOther ? ((this.labels.missing ? "\n" : "") + "- Other Title Prefix") : "");
            this.labels.missing += (!this.forms.Firstname ? ((this.labels.missing ? "\n" : "") + "- First Name") : "");
            this.labels.missing += (!this.forms.Lastname ? ((this.labels.missing ? "\n" : "") + "- Last Name") : "");
            this.labels.missing += (!this.forms.Email ? ((this.labels.missing ? "\n" : "") + "- Email Address") : "");
            this.labels.missing += (this.forms.Email && !this.validEmail(this.forms.Email) ? ((this.labels.missing ? "\n" : "") + "- Invalid Email Address") : "");
            this.labels.missing += (!this.forms.Section ? ((this.labels.missing ? "\n" : "") + "- Faculty / College / Institue / Center") : "");
            this.labels.missing += (!this.forms.Country ? ((this.labels.missing ? "\n" : "") + "- Country") : "");
            this.labels.missing += (!this.forms.Food ? ((this.labels.missing ? "\n" : "") + "- Food") : "");
            this.labels.missing += (!this.forms.StatusMU ? ((this.labels.missing ? "\n" : "") + "- Status at Mahidol University") : "");
            this.labels.missing += (this.forms.StatusMU && this.forms.StatusMU !== "Student" && !this.position ? ((this.labels.missing ? "\n" : "") + "- Position") : "");
            this.labels.missing += (this.forms.StudentDegree === "Other" && !this.forms.StudentDegreeOther ? ((this.labels.missing ? "\n" : "") + "- Degree Other") : "");

            this.forms.StaffPosition = (this.forms.StatusMU === "Staff" ? this.position : null);
            this.forms.StudentId = (this.forms.StatusMU === "Student" ? this.position : null);
            this.forms.FacultyPosition = (this.forms.StatusMU === "Faculty" ? this.position : null);
            this.forms.OtherPosition = (this.forms.StatusMU === "Other" ? this.position : null);

            return (this.labels.missing ? false : true);
        },
        modalMessage(
            titles,
            texts,
            icons,
            btns,
            btntext
        ) {
            return new Promise(resolve => {
                swal({
                    title: titles,
                    text: texts,
                    icon: icons,
                    closeOnClickOutside: true,
                    closeOnEsc: true,
                    buttons: {
                        confirm: {
                            text: (!btntext ? "OK" : btntext),
                            value: true,
                            visible: true,
                            className: btns                            
                        }
                    }
                }).then(result => {
                    resolve(result);
                });
            });
        },
        modalConfirm(
            titles,
            texts,
            icons
        ) {
            return new Promise(resolve => {
                swal({
                    title: titles,
                    text: texts,
                    icon: icons,
                    closeOnClickOutside: true,
                    closeOnEsc: true,
                    buttons: {
                        confirm: {
                            text: "OK",
                            value: true,
                            visible: true,
                            className: "btn btn-primary",
                            closeModal: true
                        },
                        cancel: {
                            text: "CANCEL",
                            value: false,
                            visible: true,
                            className: "btn btn-danger",
                            closeModal: true
                        }
                    }
                }).then(result => {
                    resolve(result);
                });
            });
        },
        TitleNameChange(e) {
            this.TitleNameOnChange(e.target.value);
        },
        TitleNameOnChange(value) {
            if (value === "Other")
                this.disabled.otherTitle = false;
            else {
                this.forms.TitleNameOther = "";
                this.disabled.otherTitle = true;
            }
        },
        StatusMUChange(e) {
            this.StatusMUOnChange(e.target.value);
        },
        StatusMUOnChange(value) {
            if (value === "Student") {
                this.position = this.forms.IdCard;
                this.forms.StudentDegree = this.forms.StudentDegreeDefault;
                this.forms.StudentDegreeOther = "";

                this.disabled.position = true;
                this.disabled.degree = false;                
            }
            else {
                this.position = (value ? this.positionDefault : "");
                this.forms.StudentDegree = "";
                this.forms.StudentDegreeOther = "";

                this.disabled.position = false;
                this.disabled.degree = true;
                this.disabled.degreeOther = true;                
            }
        },
        DegreeChange(e) {
            this.DegreeOnChange(e.target.value);
        },
        DegreeOnChange(value) {
            if (value === "Other")
                this.disabled.degreeOther = false;
            else {
                this.forms.StudentDegreeOther = '';
                this.disabled.degreeOther = true;
            }
        },
        findStudentcode() {
            /*
            if ((this.forms.IdCard.length === 7 || this.forms.IdCard.length === 13) && this.forms.IdCard && !isNaN(this.forms.IdCard)) {
                this.disabled.searchPerson = true;
                this.disabled.registerForm = true;
                muinternightHub.server.searchStudentCodeToServer(this.forms.IdCard);
            }
            */
            this.disabled.searchPerson = true;
            this.disabled.registerForm = true;
            muinternightHub.server.searchStudentCodeToServer(this.forms.IdCard);
        },
        querystring() {
            let uri = window.location.search.substring(1);
            let params = new URLSearchParams(uri);

            this.registerCode = params.get("qr");
            
            if (this.registerCode)
                this.createQrcode();
        },
        createQrcode() {
            this.registerCode = (this.cleanThaiText(this.registerCode) ? this.cleanThaiText(this.registerCode).trim() : '');
            $("#registercode").val("");

            if (this.registerCode) {
                $("#qrcodex").empty();
                $("#qrcodex").qrcode({
                    render: "canvas",
                    mSize: 0.1,
                    mposx: 0.5,
                    mposy: 0.5,
                    size: 200,
                    color: "#3a3",
                    mode: 4,
                    quiet: 2,
                    image: jq("#img-buffer")[0],
                    text: ("https://smartedu.mahidol.ac.th/muinternight/admin/start/qr.cshtml?qr=" + this.registerCode)
                    /*
                    text: 'http://10.43.4.5/muinternight/admin/start/qr.cshtml?qr=' + this.registerCode
                    */
                });
            }
            return false;
        },
        RegisteredTableFromServer(data) {
            $('#table_id').DataTable({
                data: data,
                order: [[4, "desc"]],
                paging: true,
                searching: true,
                destroy: true,
                language: {
                    "search": "<i class='fa fa-search' aria-hidden='true'></i>",
                    "paginate": {
                        "previous": "<i class='fas fa-angle-left' aria-hidden='true'></i>",
                        "next": "<i class='fas fa-angle-right' aria-hidden='true'></i>"
                    }
                },
                columns: [
                    {
                        data: "rn"
                    },
                    {
                        data: "totaltitle",
                        className: "d-none d-sm-table-cell"
                    },
                    {
                        data: "fullname"
                    },
                    {
                        data: "countryNameEN",
                        className: "col4"
                    },
                    {
                        data: "createdatex",
                        className: "d-none d-sm-table-cell"
                    }
                ],
                initComplete: function () {
                    $(this).removeClass("d-none");
                }
            });
        },
        onEnter(e) {
            if (e.keyCode === 13)
                this.createQrcode();
        },
        sendMessage() {
            var errorMsg = ""

            errorMsg += (!this.contacts.name ? ((errorMsg ? "\n" : "") + "- Name") : "");
            errorMsg += (!this.contacts.email ? ((errorMsg ? "\n" : "") + "- Email Address") : "");
            errorMsg += (this.contacts.email && !this.validEmail(this.contacts.email) ? ((errorMsg ? "\n" : "") + "- Invalid Email Address") : "");
            errorMsg += (!this.contacts.message ? ((errorMsg ? "\n" : "") + "- Message") : "");

            if (errorMsg)
                this.modalMessage("Require", errorMsg, "warning", "btn btn-warning", "");
            else {
                if (this.contacts.name && this.contacts.email && this.contacts.message) {
                    this.labels.sendmessage = "Sending...";
                    this.disabled.sendmessage = true;

                    setTimeout(() => {
                        muinternightHub.server.sendEmailToServer(this.contacts.name, this.contacts.email, this.contacts.message);
                        this.contacts.name = "";
                        this.contacts.email = "";
                        this.contacts.message = "";
                        this.labels.sendmessage = "Send Messeage";
                        this.disabled.sendmessage = false;                        
                    }, 5000);
                }
            }
        },
        SendEmailFromServer(data) {
            if (data)
                this.modalMessage("Completed", "Send message is finished", "success", "btn btn-success", "");
        },
        cleanThaiText(data) {
            return (data ? data.replace(/[ๅภถุึคตจขชๆไำพะัีรนยบลฃฟหกดเ้่าสวงผปแอิืทมใฝ๑๒๓๔ู฿๕๖๗๘๙๐ฎฑธํ๊ณฯญฐฅฤฆฏโฌ็๋ษศซฉฮฺ์ฒฬฦ]+/, "") : "");
        }
    },
    computed: {
        autoShutdown() {
            if (this.openSeason)
                return (this.forms.Country === "217" && this.labels.limit === 0 ? true : false);
            else
                return true;
        },
        autoCloseForm() {
            return (this.openSeason > 0 ? true : false);
        },
        autoQrCodeButton() {
            if (this.registerCode.length > 0)
                $("#qrcodex").empty();
            
            if ($("#registercode").val()) {
                this.modalMessage("QR Code", ("Registration Code : " + this.registerCode), "info", "btn btn-info", "Create QR Code").then(() => {
                    $("#qrcode-link").click();

                    setTimeout(() => {
                        this.createQrcode();
                    }, 500);
                });
            }
            
            return (this.registerCode && this.registerCode.length >= 4 ? false : true);
        }
    }
});

/*
signalR
*/
$.connection.hub.start().done(() => {
    muinternightHub.server.openDateToServer();
    muinternightHub.server.onlineUserToServer();
    muinternightHub.server.facultyToServer();
    muinternightHub.server.currentyearToServer();
    muinternightHub.server.countriesToServer();
    muinternightHub.server.limitThaiPeopleToServer();
    muinternightHub.server.registeredTableToServer();
});

muinternightHub.client.OpenDateFromServer = app.OpenDateFromServer; // ปิดเปิดวันลงทะเบียน
muinternightHub.client.ConnectFromServer = app.ConnectFromServer; // เชื่อมระบบ
muinternightHub.client.OnlineUserFromServer = app.OnlineUserFromServer; // เช็คยอดออนไลน์
muinternightHub.client.FacultyFromServer = app.FacultyFromServer; // โหลดรายชื่อคณะ
muinternightHub.client.CurrentyearFromServer = app.CurrentyearFromServer; // โหลดปีปัจจุบัน
muinternightHub.client.CountriesFromServer = app.CountriesFromServer; // โหลดรายชื่อประเทศ
muinternightHub.client.LimitThaiPeopleFromServer = app.LimitThaiPeopleFromServer; // โหลดยอดคนลงทะเบียนคนไทย
muinternightHub.client.RegisterFromServer = app.RegisterFromServer; // กดปุ่มลงทะเบียน
muinternightHub.client.RegisteredTableFromServer = app.RegisteredTableFromServer; // โหลดรายชื่อคนลงทะเบียน
muinternightHub.client.SearchStudentCodeFromServer = app.SearchStudentCodeFromServer; // ค้นหาประวัตินักศึกษา
muinternightHub.client.SendEmailFromServer = app.SendEmailFromServer; // แจ้งรับทราบข้อความคำถาม

$.connection.hub.reconnecting(() => {
    app.connect.class = "fg-red";
    tryReconnect = true;
});

$.connection.hub.reconnected(() => {
    tryReconnect = false;
});