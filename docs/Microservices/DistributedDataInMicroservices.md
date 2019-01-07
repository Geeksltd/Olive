# Distributed Data in Microservices 

## Data sovereignty per microservice 
An important rule for microservices architecture is that each microservice must own its domain data and logic under an autonomous lifecycle. 

A business usually has different functional concerns in relation to real world entities. For example for the concept of "Customer" in a business, there are different departments and business processes, each thinking about the "Customer" from their own angle. For example, the sales and CRM function is concerned with the customer's contact details, inquiry details, and perhaps an overall spending history. But the logistics business processes are concerned about each particular order, its items, delivery address, etc.

In the traditional (monolithic data) approach you have a single centralized database. This is often a normalized SQL database that is used for the whole application and all its internal subsystems. What you often see in these cases is that certain key entities have a lot of data fields, each related to one or two areas of the overall system. 

But in microservices, each service is responsible for its own data. It can have database tables and business entities which exclusively exist in that service, but also it can have a partition of data for shared business entities. For example one microservice can own the data related to certain fields of a Customer while across all services you may have 5 different tables all related to Customer, but each with different fields and concerns. 

This principle is similar in [Domain-driven design (DDD)](https://en.wikipedia.org/wiki/Domain-driven_design), where each [Bounded Context](https://martinfowler.com/bliki/BoundedContext.html) or autonomous subsystem or service must own its domain model (data plus logic and behavior). Each DDD Bounded Context correlates to one business microservice (one or several services) using a unique ID (often GUID) that links everything together logically. (We expand on this point about the Bounded Context pattern in the next section.) 

![annotation 2018-12-28 120724](https://user-images.githubusercontent.com/1321544/50508946-39f2d380-0a99-11e9-8daa-ef680d8662ca.jpg)

The centralized database approach initially looks simpler and seems to enable reuse of entities in different subsystems to make everything consistent. But the reality is you end up with huge tables that serve many different subsystems, and that include attributes and columns that are not needed in most cases.

## Diverse database technologies
Different microservices can use different kinds of databases. For some use cases, a NoSQL database such as MongoDB might have a more convenient data model and offer better performance and scalability than SQL Server. In other cases, a relational database is still the best approach. Therefore, microservices-based applications can use a mixture of SQL and NoSQL databases, which is sometimes called the [polyglot persistence](https://martinfowler.com/bliki/PolyglotPersistence.html) approach. 

## Challenges of data distribution 
A partitioned, polyglot-persistence architecture for data storage has many benefits. These include loosely coupled services and better performance, scalability, costs, and manageability. However, it can introduce some distributed data management challenges.

A monolithic app with typically a single relational database has two important benefits. They both work across all the tables and data related to your application:

- [ACID transactions](https://en.wikipedia.org/wiki/ACID), to make complex data changes across multiple tables atomically. 
- SQL language, to easily write a query that combines data from multiple tables. 

You lose that luxury in microservices architecture. Distributed data structures cannot make a single ACID transaction across microservices. This means you must use **eventual consistency** when a business process spans multiple microservices, which is much harder to implement. You can implement it through event-driven communication and a publish-and-subscribe system. These topics are covered later. 

### Querying data across several microservices
In a distributed model, a common challenge is how to implement queries that retrieve data from several microservices, while avoiding **chatty communication (sending data back and forth, over and over)** to the microservices from remote client apps. Consider a single screen in a mobile app that needs to show information that is owned by the basket, catalog, and user identity microservices. Instead of sending a request from the mobile app to each of the microservices, you can aggregate the information to improve the efficiency in the communications of your system. The most popular solutions are the following.

**API Gateway**: For simple data aggregation from multiple microservices you can create an API Gateway. Beware of creating a central big Api Gateway that acts as a hub for everything, because it would be a choke point in your system and can violate the principle of microservice autonomy. Instead you can create multiple fined-grained API Gateways each focusing on one vertical "slice" or business area of the system. 

**CQRS:** It stands for Command and Query Responsibility Segregation. Basically it means that your database for queries (reads) can be different from your database for writes (commands). In this model you have normalised tables (each owned by a microservice) where Commands (i.e. changes to data) are executed, so that you have a single consistent source of original data. But you also have tables which have aggregated data in a format that is needed by the application UI, often with a one to one mapping between the screen fields and table columns.

With CQRS, you generate, in advance, a special read-only table with the data that is owned by multiple microservices. The table has a format suited to the client app's needs and is often denormalized. 

CQRS has better performance and reliability than API Gateway, because the data is already locally available to return, without needing to query the other microservices. But CQRS is harder to implement and keep synced.

### Reporting with COLD DATA
For complex reports and queries that might not require real-time data, a common approach is to export your "hot data" (transactional data from the microservices) as "cold data" into large databases that are used only for reporting. The original updates and transactions, as your source of truth, have to remain in your microservices data.

The way you would synchronize data would be either by using event-driven communication (covered in the next sections) or by using other database import/export tools.

### Dealing with Failure: Sync vs Async
In a distributed system with many artifacts across many servers, components will eventually fail and you should take it into account when designing your microservices and their communication process.

A simple approach is to implement and call HTTP based APIs. But if you create long chains of synchronous HTTP calls across microservices (as if the microservices were objects in a monolithic application) you will eventually run into problems.

- Due to the synchronous nature of HTTP, the original request will not get a response until all the internal HTTP calls are finished. If any of the intermediate HTTP calls is blocked for some time then everyone is blocked, which slows down the whole system. 
- If one microservice fails, or is down, everyone in the chain will fail. Microservices should not be too coupled with other microservices. Otherwise they lose their autonomy, which affects their availability. 
- A microservice-based system should be designed to continue to work as well as possible during partial failures.

To avoid these problems you should use asynchronous interaction between microservices, either by using:

- Message and event-based communication (e.g. using RabbitMQ); or
- Regular HTTP polling, to
	- Bring in and save/cache remote data locally (for GET requests)
	- Use a retry mechanism for other http requests (POST, DELETE, etc).

