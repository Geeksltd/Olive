# Olive Encryption and Decryption

Some applications required to save data in the encrypted format for security reasons. Olive also facilitates the encryption and decryption of data.

## Implementation

There is an attribute `EncryptedProperty` which can be added to the Model Entity class.
     
## App.Config settings

There should be a key for encryption and decryption in app.config file under the Database node.

    "Database": {
	    .....
	    "DataEncryption": {
	      "Key": "abcd1234"
	    }
    }

### Example
We want to save **User's** first name in encrypted form.

    public class User : EnitiyType
    {
	    public User()
	    {
		    String("First name").Attributes("[EncryptedProperty]");
	    }
    }
    
### Generated Code

The generated code for the property.

     [EncryptedProperty]
     public string FirstName { get; set; }

### Saving Data

    Database.Save<Domain.User>(new Domain.User { FirstName = "xyz" }); // this will save data in encrypted form.
    
### Fetching Data

    Database.GetList<Domain.User>(); // data will be in decrypted form.
