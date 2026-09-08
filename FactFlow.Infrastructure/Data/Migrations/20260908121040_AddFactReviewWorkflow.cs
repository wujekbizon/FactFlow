using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFactReviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Facts', N'ReviewNote') IS NULL
                    ALTER TABLE [dbo].[Facts] ADD [ReviewNote] nvarchar(2000) NULL;

                IF COL_LENGTH(N'dbo.Facts', N'ReviewStatus') IS NULL
                    ALTER TABLE [dbo].[Facts] ADD [ReviewStatus] nvarchar(20) NOT NULL
                        CONSTRAINT [DF_Facts_ReviewStatus] DEFAULT N'New';

                IF COL_LENGTH(N'dbo.Facts', N'ReviewedAtUtc') IS NULL
                    ALTER TABLE [dbo].[Facts] ADD [ReviewedAtUtc] datetimeoffset NULL;

                IF COL_LENGTH(N'dbo.Facts', N'ReviewedBy') IS NULL
                    ALTER TABLE [dbo].[Facts] ADD [ReviewedBy] nvarchar(200) NULL;

                IF OBJECT_ID(N'[dbo].[FactReviewAudit]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[FactReviewAudit]
                    (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [FactId] int NOT NULL,
                        [FromStatus] nvarchar(20) NOT NULL,
                        [ToStatus] nvarchar(20) NOT NULL,
                        [ReviewedBy] nvarchar(200) NOT NULL,
                        [Note] nvarchar(2000) NULL,
                        [OccurredAtUtc] datetimeoffset NOT NULL,
                        CONSTRAINT [PK_FactReviewAudit] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_FactReviewAudit_Facts_FactId]
                            FOREIGN KEY ([FactId]) REFERENCES [dbo].[Facts] ([Id])
                    );

                    CREATE INDEX [IX_FactReviewAudit_FactId_OccurredAtUtc]
                        ON [dbo].[FactReviewAudit] ([FactId], [OccurredAtUtc]);
                END;

                DECLARE @DataverseDefaultConstraint sysname;
                SELECT @DataverseDefaultConstraint = dc.[name]
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON c.[default_object_id] = dc.[object_id]
                INNER JOIN sys.tables t ON t.[object_id] = c.[object_id]
                INNER JOIN sys.schemas s ON s.[schema_id] = t.[schema_id]
                WHERE s.[name] = N'dbo'
                  AND t.[name] = N'Facts'
                  AND c.[name] = N'DataverseSyncStatus';

                IF @DataverseDefaultConstraint IS NOT NULL
                    EXEC(N'ALTER TABLE [dbo].[Facts] DROP CONSTRAINT [' + @DataverseDefaultConstraint + N']');

                IF COL_LENGTH(N'dbo.Facts', N'DataverseRecordId') IS NOT NULL
                    ALTER TABLE [dbo].[Facts] DROP COLUMN [DataverseRecordId];
                IF COL_LENGTH(N'dbo.Facts', N'DataverseSyncError') IS NOT NULL
                    ALTER TABLE [dbo].[Facts] DROP COLUMN [DataverseSyncError];
                IF COL_LENGTH(N'dbo.Facts', N'DataverseSyncStatus') IS NOT NULL
                    ALTER TABLE [dbo].[Facts] DROP COLUMN [DataverseSyncStatus];
                IF COL_LENGTH(N'dbo.Facts', N'DataverseSyncedAtUtc') IS NOT NULL
                    ALTER TABLE [dbo].[Facts] DROP COLUMN [DataverseSyncedAtUtc];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[FactReviewAudit]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[FactReviewAudit];

                DECLARE @ReviewDefaultConstraint sysname;
                SELECT @ReviewDefaultConstraint = dc.[name]
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON c.[default_object_id] = dc.[object_id]
                INNER JOIN sys.tables t ON t.[object_id] = c.[object_id]
                INNER JOIN sys.schemas s ON s.[schema_id] = t.[schema_id]
                WHERE s.[name] = N'dbo'
                  AND t.[name] = N'Facts'
                  AND c.[name] = N'ReviewStatus';

                IF @ReviewDefaultConstraint IS NOT NULL
                    EXEC(N'ALTER TABLE [dbo].[Facts] DROP CONSTRAINT [' + @ReviewDefaultConstraint + N']');

                IF COL_LENGTH(N'dbo.Facts', N'ReviewNote') IS NOT NULL
                    ALTER TABLE [dbo].[Facts] DROP COLUMN [ReviewNote];
                IF COL_LENGTH(N'dbo.Facts', N'ReviewStatus') IS NOT NULL
                    ALTER TABLE [dbo].[Facts] DROP COLUMN [ReviewStatus];
                IF COL_LENGTH(N'dbo.Facts', N'ReviewedAtUtc') IS NOT NULL
                    ALTER TABLE [dbo].[Facts] DROP COLUMN [ReviewedAtUtc];
                IF COL_LENGTH(N'dbo.Facts', N'ReviewedBy') IS NOT NULL
                    ALTER TABLE [dbo].[Facts] DROP COLUMN [ReviewedBy];
                """);
        }
    }
}
