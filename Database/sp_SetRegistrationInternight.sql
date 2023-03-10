USE [MUInternationalWeek]
GO
/****** Object:  StoredProcedure [dbo].[sp_SetRegistrationInternight]    Script Date: 07/01/2566 01:13:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author		: <Odd#indy>
-- Create date	: <25/09/2018>
-- Description	: <Save Registration internight Profile>
-- =============================================
ALTER procedure [dbo].[sp_SetRegistrationInternight]
	@eventYear int = null,
	@IdCard varchar(50) = null,
    @TitleName varchar(50) = null,
    @TitleNameOther varchar(50) = null,
    @Firstname varchar(50) = null,
    @MiddleName varchar(50) = null,
    @Lastname varchar(50) = null,
    @Country varchar(50) = null,
    @StatusMU varchar(50) = null,
    @StudentId varchar(50) = null,
    @StudentDegree varchar(50) = null,
    @StudentDegreeOther varchar(50) = null,
    @StaffPosition varchar(50) = null,
    @FacultyPosition varchar(50) = null,
    @OtherPosition varchar(50) = null,
    @Section varchar(50) = null,
    @Email varchar(50) = null,
    @Facebook varchar(50) = null,
    @Food varchar(50) = null,
	@IpAddress varchar(50) = null
as
begin
	set concat_null_yields_null off

	begin try
		-- กำหนดค่าตัวแปร
		declare @id int
		declare @year varchar(4) = ''
		declare @opendate datetime = getdate()
		declare @closedate datetime = getdate()
		declare @ondate datetime = getdate()
		declare @internight varchar(25) = 'Internight'
		declare @registrationCode varchar(25) = ''
		declare @event varchar(1)
		declare @found int = 0

		-- ตรวจว่าเคยลงทะเบียนมาก่อนหรือไม่
		select	 top 1
				 @year = Years,
				 @opendate = opendate,
				 @closedate = Closedate,
				 @ondate = Ondate
		from	 InterConfig with (nolock)
		where	 (eventtype = @internight) and
				 (Years = @eventYear)
		order by Years desc

		select	@id = id,
				@event = event1
		from	international with (nolock)
		where	(idcard = @idcard) and 
				(eventtype = @internight) and
				(createDate between @opendate and @closedate)

		set @found = (case when @id is not null then 1 else 0 end)

		-- บันทึกผู้ลงทะเบียนคนใหม่
		if (@found = 0 and (getdate() between @opendate and @closedate))
		begin
			-- ตรวจรหัส Regsitration Code
			set @found = (
				select	count(runningCode)
				from	international with (nolock)
				where	(section = @Section) and
						(createDate between @opendate and @closedate)
			)
		
			-- สร้าง Regsitration Code ใหม่ (เอาเลขคนล่าสุด + 1)
			if (@found > 0)
				set @registrationCode = (
					select	 top 1
							 (section + right('000'+ convert(varchar(4), row_number() over (partition by section order by section, id) + 1), 3))
					from	 international with (nolock)
					where	 (section = @section) and
							 ((createDate between @opendate and @closedate) or (format(getdate(), 'dd/MM/yyyy') = format(@ondate, 'dd/MM/yyyy')))
					order by id desc
				)
			else
				set @registrationCode = (@section + '001');

			--set @registrationCode = ('MUIN' + right('000000'+ (convert(varchar, ident_current('international') + 1)), 6))

			-- กำหนด ถ้าลงทะเบียนในวันงาน ให้ event = W ถ้าไม่ใช่ ให้ B
			set @event = (select iif(format(getdate(), 'dd/MM/yyyy') = format(@ondate, 'dd/MM/yyyy'), 'W' ,'B'))

			-- เพิ่มผู้ลงทะเบียนใหม่
			insert into international
			(
				runningCode,
				idcard,
				titleName,
				titleNameOther,
				firstName, 
				middleName,
				lastName,
				country,
				statusMU,
				studentId,
				studentDegree,
				studentDegreeOther,
				staffPosition,
				facultyPosition,
				otherPosition,
				section,
				email,
				facebook,
				food,
				event1,
				createDate,
				createIp,
				eventtype
			)
			values
			(				
				@registrationCode,
				@idcard,
				@titleName,
				nullif(@titleNameOther, ''),
				@firstName, 
				nullif(@middleName, ''),
				@lastName,
				@country,
				nullif(@statusMU, ''),
				@studentId,
				nullif(@studentDegree, ''),
				nullif(@studentDegreeOther, ''),
				nullif(@staffPosition, ''),
				nullif(@facultyPosition, ''), 
				nullif(@otherPosition, ''),
				@section,
				@email,
				nullif(@facebook, ''),
				@food,
				@event,
				getdate(),
				@IpAddress,
				@internight
			)

			set @id = ident_current('international')
		end

		if (@found > 0 and len(@IdCard) > 0 and (getdate() between @opendate and @closedate) )
		begin
			-- อัพเดทข้อมูลผู้ลงทะเบียน
			update international set
				idcard				= @IdCard,
				titleName			= @TitleName,
				titleNameOther		= nullif(@TitleNameOther, ''),
				firstName			= @Firstname, 
				middleName			= nullif(@MiddleName, ''),
				lastName			= @Lastname,
				country				= @Country,
				statusMU			= @StatusMU,
				studentId			= @StudentId,
				studentDegree		= nullif(@StudentDegree, ''),
				studentDegreeOther	= nullif(@StudentDegreeOther, ''),
				staffPosition		= nullif(@StaffPosition, ''),
				facultyPosition		= nullif(@FacultyPosition, ''), 
				otherPosition		= nullif(@OtherPosition, ''),
				section				= @Section,
				email				= @Email,
				facebook			= nullif(@Facebook, ''),
				food				= @Food,
				event1				= @event,
				createDate			= getdate(),
				createIp			= @IpAddress
			where	(id = @id) and 
					(eventtype = @internight) and
					(createDate between @opendate and @closedate)
		end

		-- Final Query
		select	 top 1
				 *,
				 @found as found,
				 @year as years
		from	 international with (nolock)
		where	 (id = @id) and 
				 (eventtype = @internight) and
				 (createDate between @opendate and @closedate)
		order by createDate desc
	end try
	begin catch
		rollback tran
		insert into InfinityLog..sysError
		(
			systemName,
			errorNumber,
			errorMessage,
			hint,
			url,
			logDate
		)
		values
		(			
			'international',
			error_number(),
			error_message(),
			error_procedure(),
			null,
			getdate()
		)			
	end catch	
end