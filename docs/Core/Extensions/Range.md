# Range Extension Methods
> `Range` extension methods are really useful in all frameworks for working with 'Random' objects.
> `Olive` has various methods which you can use in your applications.

## NextBoolean({chance})
Gets a random boolean value with the specified chance (0-100).
'Chance' should be between 0 and 100 percent.
#### When to use it?
When you want to get a random 'boolean' value with the specified 'chance' (0-100) in your applications.

#### Example:
```csharp
var rand = new Random();
rand.NextBoolean(); // Returns True or false randomly by 50% chance to be False or true.

rand.NextBoolean(90); // Returns almost True because chance is 90 percent.

rand.NextBoolean(10); // Returns almost False because chance is 10 percent.

rand.NextBoolean(0); // Returns always False because chance is zero percent.

rand.NextBoolean(100); // Returns always True because chance is 100 percent.
```

## NextAlphaNumericString({length},{omitConfusableCharacters})
Generates and returns a random alphanumeric 'string'.
{length} is Length of 'string' to return.
{omitConfusableCharacters} pass 'true' to miss-out letters that can be confused with numbers (BDIOS).
#### When to use it?
When you want to generate and return a random alphanumeric 'string' in your applications.
It returns 'string' instance containing random alphanumeric characters.
#### Example:
```csharp
var rand = new Random();
rand.NextAlphaNumericString(1); // Returns a random character

rand.NextAlphaNumericString(20); // Returns a string with 20 character lenght such as:"8Z5JKP29G98B8DQN3P9Y"

rand.NextAlphaNumericString(20); // Returns a string with 20 character lenght such as:"X86MVMNVLAYAHXTV2YY5" with (BDIOS) letters.
```

## PickNumbers({quantity},{minValue},{maxValue})
Returns {quantity} number of unique random integers within the given range.
{minValue} is the floor of range.
{maxValue} is the ceil of range.
#### When to use it?
When you want to return {quantity} number of unique random integers within the given range.
#### Example:
```csharp
var rand = new Random();
rand.PickNumbers(10, 1000, 5000); // returns a list of 10 items of random numbers between 1000 to 5000
rand.PickNumbers(10, 5000, 1000); // Throws an error: "Invalid min and Max value specified."
```
