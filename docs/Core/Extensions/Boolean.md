# Boolean Extension Methods
>These are a bunch of useful extension methods related to `Boolean` objects which you can use in your applications.

## Tostring({trueText}, {falseText}, {nullText})
Converts the value of a boolean object to its equivalent `string` representation for the specified custom text instead of the default `"True"` or `"False"`.

- For simple `bool` objects, it's an alternative to writing `myBoolean ? "my true text" : "my false text."`.
- For `bool?` objects (nullable), it's an alternative to writing:

  `myNullableBoolean == true ? "my true text" : (myNullableBoolean == false) ? "my false text." : "my null text")`.


#### When to use it?
When you want to show the meaningful value of a boolean value in your applications.

#### Example:
```csharp
bool myVariable = true;

myVariable.ToString("Oh yes", "Oh no"); // returns "Oh yes".

myVariable = false;
myVariable.ToString("Oh yes", "Oh no"); // returns "Oh no".

bool? myNullable = null;
myNullable.ToString("Oh yes", "Oh no", "N/A"); // returns "N/A".
```

## ToYesNoString({trueText}, {falseText})
Returns `Yes` or `No` string depending on whether the result is `true` of `false`. It returns `Empty string` if object is `null`.

#### When to use it?
When you want to show the meaningful value of an object value in your applications. If the value is `true`, this method returns `string1`, otherwise, it returns `string2`.
If you don't initialize `string1` or `string2`, the default value is `Yes` or `No`.
#### Example:
```csharp
bool myVariable = true;
myVariable.ToYesNoString("Oh yes", "Oh no"); // returns "Oh yes".

myVariable = false;
myVariable.ToYesNoString("Oh yes", "Oh no"); // returns "Oh no".

myVariable = true;
myVariable.ToYesNoString(); // returns "Yes".

myVariable = false;
myVariable.ToYesNoString(); // returns "No".

bool? myNullable = null;
myNullable.ToYesNoString(); // returns "N/A".
```


## CompareTo({ComparedBoolean})
Compare two `boolean` object and returns 0 if both are equal.

#### When to use it?
When You want to compare two `Boolean` Object value in your codes.
#### Example:
```csharp
bool myVariable = true;
myVariable.CompareTo(true).ToString()); //returns 0

myVariable = true;
myVariable.CompareTo(false).ToString()); //returns 1

myVariable = false;
myVariable.CompareTo(false).ToString()); //returns 0

myVariable = false;
myVariable.CompareTo(true).ToString()); //returns 1

myVariable = true;
myVariable.CompareTo(null).ToString()); //returns 1

bool? myNullable = null;
myNullable.CompareTo(false).ToString()); //returns 1

myNullable = null;
myVariable.CompareTo(true).ToString()); //returns -1

myVariable = false;
myVariable.CompareTo(null).ToString()); //returns -1

myNullable = null;
myVariable.CompareTo(null).ToString()); //returns 0
```
