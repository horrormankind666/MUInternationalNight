var app = new Vue({
    el: "#page-container",
    data: {
        connect: {
            class: "",
            time: "",
            online: 0,
            approved: 0
        },
        approve: {
            registerCode: ""
        },
        labels: {
            currentyear: "",
            missing: "",
            approve: "",
            excellink: ""
        },
        profiles: [],
        max: 5,
        yearRegistered: [],
        yearRegisteredDefault: "",
        coupons: 0,
        contents: {
            approve: true,
            report: false
        },
        panels: {
            approve: false
        },
        disableds: {
            approve: false,
            minus: "disabled"
        },
        activeMenu: "approve"
    },
    mounted() {
    },
    methods: {
        CurrentyearFromServer(data) {
            this.labels.currentyear = data;
        },
        setActiveMenu(data) {
            switch (data) {
                case "approve": {
                    this.contents.approve = true;
                    this.contents.report = false;
                    break;
                }
                case "report": {
                    this.contents.approve = false;
                    this.contents.report = true;
                    muinternightHub.server.yearRegistrationToServer();
                    muinternightHub.server.registeredTableExcelToServer("");
                    muinternightHub.server.yearRegistrationToServer();
                    break;
                }
            }
            this.activeMenu = data;
        },
        isActiveMenu(data) {
            return this.activeMenu === data;
        },
        CounterApprovedFromServer(data) {
            this.connect.approved = data[0].total;
        },
        SearchApproveFromCode(data) {
            this.profiles = data;
            this.panels.approve = (this.profiles && this.profiles.length ? false : true);

            if (this.profiles &&
                this.profiles.length) {
                this.disableds.approve = (this.profiles[0].approve === "Y" ? true : false);
                this.labels.approve = (this.profiles[0].approve === "Y" ? "APPROVED" : "APPROVE");
                this.coupons = this.profiles[0].coupon;
            }
        },
        searchForApproveToServer() {
            this.approve.registerCode = this.cleanThaiText(this.approve.registerCode);

            if (this.approve.registerCode)
                muinternightHub.server.searchForApproveToServer(this.approve.registerCode);
            else
                this.modalMessage("Require", "Registration Code", "warning", "btn btn-warning");
        },
        modalMessage(
            titles,
            texts,
            icons,
            btns
        ) {
            return new Promise(resolve => {
                swal({
                    title: titles,
                    text: texts,
                    icon: icons,
                    buttons: {
                        confirm: {
                            text: "OK",
                            value: true,
                            visible: true,
                            className: btns,
                            closeModal: true
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
        onApproveEnter(e) {
            this.profiles = [];
            this.panels.approve = false;

            if (e.keyCode === 13)
                this.searchForApproveToServer();
        },
        approving() {
            let registrationCode = this.profiles[0].registrationCode;
            let username = document.getElementById("username").innerText;

            this.modalConfirm("Confirm Approve", ("Do you want to approve " + "registration code : " + registrationCode + " ?"), "warning").then(result => {
                if (result) {
                    if (registrationCode &&
                        username &&
                        this.labels.currentyear
                    ) {
                        this.disableds.approve = true;
                        muinternightHub.server.approveToServer(registrationCode, username, ("muin" + this.labels.currentyear));
                        this.modalMessage("Approved", ("Registration Code : " + registrationCode), "success", "btn btn-success");
                        this.approve.registerCode = "";
                        this.profiles = [];
                        this.panels.approve = false;
                        this.disableds.approve = false;
                    }
                }
            });                            
        },
        RegisteredTableExcelFromServer(data) {
            $('#table_id').DataTable({
                data: data,
                order: [[7, "desc"]],
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                pageLength: 25,
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
                        data: "registerCode",
                        className: "col1"
                    },
                    {
                        data: "studentDegree",
                        className: "col2"
                    }, 
                    {
                        data: "fullname",
                        className: "col3"
                    },
                    {
                        data: "country",
                        className: "col4"
                    },
                    {
                        data: "statusMU",
                        className: "col5"
                    },
                    {
                        data: "approve",
                        render: ((a) => a === "Yes" ? "<div class='text-center'><i class='fa fa-check text-green'></i></div>" : "<div class='text-center'><i class='fa fa-times text-red text-center'></i></div>"),
                        className: "col6"
                    },
                    {
                        data: "email",
                        className: "col7"
                    },
                    {
                        data: "createDates",
                        className: "col8"
                    }
                ],
                initComplete: function () {
                    $(this).wrap("<div class='table-responsive'></div>");
                    $("#report .panel-body, #report .panel-footer").removeClass("d-none");
                }
            });
        },
        YearRegistrationFromServer(data) {
            this.yearRegistered = data;
            this.yearRegisteredDefault = this.yearRegistered[0].yearRegistered;
            this.labels.excellink = ("ExportExcel.aspx?selectyear=" + this.yearRegistered[0].yearRegistered);
        },
        yearRegisteredChange(e) {
            muinternightHub.server.registeredTableExcelToServer(e.target.value);
            this.yearRegisteredDefault = e.target.value;
            this.labels.excellink = ("ExportExcel.aspx?selectyear=" + this.yearRegisteredDefault);
        },
        cleanThaiText(data) {
            return data.replace(/[ๅภถุึคตจขชๆไำพะัีรนยบลฃฟหกดเ้่าสวงผปแอิืทมใฝ๑๒๓๔ู฿๕๖๗๘๙๐ฎฑธํ๊ณฯญฐฅฤฆฏโฌ็๋ษศซฉฮฺ์ฒฬฦ]+/, "");
        },
        coupon(data) {
            this.coupons += data;
            this.coupons = (this.coupons < 0 ? 0 : this.coupons);
            this.coupons = (this.coupons > this.max ? this.max : this.coupons);

            if (this.approve.registerCode && this.coupons <= this.max)
                muinternightHub.server.couponToServer(this.approve.registerCode, this.coupons);
        },
        CouponFromServer(data) {
            if (data && data.length)
                this.modalMessage("Coupon", ("Used : " + data[0].coupon + " Time(s)"), "success", "btn btn-success");
            else
                this.modalMessage("Failed", "Data Not Found", "warning", "btn btn-warning");
        }
    },
    computed: {
        maximunCoupon() { return this.coupons >= this.max ? true : false; },
        maximunCouponClass() { return this.coupons >= this.max ? "disabled" : ""; },
        minimunCouponClass() { return this.coupons === 0 ? "disabled" : ""; }
    }
});

// signalR
$.connection.hub.start().done(() => {
    muinternightHub.server.counterApprovedToServer();
    muinternightHub.server.currentyearToServer();
});

muinternightHub.client.SearchApproveFromCode = app.SearchApproveFromCode;// โหลดชื่อคนลงทะเบียนเพื่ออนุมัติ
muinternightHub.client.CurrentyearFromServer = app.CurrentyearFromServer; // โหลดปีปัจจุบัน
muinternightHub.client.CounterApprovedFromServer = app.CounterApprovedFromServer; // นับยอดคนลงทะเบียนที่อนุมัติแล้ว
muinternightHub.client.RegisteredTableExcelFromServer = app.RegisteredTableExcelFromServer; // โหลดรายชื่อคนลงทะเบียน
muinternightHub.client.YearRegistrationFromServer = app.YearRegistrationFromServer; // โหลดปีที่เปิดรับลงทะเบียน
muinternightHub.client.CouponFromServer = app.CouponFromServer; // อัพเดทยอดคูปองที่ใช้

$.connection.hub.reconnecting(() => {
    app.connect.class = "fg-red";
    tryReconnect = true;
});

$.connection.hub.reconnected(() => { tryReconnect = false; });