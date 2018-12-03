ALTER TABLE GroupMembers ADD Constraint
                [FK_GroupMember.Group]
                FOREIGN KEY ([Group])
                REFERENCES Groups (ID)
                ON DELETE NO ACTION;
GO
ALTER TABLE GroupMembers ADD Constraint
                [FK_GroupMember.Person]
                FOREIGN KEY (Person)
                REFERENCES People (ID)
                ON DELETE NO ACTION;
GO