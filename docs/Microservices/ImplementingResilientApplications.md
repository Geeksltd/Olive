# Implementing Resilient Applications
Failure will certainly happen in a cloud environment. There will be network outages, hardware problems, Windows updates and restarts, and Container or VMs crashing. 

Resiliency is not about avoiding failures, but accepting the fact that failures will happen. It's about responding to them in a way that avoids downtime or data loss, and returning the application to a fully functioning state automatically.

## Handling partial failure
In distributed systems each individual microservice may fail at any moment. In the meantime if a dependant service were to use synchronous HTTP calls to it with no timeout, it could end up blocking the thread indefinitely and eventually bring that service down too.

In microservices-based application, if most of the internal interaction is based on synchronous HTTP calls, any partial failure can be amplified and spread like a virus, taking the whole system down.

![partial failure](https://user-images.githubusercontent.com/1321544/50514886-623cfb00-0ab6-11e9-912d-0aaf727b3020.jpg)

Strategies for dealing with partial failures include the following. 

### Use asynchronous communication
Eventual consistency and event-driven architectures will help to minimize ripple effects and result in better microservice autonomy. 

### Use timeouts
In general, clients should be designed not to block indefinitely and to always use timeouts when waiting for a response.

### Use retries with exponential backoff
If the original failure was caused by short time or intermittent issue (such as temporary container downtime, restart, heavy load, etc) then retrying the call could be an easy solution. 

[Retries with exponential backoff](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry) means attempts to retry an operation with an exponentially increasing wait time, until a maximum retry count has been reached (the [exponential backoff](https://en.wikipedia.org/wiki/Exponential_backoff)). The idea is that if the source cause of the failure is overload, giving the remote service some breathing time will more likely enable it to recover and respond, while sending too many retry requests quickly would add insult to its injury.

### Use a Circuit Breaker

It's important to not get into an infinite retry loop, As part of the framework, the client process should track the number of failed requests. If the error rate exceeds a configured limit, a "circuit breaker" kicks in so that further attempts fail immediately without even trying to make a remote call.

The idea is that if a large number of requests are failing,  the service is probably unavailable for a period of time, and that sending more requests will be pointless. After a timeout period though, it would try again and, if the new requests are successful, close the circuit breaker. 

### Provide fallbacks
In this approach, the client process performs fallback logic when a request fails, such as returning cached data or a default value. This is an approach suitable for queries, and is more complex for updates or commands. 

### Implementing retries with Polly 
The recommended approach for retries with exponential backoff is to take advantage of the open-source [Polly](https://github.com/App-vNext/Polly) library. It provides resilience and transient-fault handling capabilities through declarative policies. Poly is already integrated in the Olive ApiClient library.

## Resiliency via Containerization
In today's world of cloud computing, a microservice needs to be resilient to system failures. In a containerized world, this means being able to restart on another machine very quickly. 

As service failures are partial and temporary, on the other end, client apps or services must have a strategy to retry sending messages or requests. This is explained later. 

### Infrastructure orchestration via Kubernetes
Managing the infrastructure in a microservice based system with potentially tens of servers and services each with different release cycles is complex. 

The plain Docker Engine can manage a single image instances on one host, but it falls short when it comes to managing multiple containers deployed on multiple hosts. 

You need a **management platform** or orchestrator, such as Kubernetes, which can automatically start containers, monitor their health,  scale them out to multiple instances per image (**clusters**), suspend them or shut them down when needed, and ideally also control how they access resources like the network and data storage.

Kubernetes is the leading orchestrator platform supported by all Cloud vendors. It is open-source, and managed by Google. It lets you automate deployment, scaling, and operations of application containers across clusters of hosts. In the .Net community Kubernetes is the de facto standard. 

### Diagnostics and log messages
Logs provide information about how an app or service is running, including exceptions, warnings, or simple informational messages.

In monolithic applications, you can simply write logs to a file or database and then analyze it with any tool. Since application execution is limited to a fixed server or VM, it generally is not too complex to analyze the flow of events. 

However it's different in distributed systems. A microservice should not store the logs by itself. Instead a central log service should be used via its API by all Microservices. 

## Health monitoring 
Health monitoring provides information about the state of your services in real time. Microservices-based applications often use heartbeats, or "I am Alive" signals, to enable interested observers to keep track of them. 

Interested observers often include external performance monitoring tools that aggregate the health signals to provide an overall view of the state of your application. 

Another typical group of observers are schedulers, and container orchestrators (e.g. Kubernetes) that take care of automatic scaling, failover recovery, and server setup, as part of the cloud hosting infrastructure. 

### Using watchdogs 
You can set up external web application health monitoring agents to periodically ping your web based services or apis, and notify you if it's down. There are several SaaS based solutions such as Uptime Robot, which you can use for this purpose. 

### Health checks and orchestrators 
Orchestrators, like Kubernetes, periodically send health check requests to test each microservices or container. If it fails, Kubernetes will stop routing requests to that instance, and usually creates a new instance of that container. 

Health monitoring is especially important during application upgrades. When you have multiple containers running for the same image (for scalability or high availability) Kubernetes can update services in phases. For example, it might update one-fifth of the cluster nodes first. Then if the health checks were successful it can roll the update out to the rest. 