USE [MUInternationalWeek]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetRegisterDateInternight]    Script Date: 22/8/2562 12:37:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author		: <Odd#indy>
-- Create date	: <26/09/2018>
-- Description	: <Open date season>
-- exec sp_GetRegisterDateInternight
-- =============================================
ALTER procedure [dbo].[sp_GetRegisterDateInternight]
as
begin
	set concat_null_yields_null off

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
end