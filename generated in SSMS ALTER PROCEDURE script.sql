USE [LifelongLearning]
GO
/****** Object:  StoredProcedure [dbo].[sp_get_people]    Script Date: 9/30/2021 6:23:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[sp_get_people]
AS
-- stored procedure created by Sam Klok
-- to run
-- exec dbo.sp_get_people
SELECT * FROM people
