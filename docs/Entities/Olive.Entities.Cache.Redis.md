# Olive.Entities.Cache.Redis

## Overview

`RedisCacheProvider` is a caching implementation utilizing Redis for efficient caching of Olive.Entities (entities) data objects. It supports storing, retrieving, and managing cached entries including individual entities and entity collections. This provider integrates seamlessly with the Olive caching infrastructure through dependency injection.

---

## Table of Contents

- [RedisCacheProvider](#rediscacheprovider)
  - [Constructor](#constructor)
  - [Methods](#methods)
- [RedisCacheExtensions](#rediscacheextensions)
- [Examples](#examples)

---

## RedisCacheProvider

### Constructor

Initializes the Redis connection using the provided Redis configuration string from settings (`Database:Cache:RedisConfig`). By default, it connects to `localhost:6379`.

**Usage:**
```csharp
var provider = new RedisCacheProvider();
```

### Methods

#### Add(IEntity entity)

Stores a single entity instance in Redis cache.

- **Parameters**
  - `entity`: An instance of IEntity to cache.

**Example:**
```csharp
provider.Add(myEntity);
```

---

#### Get(Type entityType, string id)

Retrieves a single entity instance from Redis cache.

- **Parameters**
  - `entityType`: Type of the entity to retrieve.
  - `id`: The unique identifier of the entity.

- **Returns:** The cached entity instance, or null if not found.

**Example:**
```csharp
var cachedEntity = provider.Get(typeof(User), "user-123");
```

---

#### Remove(IEntity entity)

Removes a cached entity instance from Redis.

- **Parameters**
  - `entity`: The entity instance to remove.

**Example:**
```csharp
provider.Remove(myEntity);
```

---

#### AddList(Type type, IEnumerable list)

Caches a list or collection of entities in Redis.

- **Parameters**
  - `type`: The type of entities stored.
  - `list`: An IEnumerable containing the entity instances.

**Example:**
```csharp
provider.AddList(typeof(User), usersCollection);
```

---

#### GetList(Type type)

Retrieves a cached list of entities from Redis cache.

- **Parameters**
  - `type`: The type of entities to retrieve.

- **Returns:** IEnumerable containing the entity instances, or null if not found.

**Example:**
```csharp
var users = provider.GetList(typeof(User));
```

---

#### RemoveList(Type type)

Removes a cached list of entities based on entity type.

- **Parameters**
  - `type`: The type of entities whose cache is to be cleared.

**Example:**
```csharp
provider.RemoveList(typeof(User));
```

---

#### ClearAll()

Flushes the entire Redis database, removing all keys and values.

**Warning:** This action clears all cached data. Use with caution.

**Example:**
```csharp
provider.ClearAll();
```

---

#### Remove(Type type, bool invalidateCachedReferences = false)

Removes cached entities by type. Can optionally invalidate cached references.

- **Parameters**
  - `type`: The entity type whose cached instances should be removed.
  - `invalidateCachedReferences`: (Currently unused and set as default: false)

**Example:**
```csharp
provider.Remove(typeof(User));
```

---

## RedisCacheExtensions

### AddRedisCache(this IServiceCollection services)

An extension method to easily register `RedisCacheProvider` as the application's caching provider via dependency injection, using singleton lifetime.

**Example Usage in Startup.cs:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRedisCache();
}
```

---

## Examples

### Example 1: Adding and retrieving an entity to/from Redis cache

```csharp
var redisProvider = new RedisCacheProvider();

// Add entity to cache
redisProvider.Add(userEntity);

// Retrieve entity from cache
var cachedUser = redisProvider.Get(typeof(User), userEntity.ID.ToString()) as User;
```

---

### Example 2: Caching a list of entities

```csharp
var redisProvider = new RedisCacheProvider();
var userList = new List<User> { user1, user2, user3 };

// Cache list
redisProvider.AddList(typeof(User), userList);

// Get cached list
var cachedUsers = redisProvider.GetList(typeof(User)) as IEnumerable<User>;
```

---

### Example 3: Registering RedisCacheProvider in Startup.cs

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Redis caching provider
        services.AddRedisCache();

        // Other configurations...
    }
}
```

---

## Notes:

- Internally, objects are serialized using binary formatter. Ensure types are serializable when cached.
- Redis server configuration can be provided through application's configuration file under the key:

```json
{
  "Database": {
    "Cache": {
      "RedisConfig": "127.0.0.1:6379"
    }
  }
}
```

- Default Redis configuration: `localhost:6379`
- Redis connection is initialized with `allowAdmin=true`, which enables administration commands like flushing database.

**Special Consideration (Web Farms):**

- The row version functionality (`IsUpdatedSince`, `UpdateRowVersion`) isn't supported in RedisCacheProvider and will return default or no operation.