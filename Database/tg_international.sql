USE [MUInternationalWeek]
GO
/****** Object:  Trigger [dbo].[tg_international]    Script Date: 26/8/2562 15:47:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER trigger [dbo].[tg_international]
   on [dbo].[international]
   after insert, delete, update
as
begin
    set concat_null_yields_null off
	
	declare @table varchar(50) = 'international'
    declare @action varchar(10) = null
	
	if exists (select * from inserted)
	begin
		if exists (select * from deleted)
			set @action = 'UPDATE'
		else
			set @action = 'INSERT'
			
		insert into internationalLog
		(
			internationalId,
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
			coupon,
			approve,
			approveBy,
			approveDate,
			createDate,
			createIp,
			EventType,
			logDatabase,
			logTable,
			logAction,
			logActionDate,
			logActionBy,
			logIp
		)
		select	*,
				db_name(),
				@table,
				@action,
				getdate(),
				system_user,
				null
		from	inserted				
	end
	else
		begin
			set @action = 'DELETE'
			
			insert into internationalLog
			(			
				internationalId,
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
				coupon,
				approve,
				approveBy,
				approveDate,
				createDate,
				createIp,
				EventType,
				logDatabase,
				logTable,
				logAction,
				logActionDate,
				logActionBy,
				logIp
			)
			select	*,
					db_name(),
					@table,
					@action,
					getdate(),
					system_user,
					null
			from	deleted			
		end
end
