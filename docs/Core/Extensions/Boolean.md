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
myVariable.ToString("Oh yes", "Oh no", "N/A"); // returns "N/A".
```

## ToYesNoString()
Returns `Yes` or `No` string depending on whether the result is `true` of `false`. It returns `Empty string`, if object is `null`.
#### When to use it?
When you want to show the meaningful value of an object value in your applications. If the value is `true`, this method returns `string1`, otherwise, it returns `string2`.
If you don't initialize `string1` or `string2`, the default value is `Yes` or `No`.
#### Format:
string **ToYesNoString**(string1, string2 )
#### Example:
|OBJECT| INPUT |OUTPUT |
|------------|-------------------|-----------------------------|
|true |"ACCEPT","REJECT" |"ACCEPT" |
|false |"ACCEPT","REJECT" |"REJECT" |
|true |"","" |"Yes" |
|false |"","" |"No" |
|Null |"A","B" |`Empty` |
|Null |"","B" |`Empty` |
|Null |"A","" |`Empty` |
|Null |"","" |`Empty` |

## CompareTo()
Compare two `boolean` object and returns 0 if both are equal.
#### When to use it?
When You want to compare two `Boolean` Object value in your codes.
#### Format:
int **CompareTo**(boolean)
#### Example:
|OBJECT| INPUT |OUTPUT |
|------------|-------------------|-----------------------------|
|true |true |0 |
|true |false |1 |
|false |false |0 |
|false |true |1 |
|true |null |1 |
|Null |false |1 |
|Null |true |-1 |
|false |Null |-1 |
|Null |Null |0 |