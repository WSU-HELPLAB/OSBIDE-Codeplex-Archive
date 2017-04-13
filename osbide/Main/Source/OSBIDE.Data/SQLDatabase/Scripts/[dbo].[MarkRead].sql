-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [MarkRead]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[MarkRead]

	 @PostId int
	,@User int
	,@Read bit
as
begin

	set nocount on;
	update dbo.FeedPostUserTags
	set Viewed = @Read
	where FeedPostId = @PostId and UserId = @User;

end