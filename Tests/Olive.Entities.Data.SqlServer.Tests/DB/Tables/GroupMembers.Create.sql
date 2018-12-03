-- GroupMembers Table ========================
CREATE TABLE GroupMembers (
    Id uniqueidentifier PRIMARY KEY NONCLUSTERED,
    [Group] uniqueidentifier  NULL,
    Person uniqueidentifier  NULL,
    DateRegistered datetime2  NOT NULL
);
CREATE INDEX [IX_GroupMembers->Group] ON GroupMembers ([Group]);

GO

