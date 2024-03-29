﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "AspNetRoles" (
    "Id" text NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetUsers" (
    "Id" text NOT NULL,
    "ExpirationDate" timestamp with time zone,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

CREATE TABLE "Assignments" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "AcceptedFileTypes" character varying(100) NOT NULL,
    "MaxFileSize" integer NOT NULL,
    "TimePublished" timestamp with time zone,
    "TimeClosed" timestamp with time zone,
    "TeacherName" text,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Assignments" PRIMARY KEY ("Id")
);

CREATE TABLE "Files" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(1000) NOT NULL,
    "ContentType" character varying(255),
    "Size" bigint NOT NULL,
    "TimeUploaded" timestamp with time zone NOT NULL,
    "OwnerName" text,
    CONSTRAINT "PK_Files" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "RoleId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "UserId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Consultations" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "AssignmentId" integer NOT NULL,
    "StudentName" text,
    "TimeCreated" timestamp with time zone NOT NULL,
    "TimeCompleted" timestamp with time zone,
    "Feedback" text,
    "FeedbackRating" integer,
    "FeedbackComments" text,
    "Model" character varying(100),
    "PromptTokens" integer,
    "CompletionTokens" integer,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Consultations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Consultations_Assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES "Assignments" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FileContent" (
    "Id" integer NOT NULL,
    "Text" text,
    CONSTRAINT "PK_FileContent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FileContent_Files_Id" FOREIGN KEY ("Id") REFERENCES "Files" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ConsultationFiles" (
    "ConsultationId" integer NOT NULL,
    "FilesId" integer NOT NULL,
    CONSTRAINT "PK_ConsultationFiles" PRIMARY KEY ("ConsultationId", "FilesId"),
    CONSTRAINT "FK_ConsultationFiles_Consultations_ConsultationId" FOREIGN KEY ("ConsultationId") REFERENCES "Consultations" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ConsultationFiles_Files_FilesId" FOREIGN KEY ("FilesId") REFERENCES "Files" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE INDEX "IX_ConsultationFiles_FilesId" ON "ConsultationFiles" ("FilesId");

CREATE INDEX "IX_Consultations_AssignmentId" ON "Consultations" ("AssignmentId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240219232439_InitialSchema', '8.0.1');

COMMIT;

