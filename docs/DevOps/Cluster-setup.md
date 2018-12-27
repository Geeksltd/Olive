# Cluster setup (Kubernetes and AWS)

## Introduction
This document describes what Kubernetes is and how it can be set up on AWS and Azure. We will try to use a simple imaginary inventory application as a case study to run using Kubernetes to simplify the learning process.

Our application is a microservice based, containerized system. It consists of two services, `Product Management` and `Stock Reportin`g. We assume that the `Project Management` service will not be used a lot, but the `Stock Reporting` (which is a heavy process) is used frequently. 

## What is Kubernetes
For smooth running of a containerized application, you need to run all the containers for the system to be fully functional. Generally speaking, each container has a main process and if that **process stops, the whole container will stop**, which means the service that was running in the container will not be accessible. The only way to have that service back up and running is to run its container by running "docker run". However, this approach is not practical, as you will have to constantly monitor your containers and keep running different commands to make sure your application remains running! 

To automate this process instead, you need a `container orchestration` system. [Kubernetes](https://kubernetes.io/docs/concepts/overview/what-is-kubernetes/) is a very popular open source container orchastrator developed by Google. 

## Kubernetes Concepts
Below is the list of elements we need to understand to be able to run and manage an application on Kubernetes.

#### Cluster
For an application to run on Kubernetes, we need to create a `cluster` which is a grouping of servers and other bits. A cluster wraps all the Kubernetes elements required to keep the application containers up and running.

#### Node
Nodes are the `actual servers` (typically Virtual Machines) that host containers. Each cluster can have one or more nodes.

#### Master
Just like nodes, masters are special servers (often VM) used to run Kubernetes itself. In order to manage the state of a cluster there should be [some processes](https://kubernetes.io/docs/concepts/overview/components/#master-components), with different responsibilities and taks, running all the time. They monitor the cluster and take actions (remove, add or update elements) accordingly, in order to controll the overal state of the cluster. These process run on masters.

For high availability, you can set up more than one master in your cluster, and Kubernetes will distribute its jobs among the running masters. When connecting to the cluster using the Kubernetes CLI (Command Line Interface) you actually connect to a master.

#### States
Let's say for our `inventory management` application, we know that the product management service will not be used a lot. So we decide that a single running container will be enough for it. But for the `service management`, that has higher demand, we want to run 3 containers at the same time. To achieve that, we just need to `plan a state that we desire our application to have` when running on Kubernetes. In Kubernetes, that state is called the `Desired state`.

Let's say we configured Kubernetes and our application is running in the state that we defined. After a while, for what ever reason, we may lose one or more containers. For example, let's assume we lose the `product management` container and one of the `stock reporting` containers, which means that we have lost our desired state. Kubernetes constantly monitors the cluster and tries to manage the resources to bring the *current state* to the `desired state` by creating or removing containers as needed.

#### Pod
If you are familiar with containers you should know what `Docker` is. For containers to work, there should be a container runtime. Docker is a container runtime.

Kubernetes supports different container runtimes. By default it uses Docker, but *you can use the other options if neccessary*. `Pod` is the Kubernetes abstraction for a container, plus some more kubernetes related specifications such as `labels` (described later), `name`, etc.

For example, for a container image you often want to specify `how much resource` that container is allowed to use. That can be specified in the `pod definition`. All the environment elements (i.e. environment variables), used by an application running in a pod, come from the hosting pod.

Like a normal virtual machine, when running, each pod will be allocated a `cluster level IP address` which can be used to connect to it. For the desired state of the `Inventory applciation` described in the previous section, we would have a pod running the `product management` container and 3 pods running the `stock reporting` contianers.

#### Deployments
In the `pod definition` we defined the `runtime specification` of our containers. In `deployments` we define how many of those pods we want running.

For example, based on the desired state of the `inventory application`, we have to configure Kubernetes to ensure one `reporting service` and 3 `stock reporting` service pods run all the time. This configuration is defined in the form of `deployments`. We need to define two different deployments.

Deployments encapsulate pod definitions as well as the `replication specification` of that pod. Based on the replication specification, each deployment can create zero or more pods. An example of a deployment is shown below :

```yaml
apiVersion: extensions/v1beta1
 kind: Deployment
 metadata:
   name: StockReporting
 spec:
   replicas: 3
   template:
     metadata:
       labels:
         microservice: stock-reporting
     spec:
       containers:
         - name: cnt-stock-reporting
           image: The url of the container image.           
           ports:
           - containerPort: 9376
             
```

As you can see in this example, we have created a deployment called `StockReporting` (read from `metadata.name`), specified that we need 3 running pod (read from `spec.replicas`) and defined the pod spec in `spec.spec` section. When added to Kubernetes, 3 pods will be created from the container image specified in `spec.spec.image`. It also assigns a label to the pods, read from `spec.metadata.labels`, which will be described later.

#### Services
Containers run in pods. Deployments make sure pods run as planned. But, how do we access the containers? Yes, you can use the `pod ip addresses` directly, but they change everytime a new pod is created.

The `stock reporting` service in the inventory application, has to talk to the `product management` service to get the product information available in stock. We wouldn't want to use the ip address of the pod which runs the product management service. For some reason, if the `product management` pod stops, and a new pod is created for it, the new pod will have a new virtual ip address and the existing `stock management` services will not be able to connect to the new pod.

> To solve that problem, Kubernetes has introduced a concept called `service`. You can define a service, give it a name, a type and a pod selector, which is used by kubernetes to `search among the running pods` and map the matching ones to the service. Once mapped, you can use the service name, as opposed to a hard-coded ip address, to connect to pods.

Below is the definition of the `StockManagement` service in our inventory application on Kubernetes:

```yaml
kind: Service
apiVersion: v1
metadata:
  name: StockReporting
spec:
  selector:
    microservice: stock-reporting
  ports:
  - protocol: TCP
    port: 80
    targetPort: 9376

```
The template above creates a service named `StockReporting` (read form `metadata.name`) for the deployment we created in the previous section. Notice the `spec.selector`, it matches `spec.template.metadata.labels`. We will describe labels and selectors in more details in the next section. The service specification also tells Kubernetes to redirect the coming TCP traffic to `StockReporting:80` to the `9376 port` of its bound pods (defined in `spec.ports`).


#### Labels and Selctors
Kubernetes resources get allocated a unique id when added to the cluster. Howerver, using long ids are difficult to remember and use. Also, normally when there are more than one instance of a resource (i.e. pods), it is hard to refere to them by their ids when managing them.

Kubernetes uses `labels` as a way to identify and query resources. In the previous example of a deployment template file, we specified a label, called `microservice`. When Kubernetes creates new pods using that deployment template, it assigns that label to them. Later on, if you want to find all the pods created for the `stock reporting` service you can query the `microservice` label and pass `"stock-reporting"`.

Labels enable Kubernetes resources to find other resources too. For example, in our previous example of a Service specification, the `spec.selector` part of the template specified `"microservice: stock-reporting"`. That configuration tells kubernetes to search for all the pods with that label/value and bind them to the service.


### Kubectl
Previously, we mentioned that the Kubernetes core components run on `master` nodes. To be able to manage the cluster, we need to send commands to the master node. The way to do that is to use the native Kubernetes command-line tool called `kubectl`.

There are different ways to install this on Windows. For example you can use [Chocolatey](https://chocolatey.org/) by running `_choco install kubernetes-cli_`. Other installation options have been discussed [here](https://kubernetes.io/docs/tasks/tools/install-kubectl/).


## AWS account setup
Earlier in this article we mentioned that Kubernetes needs some servers, to run as masters and nodes, to be able to function. There are different ways to create and manage servers for Kubernetes but for our environment we chose to use AWS. AWS is a well known IaaS provided in the market which provides some cloud computing features such as scailability, availability, security, good logging and monitoring systems that our production environment can benefit from. Compared to the other could providers we have more experience with AWS and that's another reason why we chose it.

### Installation
There are two ways we can set up our infrastructure on AWS for Kubernetes. One option is to create all the servers on AWS and install Kubernetes on them manually. But this is a very complicated and time consuming process.

Alternatively, you can use `Kops` which will automate that process. We will use Kops in this guide.

Or, you can use the more advanced [Terraform](https://www.terraform.io/) tool, to implement a more mature `infrastructure as code` architecture.

### Kops
Kops helps you create, destroy, upgrade and maintain Kubernetes clusters from the command line. With a single command line and passing some configuration arguments kops can create a Kubernetes cluster.

### Installation
At the time of creating this document, kops only works on Linux and there is no native support for Windows. Howerver, you can run it on a container, using [this docker file](https://github.com/kubernetes/kops/blob/master/docker/Dockerfile-light) (not tested).

#### Linux
You either need to have a machine with Linux running on it, or run a Linux virtual machine on windows. For the latter you can use the instrcution provided [here](https://www.windowscentral.com/how-run-linux-distros-windows-10-using-hyper-v). Once you install the virtual machine, you can install kops using the instruction [here](https://kubernetes.io/docs/setup/custom-cloud/kops/).

#### Creating the Cluster
For our current production environment we are planning to have one master and 3 worker nodes. We want the worker nodes to be in different availability zones so that if one availability zone goes down the other nodes will be up and host our pods. Below is the command which creates our cluster :
```powershell 
kops create cluster app.geeks.ltd --node-count 3 --zones eu-west-1a,eu-west-1b,eu-west-1c --master-size t2.medium --master-count 1 --node-size t2.small --cloud aws  --master-zones eu-west-1a --dns-zone app.geeks.ltd --yes
```
The above command creates :
* Creates the cluster in eu-west-1 region on AWS
* 3 X t2.small EC2 instanses as the worker nodes in eu-west-1a,eu-west-1b,eu-west-1c availability zones
* 1 X t2.medium EC2 instance as the master node in eu-west-1a availability zones
* Under the hood to make sure we always have a running master node in eu-west-1a AZ it creates an AutoScailing Group on AWS with min and max number of instances set 1. That essentially re-create the master node if for any reason the running one stops. It does the same thing for the 3 worker nodes to make sure we always have three instances running.



***kops stores the state of the infrastructure in S3. It is important not to change the infrastructure directly on AWS. Changes should be made by kops to make sure the state of the infrastructure is valid and up to date. Any inconsistency between the infrastructure and its metadata in kops can cause serious problems. Some changes may not cause problems but it is recommended to double check everything with the system admin first.***


## Azure
Almost everything we said about the Kubernetes cluster for AWS applies to Azure too. However, to install the cluster we need to use a different approach which is explained below:

### Azure Cli (az)
A common way to manage azure resouces is to use the Azure Cli. You can obviously do everything via the Azure console but this instruction uses the Azure cli for easier/better documentation.
[Here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest) is the instructions for setting up the cli.

### Resource Group
We start off by creating a resource group. A [resource group](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-overview) is a container to hold all related Azure resources together. Without resource groups managing resources will be very complicated. Imagine you want to add a couple of VMs temporarily for testing, and have to add a couple of other resources to work with them. If you already have a lot of resources in your account, your new resources will be lost among the existing ones and it will be hard to find them, specially if you want to delete everything after your test is done. With resource groups you can create one group and add all related resources to that. Deleting the resource group will remove all the resources associated with it which makes the clean up process very easy and straight forward. 

```powershell
az group create --name ResourceGroupName --location Locatioin
```
Description :
 - Command : Creates a resource group in your Azure account.
 - Parameters : 
   - name : The name of the resource group (set to ResourceGroupName in the commnad)
   - location : The azure region where all the resources will be created.
   
### Creating the cluster
Once you have the resource group created you can now create the cluster by running:

```powershell
az aks create --resource-group ResourceGroupName --name ClusterName --node-count 1 --generate-ssh-keys --os-type Windows
```
Description:
 - Command : Creates a kubernetes cluster in the Azure account. It takes a few minutes for all the resources to be created and ready.
 - Parameters : 
   - resource-group : The name of the resource group in which kubernetes resources will be created.
   - name : The name of the cluster
   - node-count : The number of nodes to be created in the cluster
   - os-type : The operating system tye of the images running in the cluster.
   
### Connect to the cluster
To be able to connect to the cluster we need to switch kubectl to point to the new cluster. To do that we need to get the cluster credentials to be used to connect. We can use the Azure cli to get the credentials using the command below:

```powershell
az aks get-credentials --resource-group ResourceGroupName --name ClusterName
```

Description:
 - Commnad : Get the credentials needed for connecting to the cluster. This command automatically switches kubectl context to point to this cluster. 
 - Parameters :
   - resource-group : The name of the resource group which holds the cluster resources.
   - name : The name of the cluster.  


In order to make sure everything is set up correctly you can run ``` kubectl get nodes ```. This should give you all the nodes running in your cluster.

------------------

TODO: https://docs.google.com/document/d/1CRvhWy5uN3dIw-agmqTjhdl8aC4bkWYsFPS45XLWick/edit
