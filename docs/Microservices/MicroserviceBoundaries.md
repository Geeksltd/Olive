# Microservice Boundaries

Defining microservice boundaries is probably the first challenge anyone encounters. Each microservice has to be autonomous enough to deliver the benefits you expect to get from Microservices architecture. Yet in practice every part of a business process seems somehow depend on other parts. How do you identify those boundaries?

Each Microservice should be small. But size is not the only factor. Your goal should be to get to the most meaningful separation, guided by your domain knowledge. The emphasis is not on the size, but instead on **business capabilities**.

You need to focus on the application's logical domain models and data and try to identify **decoupled islands of data** and different contexts within the same application. 

## One microservice per business capability
As a rule of thumb you need one microservice per business capability. Examples of business capabilities are: Pricing, Custom service, Invoicing, HR management, Supply Chain management, etc.

Another factor is whether there is a high number of dependencies, which can also prompt the need for a single microservice, which means **cohesion** also determines how to break apart microservices. Such dependencies can be unknown at the beginning of the project. As you gain more knowledge about the domain, you should adapt the size of your microservice.

As explained before a good starting point would be to create a Microservice for each bounded context. 

1. Start with a business capability (or subsystem) such as Invoicing, Quotation, Inventory, etc. 
2. Identify the business entities in the context of that individual business capability. 
3. Define a bounded context (in DDD terms) for those entities.
	- Only include data properties needed by that business capability. 
	- Name entities and properties based on the concerns of that context.
4. Ensure you have a clear boundary for that Microservice. Your aim should be to achieve a good level of autonomy for each microservice. 
	- Of course it's not always possible to have 100% autonomy as your service can depend on data and functionality from other subsystems or business capabilities. 
	- Establish the integration requirements and API contracts across microservices.

### How to tell if you got the boundaries right?
You will know that you designed the right boundaries and sizes of each BC and domain model if you have **few strong relationships** between domain models, and you do not usually need to merge information from multiple domain models when performing typical application operations. 

Aim to achieve an autonomous BC per microservice, as isolated as possible, that enables you to work without having to constantly switch to other microservices' models.

## Naming things in each microservice Domain
The names of entities used in different contexts might be similar, or different. The same real world business concept can have different names in different application / service contexts. When designing a large application, you will see how its domain model can be fragmented. For instance, a person can be referred as a "user" in the identity or membership context, as a "customer" in a CRM context, as a "buyer" in an ordering context, and so forth.

When speaking to domain expert (business users) from different domains (different departments) the same real world things may be referred to with different terms. Each may have a different set of data requirements such as data fields. They may even be different and sometimes conflicting rules. For example the address field may be optional from the viewpoint of the sales domain, while it's mandatory for logistics people. 

It is very hard to disambiguate all domain terms and rules across all the domains related to a large application. But the most important thing is that you should not try to unify the terms and rules; instead, accept the differences and richness provided by each domain. If you try to have a unified database for the whole application, attempts at a unified vocabulary will be awkward and will not sound right to any of the multiple domain experts. Therefore, BCs (implemented as microservices) will help you to clarify where you can use certain domain terms and where you will need to split the system and create additional BCs with different domains.

## Shared Entities across microservices 
When you break a large system into microservices there will be some entities that are present just in a single microservice model. They are straightforward to implement. 

However, you will also have entities that have a different shape but share the same identity across the multiple domain models from multiple microservices. For each such entity there can be data fields that are shared among multiple microservices or fields dedicated to one. Often a shared GUID based ID value will provide the mapping across distributed data sources. 

### Example
In the following example the user entity is present in the Conferences Management microservice model as the User entity and is also present in the form of the Buyer entity in the Pricing microservice, with alternate attributes or details. 

![Decomposing a traditional data](https://user-images.githubusercontent.com/1321544/50510335-92789f80-0a9e-11e9-916a-2549144e322f.jpg)

Each microservice or BC might not need all the data related to a User entity, just part of it, depending on the problem to solve or the context. For instance, in the Pricing microservice, you do not need the address of the user, just ID (as identity) and Status, which will have an impact on discounts when pricing the seats per buyer.

The Seat entity has the same name but different attributes in each domain model. However, Seat shares identity based on the same ID, as happens with User and Buyer. 

Basically, there is a shared concept of a user that exists in multiple services. But in each domain model there might be additional or different details about the user entity. 

### Data ownership and single source of truth
You need a master microservice that owns a certain type of data per entity so that updates and queries for that type of data are driven only by that microservice. You can have read only copies of data in other services though. 