# Understanding Microservices

In microservices architecture a typical enterprise app will be split into many tiny apps. For example if previously you had one asp.net app with 500 pages, instead you will have 50 small apps (each called  a “microservice”) and each with approx 10 pages on average.

How you should split the overall functionality into small microservices is not about size but rather business capabilities. Later on you will learn how to make such decisions. 

When we use the term “microservice” or just “service” you can think of it as a small asp.net app with a small dedicated database and perhaps a small number of API functions. Each service runs in its own process and communicates with other processes using HTTP or a queuing technology.

## Business case for Microservices
In short, it provides long-term agility. Microservices enable better maintainability in complex and large systems by letting you create applications based on many independently developed, tested and deployed services that each have granular and autonomous lifecycles. This enables rapid application delivery, usually with different teams focusing on different microservices.

### Issues are more isolated
If there is an issue in one service, only that service is initially impacted (except when the wrong design is used, with direct dependencies between microservices), and other services can continue to handle requests. Additionally, when an issue is resolved, you can deploy just the affected microservice without impacting the other ones.

### Release risk reduction
It helps avoid BIG and RISKY releases that, if gone wrong, could affect the whole organisation. The microservices approach allows agile changes for each microservice, because you can change specific, small areas of the overall system with reduced risk of breaking other things.

### Continuous Delivery
Architecting fine-grained microservices-based applications enables continuous integration and continuous delivery practices. It also accelerates delivery of new functions into the application.

Fine-grained composition of applications allows you to run and test microservices in isolation, and to evolve them autonomously while maintaining clear contracts between them. As long as you do not change the interfaces or contracts, you can change the internal implementation of any microservice or add new functionality without breaking other microservices. 

### Scalability
As an additional benefit, microservices can scale out independently. Instead of having a single monolithic application that you must scale out as a unit, you can instead scale out specific microservices. That way, you can scale just the functional area that needs more processing power or network bandwidth to support demand, rather than scaling out other areas of the application that do not need to be scaled. That means cost savings because you need less hardware.

### Development management
As each microservice is relatively small, it’s  easy to manage and evolve. It is easy for a new developer to understand and get started quickly with good productivity. Also multiple parallel teams can work on different parts of a larger system without getting in each other’s way. They can each have more autonomy for developing, testing and releasing their parts.

It will be easier for the developers to ​switch between different projects as they don’t have to know the entire project in order to contribute. They just need to master a microservice they're assigned to. This can improve collaboration, job satisfaction, exposure to a more diverse range of projects and learning opportunities, and feeling always productive and energised.

### Reusability
There are repeated requirements across many applications. Having an independent source code repository per microservice allows us to reuse them for different projects which decreases the development time as well as QA time. Overtime you can end up with a rich library of reusable microservices that can be quickly added to new projects to cut time and costs.

Examples of general purpose services are: Email sending and templates, SMS notifications, Audit Trail (including UI), user management, content blocks and micro-CMS,  various third party integrations, etc.

### New technology evaluation
The inherent autonomy of microservices makes it easier and less risky for the team to try new emerging technologies within a single service only. In the worst case scenario, if that technology proved to be a failure, that single service can be rewritten, which will be a lot easier due to smaller size.

### Technology migration and upgrades
Major technology migrations are always a pain in large projects, and often impractical. With microservices, you get the ability to reduce the scope of the migration to one service at a time. This allows the overall project to be gradually moved to the new tech stack without disrupting the business. 

### Requirements of large organisations
Big companies, with mature IT departments, are generally moving towards the microservices architecture for their products and business applications. When smaller software houses and consultancy firms also adopt this architecture, their skills, knowledge and frameworks will make them more appealing and a better fit for a larger client base.

## Downsides of microservices
A microservice based solution also has some drawbacks

### Distributed application
Distributing the application adds complexity. You must implement interservice communication using protocols like HTTP or AMQP, which adds complexity for testing and exception handling. It also adds latency to the system. Fortunately, the Olive framework takes away some of the complexity in implementing this.

### Deployment complexity
Having lots of small applications means a high degree of deployment complexity for IT operations and management. Unless you use a suitable DevOps infrastructure to automate deployments, that additional complexity can require more manual efforts than the development itself!

### Increased server resource needs
When  you replace a monolith with microservices, the combined amount of hosting resources (memory, drives, and network resources) needed by the new system will be larger than the one needed by the original monolithic application. However, given the low cost of resources in general and the business agility benefits of microservices it’s usually a good tradeoff for large, long-term applications. 

## Microservice Size vs Business Capability
What size should a microservice be? When developing a microservice, size should not be the important point. Instead, the important point should be to create loosely coupled services so you have autonomy of development, deployment, and scale, for each service.

Each microservice implements a specific end-to-end domain or business capability within a certain context boundary, and each must be developed autonomously and be deployable independently. For example you may have one microservice for Invoicing, one for Quotation, one for Rota management, etc.

Of course, when identifying and designing microservices, you should try to make them as small as possible as long as you do not have too many direct dependencies with other microservices. More important than the size of the microservice is the internal cohesion it must have and its independence from other services.

## Related concepts and technologies

### Microservices is SOA done right
Service-oriented architecture (SOA) was an overused term and has meant different things to different people. But as a common denominator, SOA means that you structure your application by decomposing it into multiple services (most commonly as HTTP services). 

At its core, Microservices comes from some similar ideas as SOA. Some people argue that “The microservice architecture is SOA done right.” 

Traditional SOA included some “big brother” features like Enterprise Service Bus (ESB), big central brokers, and central orchestrators at the organization level. Such central components are considered anti-patterns in the microservice community.

### Microservices takes "Bounded Context" (in DDD) deeper
The concept of microservice also derives from the [Bounded Context (BC) pattern](https://martinfowler.com/bliki/BoundedContext.html) in domain-driven design (DDD). DDD deals with large models by dividing them into multiple BCs that are each explicit about their boundaries. Each BC must have its own model and business entity classes. 

In the general sense of DDD such partitioning is logical and doesn’t have to be physical. It relates to the internal architecture of your application. You can implement that architecture even within a monolith application. A microservice takes things further to the physical layer. It is a Bounded Context that is built and deployed as a **separate process** for each Bounded Context. 

### .Net Core is ideal for Microservices 
You can use almost any technology, including the traditional .NET Framework, for building microservices-based applications (with or without containers - by using plain processes).  But remember that a microservice is meant to be as small as possible, to be light when spinning up, to have a small footprint, to be able to start and stop fast. For those requirements, you will want to use small and fast-to-instantiate container images.

.NET Core is the best candidate if you are embracing a microservices-oriented system that is based on containers. It is very lightweight. Its host container images, either based on Linux or the Windows Nano, are lean and small. Especially with Linux based containers, you can run your system with a much lower number of servers, hence saving costs in infrastructure and hosting. 

### Containers are great for hosting Microservices
In the container model, normally each container image represents a single microservice. You will see an [ENTRYPOINT](https://docs.docker.com/engine/reference/builder/#entrypoint) definition in the Dockerfile. This defines the process whose lifetime controls the lifetime of the container (typically that’s the Kestrel web server, starting your website.dll). When the process completes, the container lifecycle ends. Containers might represent long-running processes like web servers, but can also represent short-lived processes like batch jobs. 

**What if the container is terminated due to the app crashing? Does it mean your microservice goes down?**
Well, if the process fails, the container ends, and the orchestrator (e.g. Kubernetes) takes over. If the orchestrator was configured to keep five instances running and one fails, the orchestrator will create another container instance to replace the failed process. 

You might find a rare scenario where you want multiple processes running in a single container. For that scenario, since there can be only one entry point per container, you could run a script within the container that launches as many programs as needed. But the lifetime of the container is bound to the original (entry point) process irrespective of any additional processes that might also be running.

### Note: Containerization is applicable on monolithic apps too
You might want to build a single, monolithically deployed web application or service and deploy it as a container. Deployment to the various hosts can be managed with traditional deployment techniques. Docker hosts can be managed with commands like `docker run or docker-compose` performed manually, or through automation such as continuous delivery (CD) pipelines using Kubernetes and Jenkins.

There are benefits to using containers to manage monolithic application. 

- Scaling container instances is faster and easier than deploying additional VMs, which take time to start. 
- Deploying updates as Docker images is faster and more network efficient. 
- Docker images typically start in seconds, which speeds rollouts. Tearing down a Docker image instance is as easy as issuing a docker stop command, that completes in less than a second
- Containers are immutable by design, you never need to worry about corrupted VMs. In contrast, update scripts for a VM might forget to account for some specific configuration or file left on disk.
- Containers can be deployed with container orchestrators such as Kubernetes, which manage the various instances and lifecycle of each container instance. 

