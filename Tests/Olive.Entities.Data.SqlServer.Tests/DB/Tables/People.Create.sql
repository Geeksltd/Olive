-- People Table ========================
CREATE TABLE People (
    Id uniqueidentifier PRIMARY KEY NONCLUSTERED,
    FirstName nvarchar(200)  NULL,
    LastName nvarchar(200)  NULL,
    Email nvarchar(200)  NULL,
    Birthdate datetime2  NULL
);

