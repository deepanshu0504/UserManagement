using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginandRegisterMVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingBlogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if table exists, if not create it
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Blogs')
                BEGIN
                    CREATE TABLE [Blogs] (
                        [BlogId] int NOT NULL IDENTITY(1,1),
                        [Title] nvarchar(200) NOT NULL,
                        [ShortDescription] nvarchar(500) NOT NULL,
                        [Content] nvarchar(max) NOT NULL,
                        [FeaturedImage] nvarchar(max) NOT NULL,
                        [MetaDescription] nvarchar(160) NULL,
                        [Status] int NOT NULL,
                        [CreatedDate] datetime2 NOT NULL,
                        [LastModifiedDate] datetime2 NOT NULL,
                        [PublishedDate] datetime2 NULL,
                        [ViewCount] int NOT NULL DEFAULT 0,
                        [AuthorId] nvarchar(128) NOT NULL,
                        CONSTRAINT [PK_Blogs] PRIMARY KEY ([BlogId]),
                        CONSTRAINT [FK_Blogs_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([UserId])
                    );
                END
            ");

            // Add missing columns if table exists but columns are missing
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Blogs')
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'Title')
                        ALTER TABLE [Blogs] ADD [Title] nvarchar(200) NOT NULL DEFAULT '';
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'ShortDescription')
                        ALTER TABLE [Blogs] ADD [ShortDescription] nvarchar(500) NOT NULL DEFAULT '';
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'Content')
                        ALTER TABLE [Blogs] ADD [Content] nvarchar(max) NOT NULL DEFAULT '';
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'FeaturedImage')
                        ALTER TABLE [Blogs] ADD [FeaturedImage] nvarchar(max) NOT NULL DEFAULT '';
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'MetaDescription')
                        ALTER TABLE [Blogs] ADD [MetaDescription] nvarchar(160) NULL;
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'Status')
                        ALTER TABLE [Blogs] ADD [Status] int NOT NULL DEFAULT 0;
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'CreatedDate')
                        ALTER TABLE [Blogs] ADD [CreatedDate] datetime2 NOT NULL DEFAULT GETDATE();
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'LastModifiedDate')
                        ALTER TABLE [Blogs] ADD [LastModifiedDate] datetime2 NOT NULL DEFAULT GETDATE();
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'PublishedDate')
                        ALTER TABLE [Blogs] ADD [PublishedDate] datetime2 NULL;
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'ViewCount')
                        ALTER TABLE [Blogs] ADD [ViewCount] int NOT NULL DEFAULT 0;
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Blogs') AND name = 'AuthorId')
                        ALTER TABLE [Blogs] ADD [AuthorId] nvarchar(128) NOT NULL DEFAULT '';
                END
            ");

            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Blogs_AuthorId')
                    CREATE INDEX [IX_Blogs_AuthorId] ON [Blogs] ([AuthorId]);
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Blogs_PublishedDate')
                    CREATE INDEX [IX_Blogs_PublishedDate] ON [Blogs] ([PublishedDate]);
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Blogs_Status')
                    CREATE INDEX [IX_Blogs_Status] ON [Blogs] ([Status]);
            ");

            // Add foreign key if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Blogs_Users_AuthorId')
                    ALTER TABLE [Blogs] ADD CONSTRAINT [FK_Blogs_Users_AuthorId] 
                        FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([UserId]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS [Blogs]");
        }
    }
}
