# Cluster setup (Kubernetes and AWS)

## What is Kubernetes
For running a containerized application you need to run all the containers for the system to be fully functional. Generally speaking each container has a main process and if that process stops the whole container will stop which means the service that was running in the container will not be accessible. The only way to have that service back up and running is to run its container by running "docker run". However, this approach is not practical at all as you constantly have to monitor your containers and keep running different commands to make sure your application remains running. This process can be automated using a container orchestration. [Kubernetes](https://kubernetes.io/docs/concepts/overview/what-is-kubernetes/) is a very popular open source container orchastrator developed by google. 
## Kubernetes Concepts
Below is the list of elements we need to understand to be able to run and manage an application on Kubernetes.

#### Cluster
For an application to run on Kubernetes we need to create a cluster which wraps all the Kubernetes elements required to keep all the application containers up and running. 

#### Node
Nodes are the actual servers (bear metal or VM) that host containers. Each cluster can have one or more nodes.

#### Master
The Kubernetes core elements run on a master. For high availability you can set up more than one master in your cluster and Kubernetes will distribute its jobs among the running masters.

#### States
For running an application you plan to run a certain number of instances of each service. For example you want your application to have 3 running instances of Service A and only one running instance of Service B. That is called your Desired State. When you set up your application on Kubernetes, you specify what your desired state is. There is another state called current State, which is the state of your cluster when your application is running on it. When the application is running, for one reason or another you may lose some containers, which results in your current state not to match your desired state. Kubernetes constantly monitors the cluster and trys to manage the resources to bring the current state to the desired state.

#### Pod
Pod is the runtime host of a container. All the environment elements (i.e. environment variables) that a container works with come from the hosting pod. When you create a container image you often want to specify how much resource that container is allowed to use which can be specified in the pod definition. Like a normal virtual machine, when running, each pod will be allocated a cluster level IP address which can be used to connect to it.

#### Deployments
For making sure that a pod is up and running, how many of it should run at the same time or even where they should be running relative to other pods we can use deployments. In each deployment you specify the information (i.e. docker image, port binding, startup arguments, environment  variables ...) kubernetes needs when creating an instance of the pod.

#### Services
Containers run in pods, deployments make sure pods run as planned, but how do we access the containers? Yes you can use pod ip addresses but they change everytime a new pod is created. Kubernetes Services are abstractions for pods. You can define a service and map it to the pods that run specific containers.

#### Labels
Each resource (pod, deployment, service etc) in 


#### Selectors



#### Kubectl
...

## AWS account setup

## ...

TODO: https://docs.google.com/document/d/1CRvhWy5uN3dIw-agmqTjhdl8aC4bkWYsFPS45XLWick/edit

## Application Node role
The role of the Node which is created by Kubernetes. The EC2 servers natively have this role. This role should have the permission to assume other roles in general. Based on the following policy:
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "VisualEditor0",
            "Effect": "Allow",
            "Action": "sts:AssumeRole",
            "Resource": "*"
        }
    ]
}
```
