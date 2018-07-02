# Cluster setup (Kubernetes and AWS)

## What is Kubernetes

## AWS account setup

## ...



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
