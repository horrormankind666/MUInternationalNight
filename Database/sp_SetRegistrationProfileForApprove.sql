USE [MUInternationalWeek]
GO
/****** Object:  StoredProcedure [dbo].[sp_SetRegistrationProfileForApprove]    Script Date: 07/01/2566 00:45:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author		: <Odd#indy>
-- Create date	: <29-09-2018>
-- Description	: <Search Register Profile>
-- =============================================
-- exec sp_SetRegistrationProfileForApprove 2023 'o001','nopparat.jap'

ALTER procedure [dbo].[sp_SetRegistrationProfileForApprove]
(
	@eventYear int = null,
	@registrationCode varchar(15) = null,
	@username varchar(255) = null
)
as
begin
	set concat_null_yields_null off

	begin try
		declare @id int		
		declare @opendate datetime = getdate()
		declare @closedate datetime = getdate()
		declare @internight varchar(25) = 'Internight'

		select	 top 1
				 @opendate = opendate,
				 @closedate = Closedate
		from	 InterConfig with (nolock)
		where	 (eventtype = @internight) and
				 (Years = @eventYear)

		select	@id = id
		from	international with (nolock)
		where	(runningCode = @registrationCode) and 
				(eventtype = @internight) and
				(createDate between @opendate and @closedate)

		if (len(isnull(@username, '')) > 0 and len(isnull(@registrationCode, '')) > 0)
		begin
			update international set
				approve = 'Y',
				approveDate = getdate(),
				approveBy = @username
			where	(id = @id) and 
					(eventtype = @internight) and
					(createDate between @opendate and @closedate)
		end

		select	approve
		from	international
		where	(id = @id) and 
				(eventtype = @internight) and
				(createDate between @opendate and @closedate)
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
END
