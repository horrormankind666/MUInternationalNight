USE [MUInternationalWeek]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetRegisterReportInternightExportExcel]    Script Date: 27/8/2562 0:10:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author		:		<Odd#indy>
-- Create date	: <01/10/2018>
-- Description	:	<Export Excel Register report>
-- sp_GetRegisterReportInternightExportExcel '2019'
-- =============================================
alter procedure [dbo].[sp_GetRegisterReportInternightExportExcel]
	@registeredYear varchar(4) = null
as
begin
	set concat_null_yields_null off

	declare @opendate datetime = (
		select	top 1
				opendate
		from	InterConfig
		where	(eventtype = 'Internight')
	)
	declare @closedate datetime = (
		select	top 1
				dateadd(day, 1, ondate)
		from	InterConfig
		where	(eventtype = 'Internight')
	)
	declare @registeryeardefault varchar(4) = (
		select	 top 1
				 year(createdate) as yearRegistered 
		from	 international
		group by year(createDate)
		order by yearRegistered desc
	)
    
	set @registeryeardefault = (
		select	iif(@registeredYear != '', @registeredYear, @registeryeardefault)
	)
	
	select	 row_number() over (order by a.section) as Ordering,
			 a.runningCode as registerCode,
			 --(a.section + right('000'+ convert(varchar(4),ROW_NUMBER() OVER (PARTITION BY a.section ORDER By section, a.id)),3) registerCode,
			 (case when a.titlename != 'Other' then a.titlename else a.titleNameOther end) as totaltitle,
			 a.idcard,
			 (isnull(a.firstName, '') + ' ' + isnull(a.middleName, '') + ' ' + isnull(a.lastName, '')) as fullname,
			 isnull(b.countryNameEN, 'Guest') as country,
             a.statusMU,
             a.studentId,
             a.studentDegree,
             a.studentDegreeOther,
             a.staffPosition,
             a.facultyPosition,
             a.otherPosition,
             a.section,
             c.facultyname,
             a.email,
             a.facebook,
             a.food,
			 year(a.createdate) as createyear,
			 convert(varchar(16), a.createDate, 120) as createDates,
			 iif(a.approve = 'Y', 'Yes', 'No') as approve,
			 a.approveBy,
			 a.approveDate
    from	 MUInternationalWeek..international as a left join
			 Infinity..plcCountry as b on a.country = b.id left join
			 MUInternationalWeek..Faculty as c on a.section = c.facultycode
    where	 (year(a.createdate) = @registeryeardefault) and
			 (eventtype = 'Internight')
	order by a.section;
end