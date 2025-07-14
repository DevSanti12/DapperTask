CREATE TABLE [dbo].[Order]
(
	[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	Status INT NOT NULL, -- Will store integer representation of the OrderStatus enum,
	CreatedDate DATETIME NOT NULL,
	UpdatedDate DATETIME NOT NULL,
	ProductId INT NOT NULL,
	CONSTRAINT FK_Product FOREIGN KEY (ProductId) references [dbo].[Product](Id)
)
