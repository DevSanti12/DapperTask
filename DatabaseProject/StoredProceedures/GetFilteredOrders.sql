CREATE PROCEDURE [dbo].[GetFilteredOrders]
	@Year INT = NULL,
	@Month INT = NULL,
	@Status NVARCHAR(50) = NULL,
	@ProductId INT = NULL
AS
BEGIN
	SELECT *
	FROM dbo.[Order]
	WHERE (@Year is NULL OR YEAR(CreatedDate) = @Year)
	  AND (@Month is NULL OR MONTH(CreatedDate) = @Month)
	  AND (@Status is NULL OR Status = @Status)
	  AND (@ProductId is NULL OR ProductId = @ProductId)
END;