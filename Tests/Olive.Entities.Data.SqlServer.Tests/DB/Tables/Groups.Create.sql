-- Groups Table ========================
CREATE TABLE Groups (
    Id uniqueidentifier PRIMARY KEY NONCLUSTERED,
    Name nvarchar(200)  NOT NULL,
    DateCreated datetime2  NOT NULL
);

