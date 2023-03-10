USE [MUInternationalWeek]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetRegistrationProfileForApprove]    Script Date: 07/01/2566 00:26:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author		: <Odd#indy>
-- Create date	: <29-09/2018>
-- Description	: <Search Register Profile>
-- =============================================
-- exec sp_GetRegistrationProfileForApprove 2023, 'O001'

ALTER procedure [dbo].[sp_GetRegistrationProfileForApprove]
(
	@eventYear int = null,
	@registrationCode varchar(15) = null
)
as
begin
	set concat_null_yields_null off

	declare @opendate datetime = getdate()
	declare @closedate datetime = getdate()
	declare @internight varchar(25) = 'Internight'

	select	 top 1
			 @opendate = opendate,
			 @closedate = Closedate
	from	 InterConfig with(nolock)
	where	 (eventtype = @internight) and
			 (Years = @eventYear)

	select	 top 1
			 a.runningCode as registrationCode,
			 (case when a.titlename != 'Other' then a.titlename else a.titleNameOther end) as titlename,
			 (isnull(a.firstName, '') + ' ' + isnull(a.middleName,'') + ' ' + isnull(a.lastName,'')) as fullname,
			 a.statusMU,
			 (case when a.studentDegree = 'Other' then a.studentDegreeOther else a.studentDegree end) as studentDegree,
			 b.countryNameEN,
			 d.FacultyName as section,	
			 iif(a.event1 = 'W','Walkin','Booking') as event1,
			 a.approve,
			 isnull(a.coupon,0) as coupon,
			 (convert(varchar(10), a.createdate, 103) + ', ' + convert(varchar(8), a.createdate, 24)) as createdate,
			 (case when a.approve = 'Y' then (convert(varchar(10), a.approvedate, 103) + ', ' + convert(varchar(8), a.approvedate, 24)) else null end) as approvedate
    from	 MUInternationalWeek..international as a with (nolock) left join
			 Infinity..plcCountry as b with (nolock) on a.country = b.id left join
			 Infinity..perTitlePrefix as c with (nolock) on a.titleName = c.id left join
			 MUInternationalWeek..Faculty as d with (nolock) on a.section = d.FacultyCode
    where	 (a.eventtype = @internight) and
			 (a.runningCode = @registrationCode or a.idcard = @registrationCode) and
			 (a.createDate between @opendate and @closedate)
	order by a.createdate desc
end
