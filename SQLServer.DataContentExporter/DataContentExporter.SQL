USE [SampleCodeDB]
GO

/****** Object:  Table [dbo].[Employee]    Script Date: 10/6/2020 2:12:58 AM ******/
DROP TABLE [dbo].[Employee]
GO

/****** Object:  Table [dbo].[Employee]    Script Date: 10/6/2020 2:12:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Employee](
	[EmployeeID] [uniqueidentifier] NOT NULL,
	[DepartmentID] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[MiddleName] [nvarchar](100) NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Title] [nvarchar](100) NOT NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[Extension] [nvarchar](50) NULL,
	[FaxNumber] [nvarchar](50) NULL,
	[EmailAddress] [nvarchar](100) NULL,
	[IsActive] [bit] NOT NULL,
	[LegacyID] [nvarchar](50) NOT NULL,
	[UserID] [uniqueidentifier] NULL,
	[IsLegalVerified] [bit] NOT NULL,
	[IsSalesContact] [bit] NOT NULL,
	[IsClientServicesRep] [bit] NOT NULL,
	[IsConsultantAnalyst] [bit] NOT NULL,
	[IsPortfolioManager] [bit] NOT NULL,
	[IsSalesRep] [bit] NOT NULL,
	[IsCompliancePerson] [bit] NOT NULL,
	[CalcName] [nvarchar](302) NULL,
	[LastNameFirstName] [nvarchar](202) NULL
) ON [PRIMARY]
GO

INSERT INTO [dbo].[Employee]
           ([EmployeeID]
		   ,[DepartmentID]
		   ,[FirstName]
           ,[MiddleName]
           ,[LastName]
           ,[Title]
           ,[PhoneNumber]
           ,[Extension]
           ,[FaxNumber]
           ,[EmailAddress]
           ,[IsActive]
           ,[LegacyID]
           ,[IsLegalVerified]
           ,[IsSalesContact]
           ,[IsClientServicesRep]
           ,[IsConsultantAnalyst]
           ,[IsPortfolioManager]
           ,[IsSalesRep]
           ,[IsCompliancePerson]
           ,[CalcName]
           ,[LastNameFirstName])
     VALUES
           ('8E15AC5E-62D1-459A-97DB-05E64FA3C031',
		   '00000000-0000-0000-0000-000000000000',
		   'Carlos'
           ,'F'
           ,'Paez'
           ,'Mr'
           ,NULL
           ,NULL
           ,NULL
           ,'CarlosPaez057@gmail.com'
           ,1
           ,0
           ,1
           ,0
           ,0
           ,0
           ,1
           ,0
           ,0
           ,Null
           ,'paez.carlos')
GO