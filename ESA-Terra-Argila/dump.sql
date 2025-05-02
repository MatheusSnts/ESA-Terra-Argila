IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Street] nvarchar(max) NULL,
    [StreetNumber] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [ZipCode] nvarchar(max) NULL,
    [Website] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [AcceptedByAdmin] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [DeletedAt] datetime2 NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Invitations] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(max) NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [ExpirationDate] datetime2 NOT NULL,
    [Used] bit NOT NULL,
    CONSTRAINT [PK_Invitations] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [LogEntries] (
    [Id] int NOT NULL IDENTITY,
    [UserEmail] nvarchar(max) NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [Ip] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_LogEntries] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Reference] nvarchar(50) NOT NULL,
    [UserId] nvarchar(450) NULL,
    [Name] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);
GO

CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Tags] (
    [Id] int NOT NULL IDENTITY,
    [Reference] nvarchar(50) NOT NULL,
    [UserId] nvarchar(450) NULL,
    [Name] nvarchar(100) NOT NULL,
    [IsPublic] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tags_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);
GO

CREATE TABLE [UserActivities] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [ActivityType] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NOT NULL,
    [IpAddress] nvarchar(50) NULL,
    [UserAgent] nvarchar(255) NULL,
    [IsSuccess] bit NOT NULL,
    [AdditionalInfo] nvarchar(500) NULL,
    CONSTRAINT [PK_UserActivities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserActivities_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Items] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [CategoryId] int NULL,
    [Name] nvarchar(100) NOT NULL,
    [Reference] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Price] real NOT NULL,
    [Stock] real NOT NULL,
    [Unit] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [DeletedAt] datetime2 NULL,
    [IsSustainable] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Discriminator] nvarchar(8) NOT NULL,
    CONSTRAINT [PK_Items] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Items_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Items_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [Amount] real NOT NULL,
    [PaymentDateTime] datetime2 NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ItemImages] (
    [Id] int NOT NULL IDENTITY,
    [ItemId] int NULL,
    [Name] nvarchar(max) NOT NULL,
    [Path] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ItemImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ItemImages_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MaterialTags] (
    [MaterialsId] int NOT NULL,
    [TagsId] int NOT NULL,
    CONSTRAINT [PK_MaterialTags] PRIMARY KEY ([MaterialsId], [TagsId]),
    CONSTRAINT [FK_MaterialTags_Items_MaterialsId] FOREIGN KEY ([MaterialsId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MaterialTags_Tags_TagsId] FOREIGN KEY ([TagsId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [OrderItems] (
    [Id] int NOT NULL IDENTITY,
    [Quantity] real NOT NULL,
    [OrderId] int NULL,
    [ItemId] int NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderItems_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [ProductMaterials] (
    [ProductId] int NOT NULL,
    [MaterialId] int NOT NULL,
    [Quantity] real NOT NULL,
    [Stock] real NOT NULL DEFAULT CAST(0 AS real),
    CONSTRAINT [PK_ProductMaterials] PRIMARY KEY ([ProductId], [MaterialId]),
    CONSTRAINT [FK_ProductMaterials_Items_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [Items] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductMaterials_Items_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Items] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ProductTags] (
    [ProductsId] int NOT NULL,
    [TagsId] int NOT NULL,
    CONSTRAINT [PK_ProductTags] PRIMARY KEY ([ProductsId], [TagsId]),
    CONSTRAINT [FK_ProductTags_Items_ProductsId] FOREIGN KEY ([ProductsId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductTags_Tags_TagsId] FOREIGN KEY ([TagsId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [StockMovements] (
    [Id] int NOT NULL IDENTITY,
    [ItemId] int NOT NULL,
    [Quantity] real NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [Date] datetime2 NOT NULL,
    [UserId] nvarchar(450) NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StockMovements_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_StockMovements_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserMaterialFavorites] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [MaterialId] int NOT NULL,
    CONSTRAINT [PK_UserMaterialFavorites] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserMaterialFavorites_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserMaterialFavorites_Items_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [Items] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Categories_UserId] ON [Categories] ([UserId]);
GO

CREATE INDEX [IX_ItemImages_ItemId] ON [ItemImages] ([ItemId]);
GO

CREATE INDEX [IX_Items_CategoryId] ON [Items] ([CategoryId]);
GO

CREATE INDEX [IX_Items_UserId] ON [Items] ([UserId]);
GO

CREATE INDEX [IX_MaterialTags_TagsId] ON [MaterialTags] ([TagsId]);
GO

CREATE INDEX [IX_OrderItems_ItemId] ON [OrderItems] ([ItemId]);
GO

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
GO

CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
GO

CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
GO

CREATE INDEX [IX_ProductMaterials_MaterialId] ON [ProductMaterials] ([MaterialId]);
GO

CREATE INDEX [IX_ProductTags_TagsId] ON [ProductTags] ([TagsId]);
GO

CREATE INDEX [IX_StockMovements_ItemId] ON [StockMovements] ([ItemId]);
GO

CREATE INDEX [IX_StockMovements_UserId] ON [StockMovements] ([UserId]);
GO

CREATE INDEX [IX_Tags_UserId] ON [Tags] ([UserId]);
GO

CREATE INDEX [IX_UserActivities_UserId] ON [UserActivities] ([UserId]);
GO

CREATE INDEX [IX_UserMaterialFavorites_MaterialId] ON [UserMaterialFavorites] ([MaterialId]);
GO

CREATE INDEX [IX_UserMaterialFavorites_UserId] ON [UserMaterialFavorites] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250415164645_migration1', N'8.0.11');
GO

COMMIT;
GO

