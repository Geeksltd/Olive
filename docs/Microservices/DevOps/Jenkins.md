# Continious Integration
`Continious Delivery` and `DevOps` have many well known operational benefits for the process of software development and management. But they are even more vital in a Microservice-oriented architecture. Why? Because with Microservices, you will typically have `10s of deployments per day`, after every small bug fix, or new feature. That makes it impossible to go about it manually.

`Continious deployment` enables us to rapidly deploy changes without the risk of `human errors`. Once designed and tested, you can trust the deployment process way more than any developer, even the most experienced ones.

Also, `scripting the deployment process` is a form of `documenting` it, that enables anybody to learn and do the process easily.


## Jenkins
There are many tools out there for deployment automation but we have chosen `Jenkins` because:

- It is one of the most popular tools, which means there is a good community support for it.
- It is open source, so we don't have to pay for anything.
- It has a rich library of plugins for different build and deployment tasks.

Our build process has a fairly complex structure and chosing Jenkins has been nothing but benefitial for us (hopefully it will remain that way). 
