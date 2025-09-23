using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only create tables that don't exist yet
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FormSections' AND xtype='U')
                BEGIN
                    CREATE TABLE [FormSections] (
                        [Id] uniqueidentifier NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [Order] int NOT NULL,
                        [IsConditional] bit NOT NULL,
                        [ConditionExpression] nvarchar(max) NOT NULL,
                        [FormTemplateId] uniqueidentifier NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [IsDeleted] bit NOT NULL,
                        CONSTRAINT [PK_FormSections] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_FormSections_FormTemplates_FormTemplateId] FOREIGN KEY ([FormTemplateId]) REFERENCES [FormTemplates] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_FormSections_FormTemplateId] ON [FormSections] ([FormTemplateId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FormFields' AND xtype='U')
                BEGIN
                    CREATE TABLE [FormFields] (
                        [Id] uniqueidentifier NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [Label] nvarchar(max) NOT NULL,
                        [Placeholder] nvarchar(max) NOT NULL,
                        [HelpText] nvarchar(max) NOT NULL,
                        [Type] int NOT NULL,
                        [IsRequired] bit NOT NULL,
                        [IsReadOnly] bit NOT NULL,
                        [Order] int NOT NULL,
                        [ValidationRules] nvarchar(max) NOT NULL,
                        [Options] nvarchar(max) NOT NULL,
                        [DefaultValue] nvarchar(max) NOT NULL,
                        [ShowForMinor] bit NOT NULL,
                        [ShowForAdult] bit NOT NULL,
                        [FormSectionId] uniqueidentifier NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [IsDeleted] bit NOT NULL,
                        CONSTRAINT [PK_FormFields] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_FormFields_FormSections_FormSectionId] FOREIGN KEY ([FormSectionId]) REFERENCES [FormSections] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_FormFields_FormSectionId] ON [FormFields] ([FormSectionId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Documents' AND xtype='U')
                BEGIN
                    CREATE TABLE [Documents] (
                        [Id] uniqueidentifier NOT NULL,
                        [FileName] nvarchar(max) NOT NULL,
                        [OriginalFileName] nvarchar(max) NOT NULL,
                        [ContentType] nvarchar(max) NOT NULL,
                        [FileSize] bigint NOT NULL,
                        [StoragePath] nvarchar(max) NOT NULL,
                        [DocumentType] nvarchar(max) NOT NULL,
                        [IsVerified] bit NOT NULL,
                        [VerificationNotes] nvarchar(max) NOT NULL,
                        [RegistrationId] uniqueidentifier NOT NULL,
                        [UploadedByUserId] uniqueidentifier NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [IsDeleted] bit NOT NULL,
                        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Documents_Registrations_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_Documents_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([Id])
                    );

                    CREATE INDEX [IX_Documents_RegistrationId] ON [Documents] ([RegistrationId]);
                    CREATE INDEX [IX_Documents_UploadedByUserId] ON [Documents] ([UploadedByUserId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RegistrationComments' AND xtype='U')
                BEGIN
                    CREATE TABLE [RegistrationComments] (
                        [Id] uniqueidentifier NOT NULL,
                        [Content] nvarchar(max) NOT NULL,
                        [IsInternal] bit NOT NULL,
                        [RegistrationId] uniqueidentifier NOT NULL,
                        [AuthorId] uniqueidentifier NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [IsDeleted] bit NOT NULL,
                        CONSTRAINT [PK_RegistrationComments] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_RegistrationComments_Registrations_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_RegistrationComments_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_RegistrationComments_AuthorId] ON [RegistrationComments] ([AuthorId]);
                    CREATE INDEX [IX_RegistrationComments_RegistrationId] ON [RegistrationComments] ([RegistrationId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RegistrationHistories' AND xtype='U')
                BEGIN
                    CREATE TABLE [RegistrationHistories] (
                        [Id] uniqueidentifier NOT NULL,
                        [Action] nvarchar(max) NOT NULL,
                        [OldStatus] int NOT NULL,
                        [NewStatus] int NOT NULL,
                        [Details] nvarchar(max) NOT NULL,
                        [ChangedFields] nvarchar(max) NOT NULL,
                        [RegistrationId] uniqueidentifier NOT NULL,
                        [PerformedByUserId] uniqueidentifier NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [IsDeleted] bit NOT NULL,
                        CONSTRAINT [PK_RegistrationHistories] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_RegistrationHistories_Registrations_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_RegistrationHistories_Users_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users] ([Id])
                    );

                    CREATE INDEX [IX_RegistrationHistories_PerformedByUserId] ON [RegistrationHistories] ([PerformedByUserId]);
                    CREATE INDEX [IX_RegistrationHistories_RegistrationId] ON [RegistrationHistories] ([RegistrationId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name='RegistrationHistories' AND xtype='U')
                    DROP TABLE [RegistrationHistories];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name='RegistrationComments' AND xtype='U')
                    DROP TABLE [RegistrationComments];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name='Documents' AND xtype='U')
                    DROP TABLE [Documents];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name='FormFields' AND xtype='U')
                    DROP TABLE [FormFields];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name='FormSections' AND xtype='U')
                    DROP TABLE [FormSections];
            ");
        }
    }
}