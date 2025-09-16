using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDIA.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Registration_RegistrationId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_FormSection_FormTemplate_FormTemplateId",
                table: "FormSection");

            migrationBuilder.DropForeignKey(
                name: "FK_FormTemplate_Organizations_OrganizationId",
                table: "FormTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_FormTemplate_FormTemplateId",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Organizations_OrganizationId",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Users_AssignedToUserId",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationComment_Registration_RegistrationId",
                table: "RegistrationComment");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationComment_Users_AuthorId",
                table: "RegistrationComment");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationHistory_Registration_RegistrationId",
                table: "RegistrationHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegistrationComment",
                table: "RegistrationComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Registration",
                table: "Registration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FormTemplate",
                table: "FormTemplate");

            migrationBuilder.RenameTable(
                name: "RegistrationComment",
                newName: "RegistrationComments");

            migrationBuilder.RenameTable(
                name: "Registration",
                newName: "Registrations");

            migrationBuilder.RenameTable(
                name: "FormTemplate",
                newName: "FormTemplates");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrationComment_RegistrationId",
                table: "RegistrationComments",
                newName: "IX_RegistrationComments_RegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrationComment_AuthorId",
                table: "RegistrationComments",
                newName: "IX_RegistrationComments_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_OrganizationId",
                table: "Registrations",
                newName: "IX_Registrations_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_FormTemplateId",
                table: "Registrations",
                newName: "IX_Registrations_FormTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_AssignedToUserId",
                table: "Registrations",
                newName: "IX_Registrations_AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FormTemplate_OrganizationId",
                table: "FormTemplates",
                newName: "IX_FormTemplates_OrganizationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegistrationComments",
                table: "RegistrationComments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Registrations",
                table: "Registrations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FormTemplates",
                table: "FormTemplates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Registrations_RegistrationId",
                table: "Document",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormSection_FormTemplates_FormTemplateId",
                table: "FormSection",
                column: "FormTemplateId",
                principalTable: "FormTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormTemplates_Organizations_OrganizationId",
                table: "FormTemplates",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationComments_Registrations_RegistrationId",
                table: "RegistrationComments",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationComments_Users_AuthorId",
                table: "RegistrationComments",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationHistory_Registrations_RegistrationId",
                table: "RegistrationHistory",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_FormTemplates_FormTemplateId",
                table: "Registrations",
                column: "FormTemplateId",
                principalTable: "FormTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Organizations_OrganizationId",
                table: "Registrations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Users_AssignedToUserId",
                table: "Registrations",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Registrations_RegistrationId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_FormSection_FormTemplates_FormTemplateId",
                table: "FormSection");

            migrationBuilder.DropForeignKey(
                name: "FK_FormTemplates_Organizations_OrganizationId",
                table: "FormTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationComments_Registrations_RegistrationId",
                table: "RegistrationComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationComments_Users_AuthorId",
                table: "RegistrationComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationHistory_Registrations_RegistrationId",
                table: "RegistrationHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_FormTemplates_FormTemplateId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Organizations_OrganizationId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Users_AssignedToUserId",
                table: "Registrations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Registrations",
                table: "Registrations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegistrationComments",
                table: "RegistrationComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FormTemplates",
                table: "FormTemplates");

            migrationBuilder.RenameTable(
                name: "Registrations",
                newName: "Registration");

            migrationBuilder.RenameTable(
                name: "RegistrationComments",
                newName: "RegistrationComment");

            migrationBuilder.RenameTable(
                name: "FormTemplates",
                newName: "FormTemplate");

            migrationBuilder.RenameIndex(
                name: "IX_Registrations_OrganizationId",
                table: "Registration",
                newName: "IX_Registration_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Registrations_FormTemplateId",
                table: "Registration",
                newName: "IX_Registration_FormTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_Registrations_AssignedToUserId",
                table: "Registration",
                newName: "IX_Registration_AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrationComments_RegistrationId",
                table: "RegistrationComment",
                newName: "IX_RegistrationComment_RegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrationComments_AuthorId",
                table: "RegistrationComment",
                newName: "IX_RegistrationComment_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_FormTemplates_OrganizationId",
                table: "FormTemplate",
                newName: "IX_FormTemplate_OrganizationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Registration",
                table: "Registration",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegistrationComment",
                table: "RegistrationComment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FormTemplate",
                table: "FormTemplate",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Registration_RegistrationId",
                table: "Document",
                column: "RegistrationId",
                principalTable: "Registration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormSection_FormTemplate_FormTemplateId",
                table: "FormSection",
                column: "FormTemplateId",
                principalTable: "FormTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormTemplate_Organizations_OrganizationId",
                table: "FormTemplate",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_FormTemplate_FormTemplateId",
                table: "Registration",
                column: "FormTemplateId",
                principalTable: "FormTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_Organizations_OrganizationId",
                table: "Registration",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_Users_AssignedToUserId",
                table: "Registration",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationComment_Registration_RegistrationId",
                table: "RegistrationComment",
                column: "RegistrationId",
                principalTable: "Registration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationComment_Users_AuthorId",
                table: "RegistrationComment",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationHistory_Registration_RegistrationId",
                table: "RegistrationHistory",
                column: "RegistrationId",
                principalTable: "Registration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
