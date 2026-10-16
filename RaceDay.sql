-- ============================================================
-- RaceDay Database
-- SQL Server
-- Based on the RaceDay ERD
-- ============================================================


-- ============================================================
-- CREATE DATABASE
-- ============================================================

CREATE DATABASE RaceDay ;

USE RaceDay ;

GO


-- ============================================================
-- 1. ORGANISER
-- ============================================================

CREATE TABLE Organiser
(
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,

    FirstName VARCHAR(50) NOT NULL,

    LastName VARCHAR(50) NOT NULL,

    Email VARCHAR(100) NOT NULL UNIQUE,

    PasswordHash VARCHAR(255) NOT NULL,

    PhoneNumber VARCHAR(20),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

GO


-- ============================================================
-- 2. ORGANISER_PROFILE
-- One Organiser has one Organiser Profile
-- ============================================================

CREATE TABLE Organiser_Profile
(
    OrganiserProfileID INT IDENTITY(1,1) PRIMARY KEY,

    OrganiserID INT NOT NULL UNIQUE,

    PhoneNumber VARCHAR(20),

    Address VARCHAR(255),

    OrganisationName VARCHAR(150),

    Bio TEXT,

    CONSTRAINT FK_OrganiserProfile_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES Organiser(OrganiserID)
        ON DELETE CASCADE
);

GO


-- ============================================================
-- 3. PARTICIPANT
-- ============================================================

CREATE TABLE Participant
(
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,

    FirstName VARCHAR(50) NOT NULL,

    LastName VARCHAR(50) NOT NULL,

    Email VARCHAR(100) NOT NULL UNIQUE,

    PasswordHash VARCHAR(255) NOT NULL,

    PhoneNumber VARCHAR(20),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

GO


-- ============================================================
-- 4. PARTICIPANT_PROFILE
-- One Participant has one Participant Profile
-- ============================================================

CREATE TABLE Participant_Profile
(
    ParticipantProfileID INT IDENTITY(1,1) PRIMARY KEY,

    ParticipantID INT NOT NULL UNIQUE,

    DateOfBirth DATE,

    Address VARCHAR(255),

    EmergencyContactName VARCHAR(100),

    EmergencyContactPhone VARCHAR(20),

    Gender VARCHAR(20),

    CONSTRAINT CK_ParticipantProfile_Gender
        CHECK (Gender IN ('Male', 'Female', 'Other')),

    CONSTRAINT FK_ParticipantProfile_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Participant(ParticipantID)
        ON DELETE CASCADE
);

GO


-- ============================================================
-- 5. EVENT
-- An Organiser can create many Events
-- ============================================================

CREATE TABLE [Event]
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,

    OrganiserID INT NOT NULL,

    EventName VARCHAR(150) NOT NULL,

    Description VARCHAR(MAX),

    EventDate DATE NOT NULL,

    StartTime TIME NOT NULL,

    Location VARCHAR(150) NOT NULL,

    Distance VARCHAR(50),

    Status VARCHAR(20) NOT NULL DEFAULT 'Upcoming',

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT CK_Event_Status
        CHECK (Status IN
        (
            'Upcoming',
            'Ongoing',
            'Completed',
            'Cancelled'
        )),

    CONSTRAINT FK_Event_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES Organiser(OrganiserID)
        ON DELETE NO ACTION
);

GO


-- ============================================================
-- 6. CATEGORY
-- ============================================================

CREATE TABLE Category
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,

    CategoryName VARCHAR(100) NOT NULL UNIQUE,

    Description VARCHAR(MAX),

    Type VARCHAR(20) NOT NULL,

    CONSTRAINT CK_Category_Type
        CHECK (Type IN ('Run', 'Walk', 'Cycle'))
);

GO


-- ============================================================
-- 7. EVENT_CATEGORY
-- Resolves the many-to-many relationship
-- between Event and Category
-- ============================================================

CREATE TABLE Event_Category
(
    EventCategoryID INT IDENTITY(1,1) PRIMARY KEY,

    EventID INT NOT NULL,

    CategoryID INT NOT NULL,

    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,

    MaximumParticipants INT,

    AgeRestriction VARCHAR(100) NULL,

    GenderRestriction VARCHAR(20) NULL,

    CONSTRAINT FK_EventCategory_Event
        FOREIGN KEY (EventID)
        REFERENCES [Event](EventID)
        ON DELETE CASCADE,

    CONSTRAINT FK_EventCategory_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Category(CategoryID)
        ON DELETE NO ACTION,

    CONSTRAINT UQ_EventCategory
        UNIQUE (EventID, CategoryID),

    CONSTRAINT CK_EventCategory_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT CK_EventCategory_MaxParticipants
        CHECK (MaximumParticipants IS NULL
               OR MaximumParticipants > 0)
);

GO


-- ============================================================
-- 8. ENROLLMENT
-- A Participant can have many enrollments
-- An Event Category can have many enrollments
-- ============================================================

CREATE TABLE Enrollment
(
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,

    ParticipantID INT NOT NULL,

    EventCategoryID INT NOT NULL,

    EnrollmentDate DATETIME NOT NULL DEFAULT GETDATE(),

    Status VARCHAR(20) NOT NULL DEFAULT 'Active',

    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Pending',

    PaymentReference VARCHAR(100) NULL,

    CONSTRAINT CK_Enrollment_Status
        CHECK (Status IN
        (
            'Active',
            'Cancelled',
            'Completed'
        )),

    CONSTRAINT CK_Enrollment_PaymentStatus
        CHECK (PaymentStatus IN
        (
            'Pending',
            'Paid',
            'Refunded'
        )),

    CONSTRAINT FK_Enrollment_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Participant(ParticipantID)
        ON DELETE NO ACTION,

    CONSTRAINT FK_Enrollment_EventCategory
        FOREIGN KEY (EventCategoryID)
        REFERENCES Event_Category(EventCategoryID)
        ON DELETE NO ACTION,

    CONSTRAINT UQ_Participant_EventCategory
        UNIQUE (ParticipantID, EventCategoryID)
);

GO


-- ============================================================
-- 9. PAYMENT
-- An Enrollment can have multiple Payment records
-- ============================================================

CREATE TABLE Payment
(
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,

    EnrollmentID INT NOT NULL,

    Amount DECIMAL(10,2) NOT NULL,

    PaymentMethod VARCHAR(20) NOT NULL,

    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),

    TransactionReference VARCHAR(100),

    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',

    CONSTRAINT CK_Payment_Amount
        CHECK (Amount >= 0),

    CONSTRAINT CK_Payment_Method
        CHECK (PaymentMethod IN
        (
            'Card',
            'EFT',
            'Cash',
            'Other'
        )),

    CONSTRAINT CK_Payment_Status
        CHECK (Status IN
        (
            'Pending',
            'Paid',
            'Failed',
            'Refunded'
        )),

    CONSTRAINT FK_Payment_Enrollment
        FOREIGN KEY (EnrollmentID)
        REFERENCES Enrollment(EnrollmentID)
        ON DELETE NO ACTION
);

GO


-- ============================================================
-- 10. RESULT
-- One Enrollment can have one Result
-- ============================================================

CREATE TABLE Result
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,

    EnrollmentID INT NOT NULL UNIQUE,

    FinishTime TIME,

    ChipTime TIME NULL,

    Position INT,

    CategoryPosition INT,

    ResultStatus VARCHAR(20) NOT NULL,

    RecordedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT CK_Result_Status
        CHECK (ResultStatus IN
        (
            'Finished',
            'DNF',
            'DNS'
        )),

    CONSTRAINT CK_Result_Position
        CHECK (Position IS NULL OR Position > 0),

    CONSTRAINT CK_Result_CategoryPosition
        CHECK (CategoryPosition IS NULL OR CategoryPosition > 0),

    CONSTRAINT FK_Result_Enrollment
        FOREIGN KEY (EnrollmentID)
        REFERENCES Enrollment(EnrollmentID)
        ON DELETE NO ACTION
);

GO

USE RaceDay;
GO

SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
    'Organiser',
    'Organiser_Profile',
    'Participant',
    'Participant_Profile',
    'Event',
    'Category',
    'Event_Category',
    'Enrollment',
    'Payment',
    'Result'
)
ORDER BY TABLE_NAME, ORDINAL_POSITION;