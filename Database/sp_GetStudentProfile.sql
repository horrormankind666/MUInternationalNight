USE [MUInternationalWeek]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetStudentProfile]    Script Date: 09/01/2566 11:32:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author		: <Odd#indy>
-- Create date	: <28/09/2018>
-- Description	: <Find student profile>
-- sp_GetStudentProfile '6088111' /*<-- student */
-- sp_GetStudentProfile '3100201301702' /*<-- staff */
-- sp_GetStudentProfile '5938601' /*<-- lastyear */
-- =============================================
ALTER PROCEDURE [dbo].[sp_GetStudentProfile]
(
	@eventYear int = null,
	@studentCode varchar(13) = null
)
as
begin
	set concat_null_yields_null off

	declare @opendate datetime = getdate()
	declare @closedate datetime = getdate()
	declare @internight varchar(25) = 'Internight'

	select	top 1
			@opendate = opendate,
			@closedate = Closedate
	from	InterConfig with (nolock)
	where	(eventtype = @internight) and
			(Years = @eventYear)

	-- lastyear data
	declare @found int = (
		select	count(id)
		from	international with (nolock)
		where	(idcard = @studentCode) or
				(studentId = @studentCode)
	)

	if (@found > 0)
	begin
		select	 top 1
				 (case when createDate between @opendate and @closedate then 'update' else 'add' end) as action,
				 (case when createDate between @opendate and @closedate then approve else null end) as approve,
				 idcard,
				 isnull(studentId, idcard) as studentCode,
				 titlename as enTitleInitials,
				 titleNameOther,
				 firstname as enFirstName,
				 middlename as enMiddleName,
				 lastName as enLastName,
				 email,
				 facebook,
				 section as facultyId,
				 country as plcCountryId,
				 food,
				 statusMU,
				 staffPosition,
				 facultyPosition,
				 otherPosition,
				 studentDegree,
				 studentDegreeOther
		from	 international with (nolock)
		where	 (idcard = @studentCode) or 
				 (studentId = @studentCode)
		order by id desc
	end

	if (@found = 0)
	begin
		-- student
		if (len(@studentCode) = 7)
		begin
			declare @student int = (
				select	count(id)
				from	Infinity..stdStudent with (nolock)
				where	(studentCode = @studentCode)
			)
			
			if (@student > 0)
				select	top 1
						'add' as action,
						null as approve,
						isnull(b.idCard, @studentCode) as idcard,
						a.studentCode,
						(case when c.enTitleInitials = 'Ms.' then c.enTitleFullName else c.enTitleInitials end) as enTitleInitials,
						null as titleNameOther,
						b.enFirstName,
						b.enMiddleName,
						b.enLastName,
						b.email,
						null as facebook,
						iif(left(a.facultyId, 2) = 'IT', 'ICT', left(a.facultyId, 2)) as facultyId,						
						b.plcCountryId,
						null as food,
						'Student' as statusMU,
						null as staffPosition,
						null as facultyPosition,
						null as otherPosition,
						null as studentDegree,
						null as studentDegreeOther
				from	Infinity..stdStudent as a with (nolock) left join
						Infinity..perPerson as b with (nolock) on a.personId = b.id	left join
						Infinity..perTitlePrefix as c with (nolock) on b.perTitlePrefixId = c.id
				where	(a.studentCode = @studentCode) or 
						(a.studentCode = b.idCard)
		end
	
		-- staff
		if (len(@studentCode) = 13)
		begin
			declare @staff int = (
				select	count(idCard)
				from	Infinity..vwEmployAD with (nolock)
				where	(idCard = @studentCode)
			)
			
			if (@staff > 0)
				select	top 1
						'add' as action,
						null as approve,
						isnull(a.idCard, @studentCode) as idcard,
						@studentCode as studentCode,
						(case when b.enTitleInitials = 'Ms.' then b.enTitleFullName else b.enTitleInitials end) as enTitleInitials,
						null as titleNameOther,
						a.enFirstName,
						null as enMiddleName,
						a.enLastName,
						a.eMail as email,
						null as facebook,
						left(a.facultyId, 2) as facultyId,
						'217' as plcCountryId,
						null as food,
						'Staff' as statusMU,
						null as staffPosition,
						null as facultyPosition,
						null as otherPosition,
						null as studentDegree,
						null as studentDegreeOther
				from	Infinity..vwEmployAD as a with (nolock)	left join
						Infinity..perTitlePrefix as b with (nolock) on a.title = b.thTitleInitials
				where	(idCard = @studentCode)
		end
	end
end