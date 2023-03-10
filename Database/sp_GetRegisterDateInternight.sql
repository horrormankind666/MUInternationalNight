USE [MUInternationalWeek]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetRegisterDateInternight]    Script Date: 12/01/2566 15:51:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author		: <Odd#indy>
-- Create date	: <26/09/2018>
-- Description	: <Open date season>
-- =============================================
-- exec sp_GetRegisterDateInternight 2023

ALTER procedure [dbo].[sp_GetRegisterDateInternight]
(
	@eventYear int = null
)
as
begin
	set concat_null_yields_null off

	declare @openSeason int = null

	select	Opendate as openDateMaster,
			(datename(day, Opendate) + ' ' + datename(month, Opendate) + ' ' + datename(year, Opendate)) as opendate,
			Closedate as closeDateMaster,
			(datename(day, dateadd(day, -1, Closedate)) + ' ' + datename(month, dateadd(day, -1, Closedate)) + ' ' + datename(year, dateadd(day, -1, Closedate))) as closedate,
			Ondate as onDateMaster,
			(datename(day, Ondate) + ' ' + datename(month, Ondate) + ' ' + datename(year, Ondate)) as ondate,
			Livedate as livedate,
			location,
			ContactMail as contactMail,
			ContactNameEn as contactNameEN,
			ContactPhone as contactPhone,
			eventInfo
	into	#tb_muinEventInfo1
	from	InterConfig with(nolock)
	where	(eventtype = 'Internight') and
			(Years = @eventYear)

	select	(
				case when
					(getdate() between Opendate and Closedate) or
					((datename(day, getdate()) + ' ' + datename(month, getdate()) + ' ' + datename(year, getdate())) = (datename(day, Ondate) + ' ' + datename(month, Ondate) + ' ' + datename(year, Ondate)))
				then
					1
				else
					0
				end
			) as openSeason
	into	#tb_muinEventInfo2
	from	InterConfig with(nolock)
	where	(eventtype = 'Internight') and
			(Years = @eventYear) and
			(getdate() between Opendate and dateadd(day, 1, Ondate))
	
	select	@openSeason = openSeason
	from	#tb_muinEventInfo2

	select	*,
			isnull(@openSeason, 0) as openSeason,
			(
				case when getdate() < openDateMaster then 'Soon' else
					case when isnull(@openSeason, 0) = 0 then 'Close' else
						case when isnull(@openSeason, 0) = 1 then 'Open' else						
							null
						end
					end
				end
			) as registrationOpenStatus
	from	#tb_muinEventInfo1
	
	/*
	select	 (case when (getdate() between opendate and Closedate) or (format(getdate(),'d/MM/yyyy') = format(ondate, 'd/MM/yyyy')) then 1 else 0 end) as openSeason, 
			 format(Opendate, 'd MMMM yyyy') as opendate, 
			 format(dateadd(day, -1, Closedate), 'd MMMM yyyy') as closedate,
			 format(Ondate, 'd MMMM yyyy') as ondate,
			 livedate,
			 [location]
	from	 InterConfig with (nolock)
	where	 (eventtype = 'Internight') and
			 (getdate() between Opendate and dateadd(day, 1, Ondate)) --or
			 --(format(getdate(), 'dd/MM/yyyy') = format(ondate, 'dd/MM/yyyy'))
	group by Opendate,
			 Closedate,
			 Ondate,
			 livedate,
			 [location]
	--order by Ondate
	*/
end