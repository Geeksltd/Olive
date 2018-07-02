# Cluster setup (Kubernetes and AWS)

## What is Kubernetes

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
